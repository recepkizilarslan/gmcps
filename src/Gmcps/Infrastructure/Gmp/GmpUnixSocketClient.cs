using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;
using Gmcps.Domain;
using Gmcps.Domain.Configuration;
using Gmcps.Domain.Interfaces;
using Gmcps.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gmcps.Infrastructure.Gmp;

/// <summary>
/// GMP client that connects directly to gvmd's Unix domain socket.
/// Used when the MCP server runs in a container that shares the gvmd socket volume.
/// </summary>
public sealed class GmpUnixSocketClient : IGmpClient, IDisposable
{
    private readonly GvmOptions _options;
    private readonly ILogger<GmpUnixSocketClient> _logger;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private Socket? _socket;
    private NetworkStream? _stream;
    private bool _authenticated;
    private const int MaxResponseBytes = 4 * 1024 * 1024;

    public GmpUnixSocketClient(IOptions<GvmOptions> options, ILogger<GmpUnixSocketClient> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    private async Task EnsureConnectedAsync(CancellationToken ct)
    {
        if (_socket?.Connected == true && _authenticated)
        {
            return;
        }

        _stream?.Dispose();
        _socket?.Dispose();

        _socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        var endpoint = new UnixDomainSocketEndPoint(_options.SocketPath);
        await _socket.ConnectAsync(endpoint, ct);
        _stream = new NetworkStream(_socket, ownsSocket: false);

        var authResult = await SendCommandAsync(
            $"<authenticate><credentials><username>{EscapeXml(_options.Username)}</username><password>{EscapeXml(_options.Password)}</password></credentials></authenticate>",
            ct);

        if (authResult.IsFailure)
        {
            throw new InvalidOperationException($"GMP authentication failed: {authResult.Error}");
        }

        _authenticated = true;
        _logger.LogInformation("Connected and authenticated to GVM via Unix socket {SocketPath}", _options.SocketPath);
    }

    private async Task<Result<XDocument>> SendCommandAsync(string command, CancellationToken ct)
    {
        try
        {
            var bytes = Encoding.UTF8.GetBytes(command);
            await _stream!.WriteAsync(bytes, ct);
            await _stream.FlushAsync(ct);

            var response = await ReadResponseAsync(ct);
            var doc = XDocument.Parse(response);

            var status = doc.Root?.Attribute("status")?.Value;
            if (status is not null && !status.StartsWith("2"))
            {
                var statusText = doc.Root?.Attribute("status_text")?.Value ?? "Unknown error";
                return Result<XDocument>.Failure($"GMP error {status}: {statusText}");
            }

            return Result<XDocument>.Success(doc);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GMP command failed");
            _authenticated = false;
            return Result<XDocument>.Failure($"GMP communication error: {ex.Message}");
        }
    }

    private async Task<string> ReadResponseAsync(CancellationToken ct)
    {
        var buffer = new byte[8192];
        var sb = new StringBuilder();
        var started = false;

        while (true)
        {
            ct.ThrowIfCancellationRequested();
            var bytesRead = await _stream!.ReadAsync(buffer, ct);
            if (bytesRead == 0)
            {
                throw new IOException("Connection closed by GVM");
            }

            var chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            sb.Append(chunk);

            foreach (var ch in chunk)
            {
                if (ch == '<')
                {
                    started = true;
                }
            }

            if (started && IsCompleteXml(sb.ToString()))
            {
                break;
            }

            if (sb.Length > MaxResponseBytes)
            {
                throw new InvalidOperationException("GMP response exceeded maximum allowed size");
            }
        }

        return sb.ToString();
    }

    private static bool IsCompleteXml(string xml)
    {
        try
        {
            XDocument.Parse(xml);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<Result<XDocument>> ExecuteCommandAsync(string command, CancellationToken ct)
    {
        await _connectionLock.WaitAsync(ct);
        try
        {
            await EnsureConnectedAsync(ct);
            return await SendCommandAsync(command, ct);
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    // ---- IGmpClient implementation ----

    public async Task<Result<string>> GetVersionAsync(CancellationToken ct)
    {
        var result = await ExecuteCommandAsync("<get_version/>", ct);
        return result.Map(doc => doc.Root?.Element("version")?.Value ?? "unknown");
    }

    public async Task<Result<IReadOnlyList<ScanConfig>>> GetScanConfigsAsync(CancellationToken ct)
    {
        var result = await ExecuteCommandAsync("<get_configs/>", ct);
        return result.Map(GmpXmlParser.ParseScanConfigs);
    }

    public async Task<Result<IReadOnlyList<Target>>> GetTargetsAsync(CancellationToken ct)
    {
        var result = await ExecuteCommandAsync("<get_targets/>", ct);
        return result.Map(GmpXmlParser.ParseTargets);
    }

    public async Task<Result<string>> CreateTargetAsync(string name, string hosts, string? comment, CancellationToken ct)
    {
        var xml = $"<create_target><name>{EscapeXml(name)}</name><hosts>{EscapeXml(hosts)}</hosts>";
        if (comment is not null)
        {
            xml += $"<comment>{EscapeXml(comment)}</comment>";
        }
        xml += "</create_target>";

        var result = await ExecuteCommandAsync(xml, ct);
        return result.Map(doc => doc.Root?.Attribute("id")?.Value ?? "");
    }

    public async Task<Result<string>> CreateTaskAsync(string name, string targetId, string scanConfigId, string scannerId, CancellationToken ct)
    {
        var xml = $"<create_task><name>{EscapeXml(name)}</name>" +
            $"<target id=\"{EscapeXml(targetId)}\"/>" +
            $"<config id=\"{EscapeXml(scanConfigId)}\"/>" +
            $"<scanner id=\"{EscapeXml(scannerId)}\"/>" +
            $"<usage_type>scan</usage_type></create_task>";

        var result = await ExecuteCommandAsync(xml, ct);
        return result.Map(doc => doc.Root?.Attribute("id")?.Value ?? "");
    }

    public async Task<Result<string>> StartTaskAsync(string taskId, CancellationToken ct)
    {
        var result = await ExecuteCommandAsync($"<start_task task_id=\"{EscapeXml(taskId)}\"/>", ct);
        return result.Map(doc => doc.Root?.Element("report_id")?.Value ?? "");
    }

    public async Task<Result<GvmTask>> GetTaskStatusAsync(string taskId, CancellationToken ct)
    {
        var result = await ExecuteCommandAsync($"<get_tasks task_id=\"{EscapeXml(taskId)}\" usage_type=\"scan\"/>", ct);
        return result.Bind(doc =>
        {
            var tasks = GmpXmlParser.ParseTasks(doc);
            return tasks.Count > 0
                ? Result<GvmTask>.Success(tasks[0])
                : Result<GvmTask>.Failure($"Task {taskId} not found");
        });
    }

    public async Task<Result<IReadOnlyList<GvmTask>>> GetTasksAsync(CancellationToken ct)
    {
        var result = await ExecuteCommandAsync("<get_tasks usage_type=\"scan\"/>", ct);
        return result.Map(GmpXmlParser.ParseTasks);
    }

    public async Task<Result<Report>> GetReportSummaryAsync(string reportId, CancellationToken ct)
    {
        var result = await ExecuteCommandAsync(
            $"<get_reports report_id=\"{EscapeXml(reportId)}\" details=\"0\"/>", ct);
        return result.Bind(doc => GmpXmlParser.ParseReportSummary(doc, reportId));
    }

    public async Task<Result<IReadOnlyList<Finding>>> GetReportFindingsAsync(string reportId, CancellationToken ct)
    {
        var result = await ExecuteCommandAsync(
            $"<get_reports report_id=\"{EscapeXml(reportId)}\" details=\"1\" ignore_pagination=\"1\"/>", ct);
        return result.Map(GmpXmlParser.ParseFindings);
    }

    private static string EscapeXml(string input) =>
        input.Replace("&", "&amp;")
             .Replace("<", "&lt;")
             .Replace(">", "&gt;")
             .Replace("\"", "&quot;")
             .Replace("'", "&apos;");

    public void Dispose()
    {
        _stream?.Dispose();
        _socket?.Dispose();
        _connectionLock.Dispose();
    }
}
