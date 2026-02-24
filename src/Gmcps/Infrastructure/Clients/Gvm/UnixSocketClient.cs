using System.Net.Sockets;
using System.Text;
using Gmcps.Configuration;
using Microsoft.Extensions.Options;

namespace Gmcps.Infrastructure.Clients.Gvm;

public sealed partial class UnixSocketClient(IOptions<GvmOptions> options, ILogger<UnixSocketClient> logger)
    : IClient, IDisposable
{
    private readonly GvmOptions _options = options.Value;

    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    private Socket? _socket;

    private NetworkStream? _stream;

    private bool _authenticated;

    private const int MaxResponseBytes = 4 * 1024 * 1024;

    public async Task<Result<XDocument>> SendCommandAsync(string command, CancellationToken ct)
    {
        await _connectionLock.WaitAsync(ct);
        try
        {
            await EnsureConnectedAsync(ct);

            var result = await SendCommandInternalAsync(command, ct);

            if (result.IsFailure)
            {
                throw new InvalidOperationException(result.Error);
            }

            return result;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async Task<Result<string>> GetVersionAsync(CancellationToken ct)
    {
        var result = await SendCommandAsync("<get_version/>", ct);
        return result.Map(doc => doc.Root?.Element("version")?.Value ?? "unknown");
    }

    public async Task<Result<IReadOnlyList<ScanConfig>>> GetScanConfigsAsync(CancellationToken ct)
    {
        var result = await SendCommandAsync("<get_configs/>", ct);
        return result.Map(ParseScanConfigs);
    }

    public async Task<Result<IReadOnlyList<Target>>> GetTargetsAsync(CancellationToken ct)
    {
        var result = await SendCommandAsync("<get_targets/>", ct);
        return result.Map(ParseTargets);
    }

    public async Task<Result<string>> CreateTargetAsync(string name, string hosts, string? comment, CancellationToken ct)
    {
        var xml = new StringBuilder();
        xml.Append("<create_target>");
        xml.Append($"<name>{EscapeXml(name)}</name>");
        xml.Append($"<hosts>{EscapeXml(hosts)}</hosts>");
        if (!string.IsNullOrWhiteSpace(comment))
        {
            xml.Append($"<comment>{EscapeXml(comment)}</comment>");
        }

        xml.Append("</create_target>");

        var result = await SendCommandAsync(xml.ToString(), ct);
        return result.Map(doc => doc.Root?.Attribute("id")?.Value ?? string.Empty);
    }

    public async Task<Result<IReadOnlyList<GvmTask>>> GetTasksAsync(CancellationToken ct)
    {
        var result = await SendCommandAsync("<get_tasks usage_type=\"scan\"/>", ct);
        return result.Map(ParseTasks);
    }

    public async Task<Result<IReadOnlyList<GvmTask>>> GetTasksAsync(int limit, string usageType, CancellationToken ct)
    {
        var safeLimit = NormalizeLimit(limit);
        var safeUsageType = string.IsNullOrWhiteSpace(usageType) ? "scan" : EscapeXml(usageType);
        var command = $"<get_tasks usage_type=\"{safeUsageType}\" filter=\"rows={safeLimit}\"/>";

        var result = await SendCommandAsync(command, ct);
        return result.Map(ParseTasks);
    }

    public async Task<Result<GvmTask>> GetTaskStatusAsync(string taskId, CancellationToken ct)
    {
        var command = $"<get_tasks task_id=\"{EscapeXml(taskId)}\" usage_type=\"scan\"/>";
        var result = await SendCommandAsync(command, ct);

        return result.Bind(doc =>
        {
            var tasks = ParseTasks(doc);
            return tasks.Count > 0
                ? Result<GvmTask>.Success(tasks[0])
                : Result<GvmTask>.Failure($"Task {taskId} not found");
        });
    }

    public async Task<Result<string>> CreateTaskAsync(
        string name,
        string targetId,
        string scanConfigId,
        string scannerId,
        CancellationToken ct)
    {
        var xml = $"<create_task><name>{EscapeXml(name)}</name>" +
                  $"<target id=\"{EscapeXml(targetId)}\"/>" +
                  $"<config id=\"{EscapeXml(scanConfigId)}\"/>" +
                  $"<scanner id=\"{EscapeXml(scannerId)}\"/>" +
                  "<usage_type>scan</usage_type></create_task>";

        var result = await SendCommandAsync(xml, ct);
        return result.Map(doc => doc.Root?.Attribute("id")?.Value ?? string.Empty);
    }

    public async Task<Result<string>> StartTaskAsync(string taskId, CancellationToken ct)
    {
        var result = await SendCommandAsync($"<start_task task_id=\"{EscapeXml(taskId)}\"/>", ct);
        return result.Map(doc => doc.Root?.Element("report_id")?.Value ?? string.Empty);
    }

    public async Task<Result<bool>> StopTaskAsync(string taskId, CancellationToken ct)
    {
        var result = await SendCommandAsync($"<stop_task task_id=\"{EscapeXml(taskId)}\"/>", ct);
        return result.Map(_ => true);
    }

    public async Task<Result<string>> ResumeTaskAsync(string taskId, CancellationToken ct)
    {
        var result = await SendCommandAsync($"<resume_task task_id=\"{EscapeXml(taskId)}\"/>", ct);
        return result.Map(doc => doc.Root?.Element("report_id")?.Value ?? string.Empty);
    }

    public async Task<Result<bool>> DeleteTaskAsync(string taskId, bool ultimate, CancellationToken ct)
    {
        var command = $"<delete_task task_id=\"{EscapeXml(taskId)}\" ultimate=\"{ToBoolAttribute(ultimate)}\"/>";
        var result = await SendCommandAsync(command, ct);
        return result.Map(_ => true);
    }

    public async Task<Result<Report>> GetReportSummaryAsync(string reportId, CancellationToken ct)
    {
        var command = $"<get_reports report_id=\"{EscapeXml(reportId)}\" details=\"0\"/>";
        var result = await SendCommandAsync(command, ct);
        return result.Bind(doc => ParseReportSummary(doc, reportId));
    }

    public async Task<Result<IReadOnlyList<Report>>> GetReportsAsync(int limit, string usageType, CancellationToken ct)
    {
        var safeLimit = NormalizeLimit(limit);
        var safeUsageType = string.IsNullOrWhiteSpace(usageType) ? "scan" : EscapeXml(usageType);
        var command = $"<get_reports usage_type=\"{safeUsageType}\" details=\"0\" filter=\"rows={safeLimit}\"/>";

        var result = await SendCommandAsync(command, ct);
        return result.Map(ParseReports);
    }

    public async Task<Result<IReadOnlyList<Finding>>> GetReportFindingsAsync(string reportId, CancellationToken ct)
    {
        var command = $"<get_reports report_id=\"{EscapeXml(reportId)}\" details=\"1\" ignore_pagination=\"1\"/>";
        var result = await SendCommandAsync(command, ct);
        return result.Map(ParseFindings);
    }

    public async Task<Result<bool>> DeleteReportAsync(string reportId, CancellationToken ct)
    {
        var result = await SendCommandAsync($"<delete_report report_id=\"{EscapeXml(reportId)}\"/>", ct);
        return result.Map(_ => true);
    }

    public async Task<Result<IReadOnlyList<ScanResultItem>>> GetResultsAsync(int limit, CancellationToken ct)
    {
        var safeLimit = NormalizeLimit(limit);
        var command = $"<get_results details=\"1\" filter=\"rows={safeLimit}\"/>";

        var result = await SendCommandAsync(command, ct);
        return result.Map(ParseResults);
    }

    public async Task<Result<IReadOnlyList<NoteItem>>> GetNotesAsync(int limit, CancellationToken ct)
    {
        var safeLimit = NormalizeLimit(limit);
        var command = $"<get_notes filter=\"rows={safeLimit}\"/>";

        var result = await SendCommandAsync(command, ct);
        return result.Map(ParseNotes);
    }

    public async Task<Result<string>> CreateNoteAsync(
        string text,
        string nvtOid,
        string? resultId,
        string? taskId,
        string? hosts,
        string? port,
        double? severity,
        int? activeDays,
        CancellationToken ct)
    {
        var command = BuildCreateNoteCommand(text, nvtOid, resultId, taskId, hosts, port, severity, activeDays);
        var result = await SendCommandAsync(command, ct);
        return result.Map(doc => doc.Root?.Attribute("id")?.Value ?? string.Empty);
    }

    public async Task<Result<bool>> ModifyNoteAsync(
        string noteId,
        string text,
        string? nvtOid,
        string? resultId,
        string? taskId,
        string? hosts,
        string? port,
        double? severity,
        int? activeDays,
        CancellationToken ct)
    {
        var command = BuildModifyNoteCommand(noteId, text, nvtOid, resultId, taskId, hosts, port, severity, activeDays);
        var result = await SendCommandAsync(command, ct);
        return result.Map(_ => true);
    }

    public async Task<Result<bool>> DeleteNoteAsync(string noteId, bool ultimate, CancellationToken ct)
    {
        var command = $"<delete_note note_id=\"{EscapeXml(noteId)}\" ultimate=\"{ToBoolAttribute(ultimate)}\"/>";
        var result = await SendCommandAsync(command, ct);
        return result.Map(_ => true);
    }

    public async Task<Result<IReadOnlyList<OverrideItem>>> GetOverridesAsync(int limit, CancellationToken ct)
    {
        var safeLimit = NormalizeLimit(limit);
        var command = $"<get_overrides filter=\"rows={safeLimit}\"/>";

        var result = await SendCommandAsync(command, ct);
        return result.Map(ParseOverrides);
    }

    public async Task<Result<string>> CreateOverrideAsync(
        string text,
        string nvtOid,
        double? newSeverity,
        string? resultId,
        string? taskId,
        string? hosts,
        string? port,
        double? severity,
        int? activeDays,
        CancellationToken ct)
    {
        var command = BuildCreateOverrideCommand(text, nvtOid, newSeverity, resultId, taskId, hosts, port, severity, activeDays);
        var result = await SendCommandAsync(command, ct);
        return result.Map(doc => doc.Root?.Attribute("id")?.Value ?? string.Empty);
    }

    public async Task<Result<bool>> ModifyOverrideAsync(
        string overrideId,
        string text,
        string? nvtOid,
        double? newSeverity,
        string? resultId,
        string? taskId,
        string? hosts,
        string? port,
        double? severity,
        int? activeDays,
        CancellationToken ct)
    {
        var command = BuildModifyOverrideCommand(overrideId, text, nvtOid, newSeverity, resultId, taskId, hosts, port, severity, activeDays);
        var result = await SendCommandAsync(command, ct);
        return result.Map(_ => true);
    }

    public async Task<Result<bool>> DeleteOverrideAsync(string overrideId, bool ultimate, CancellationToken ct)
    {
        var command = $"<delete_override override_id=\"{EscapeXml(overrideId)}\" ultimate=\"{ToBoolAttribute(ultimate)}\"/>";
        var result = await SendCommandAsync(command, ct);
        return result.Map(_ => true);
    }

    public async Task<Result<IReadOnlyList<HostAsset>>> GetHostAssetsAsync(int limit, CancellationToken ct)
    {
        var safeLimit = NormalizeLimit(limit);
        var command = $"<get_assets type=\"host\" filter=\"rows={safeLimit}\"/>";

        var result = await SendCommandAsync(command, ct);
        return result.Map(ParseHostAssets);
    }

    public async Task<Result<IReadOnlyList<OperatingSystemAsset>>> GetOperatingSystemAssetsAsync(int limit, CancellationToken ct)
    {
        var safeLimit = NormalizeLimit(limit);
        var command = $"<get_assets type=\"os\" filter=\"rows={safeLimit}\"/>";

        var result = await SendCommandAsync(command, ct);
        return result.Map(ParseOperatingSystemAssets);
    }

    public async Task<Result<IReadOnlyList<TlsCertificateAsset>>> GetTlsCertificatesAsync(int limit, CancellationToken ct)
    {
        var safeLimit = NormalizeLimit(limit);
        var command = $"<get_tls_certificates filter=\"rows={safeLimit}\"/>";

        var result = await SendCommandAsync(command, ct);
        return result.Map(ParseTlsCertificates);
    }

    public async Task<Result<IReadOnlyList<SecurityInfoEntry>>> GetSecurityInfosAsync(string type, int limit, CancellationToken ct)
    {
        var safeType = string.IsNullOrWhiteSpace(type) ? "CVE" : type.Trim().ToUpperInvariant();
        var safeLimit = NormalizeLimit(limit);
        var command = $"<get_info type=\"{EscapeXml(safeType)}\" filter=\"rows={safeLimit}\"/>";

        var result = await SendCommandAsync(command, ct);
        return result.Map(doc => ParseSecurityInfos(doc, safeType));
    }

    public async Task<Result<IReadOnlyList<NvtEntry>>> GetNvtsAsync(int limit, CancellationToken ct)
    {
        var safeLimit = NormalizeLimit(limit);
        var command = $"<get_nvts details=\"1\" filter=\"rows={safeLimit}\"/>";

        var result = await SendCommandAsync(command, ct);
        return result.Map(ParseNvts);
    }

    public async Task<Result<IReadOnlyList<GvmPortList>>> GetPortListsAsync(int limit, CancellationToken ct)
    {
        var safeLimit = NormalizeLimit(limit);
        var command = $"<get_port_lists filter=\"rows={safeLimit}\"/>";

        var result = await SendCommandAsync(command, ct);
        return result.Map(ParsePortLists);
    }

    public async Task<Result<IReadOnlyList<GvmCredential>>> GetCredentialsAsync(int limit, CancellationToken ct)
    {
        var safeLimit = NormalizeLimit(limit);
        var command = $"<get_credentials filter=\"rows={safeLimit}\"/>";

        var result = await SendCommandAsync(command, ct);
        return result.Map(ParseCredentials);
    }

    public async Task<Result<IReadOnlyList<GvmAlert>>> GetAlertsAsync(int limit, CancellationToken ct)
    {
        var safeLimit = NormalizeLimit(limit);
        var command = $"<get_alerts filter=\"rows={safeLimit}\"/>";

        var result = await SendCommandAsync(command, ct);
        return result.Map(ParseAlerts);
    }

    public async Task<Result<IReadOnlyList<GvmSchedule>>> GetSchedulesAsync(int limit, CancellationToken ct)
    {
        var safeLimit = NormalizeLimit(limit);
        var command = $"<get_schedules filter=\"rows={safeLimit}\"/>";

        var result = await SendCommandAsync(command, ct);
        return result.Map(ParseSchedules);
    }

    public async Task<Result<IReadOnlyList<GvmReportConfig>>> GetReportConfigsAsync(int limit, CancellationToken ct)
    {
        var safeLimit = NormalizeLimit(limit);
        var command = $"<get_report_configs filter=\"rows={safeLimit}\"/>";

        var result = await SendCommandAsync(command, ct);
        return result.Map(ParseReportConfigs);
    }

    public async Task<Result<IReadOnlyList<GvmReportFormat>>> GetReportFormatsAsync(int limit, CancellationToken ct)
    {
        var safeLimit = NormalizeLimit(limit);
        var command = $"<get_report_formats filter=\"rows={safeLimit}\"/>";

        var result = await SendCommandAsync(command, ct);
        return result.Map(ParseReportFormats);
    }

    public async Task<Result<IReadOnlyList<GvmScanner>>> GetScannersAsync(int limit, CancellationToken ct)
    {
        var safeLimit = NormalizeLimit(limit);
        var command = $"<get_scanners filter=\"rows={safeLimit}\"/>";

        var result = await SendCommandAsync(command, ct);
        return result.Map(ParseScanners);
    }

    public async Task<Result<IReadOnlyList<GvmFilter>>> GetFiltersAsync(int limit, CancellationToken ct)
    {
        var safeLimit = NormalizeLimit(limit);
        var command = $"<get_filters filter=\"rows={safeLimit}\"/>";

        var result = await SendCommandAsync(command, ct);
        return result.Map(ParseFilters);
    }

    public async Task<Result<IReadOnlyList<GvmTag>>> GetTagsAsync(int limit, CancellationToken ct)
    {
        var safeLimit = NormalizeLimit(limit);
        var command = $"<get_tags filter=\"rows={safeLimit}\"/>";

        var result = await SendCommandAsync(command, ct);
        return result.Map(ParseTags);
    }

    public async Task<Result<IReadOnlyList<GvmTicket>>> GetTicketsAsync(int limit, CancellationToken ct)
    {
        var safeLimit = NormalizeLimit(limit);
        var command = $"<get_tickets filter=\"rows={safeLimit}\"/>";

        var result = await SendCommandAsync(command, ct);
        return result.Map(ParseTickets);
    }

    public async Task<Result<string>> CreateTicketAsync(
        string resultId,
        string assignedToUserId,
        string openNote,
        string? comment,
        CancellationToken ct)
    {
        var xml = new StringBuilder("<create_ticket>");
        xml.Append($"<result id=\"{EscapeXml(resultId)}\"/>");
        xml.Append($"<assigned_to><user id=\"{EscapeXml(assignedToUserId)}\"/></assigned_to>");
        xml.Append($"<open_note>{EscapeXml(openNote)}</open_note>");
        if (!string.IsNullOrWhiteSpace(comment))
        {
            xml.Append($"<comment>{EscapeXml(comment)}</comment>");
        }

        xml.Append("</create_ticket>");

        var result = await SendCommandAsync(xml.ToString(), ct);
        return result.Map(doc => doc.Root?.Attribute("id")?.Value ?? string.Empty);
    }

    public async Task<Result<bool>> ModifyTicketAsync(
        string ticketId,
        string? status,
        string? openNote,
        string? fixedNote,
        string? closedNote,
        string? assignedToUserId,
        string? comment,
        CancellationToken ct)
    {
        var xml = new StringBuilder();
        xml.Append($"<modify_ticket ticket_id=\"{EscapeXml(ticketId)}\">");

        AppendOptionalElement(xml, "status", status);
        AppendOptionalElement(xml, "open_note", openNote);
        AppendOptionalElement(xml, "fixed_note", fixedNote);
        AppendOptionalElement(xml, "closed_note", closedNote);
        if (!string.IsNullOrWhiteSpace(assignedToUserId))
        {
            xml.Append($"<assigned_to><user id=\"{EscapeXml(assignedToUserId)}\"/></assigned_to>");
        }

        AppendOptionalElement(xml, "comment", comment);

        xml.Append("</modify_ticket>");

        var result = await SendCommandAsync(xml.ToString(), ct);
        return result.Map(_ => true);
    }

    public async Task<Result<bool>> DeleteTicketAsync(string ticketId, bool ultimate, CancellationToken ct)
    {
        var command = $"<delete_ticket ticket_id=\"{EscapeXml(ticketId)}\" ultimate=\"{ToBoolAttribute(ultimate)}\"/>";
        var result = await SendCommandAsync(command, ct);
        return result.Map(_ => true);
    }

    public async Task<Result<IReadOnlyList<GvmComplianceAuditReport>>> GetComplianceAuditReportsAsync(int limit, CancellationToken ct)
    {
        var safeLimit = NormalizeLimit(limit);
        var command = $"<get_reports usage_type=\"audit\" details=\"0\" filter=\"rows={safeLimit}\"/>";

        var result = await SendCommandAsync(command, ct);
        return result.Map(ParseComplianceAuditReports);
    }

    private async Task EnsureConnectedAsync(CancellationToken ct)
    {
        if (_socket?.Connected == true && _authenticated)
        {
            return;
        }

        logger.LogDebug("Establishing GMP socket connection to {SocketPath}", _options.SocketPath);

        _stream?.Dispose();
        _socket?.Dispose();

        _socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        var endpoint = new UnixDomainSocketEndPoint(_options.SocketPath);
        await _socket.ConnectAsync(endpoint, ct);
        _stream = new NetworkStream(_socket, ownsSocket: false);

        var authResult = await SendCommandInternalAsync(
            $"<authenticate><credentials><username>{EscapeXml(_options.Username)}</username><password>{EscapeXml(_options.Password)}</password></credentials></authenticate>",
            ct);

        if (authResult.IsFailure)
        {
            throw new InvalidOperationException($"GMP authentication failed: {authResult.Error}");
        }

        _authenticated = true;
        logger.LogInformation("Connected and authenticated to GVM via Unix socket {SocketPath}", _options.SocketPath);
    }

    private async Task<Result<XDocument>> SendCommandInternalAsync(string command, CancellationToken ct)
    {
        var commandName = ExtractCommandName(command);

        try
        {
            logger.LogDebug("Sending GMP command {CommandName}", commandName);
            var bytes = Encoding.UTF8.GetBytes(command);
            await _stream!.WriteAsync(bytes, ct);
            await _stream.FlushAsync(ct);

            var response = await ReadResponseAsync(ct);
            var doc = XDocument.Parse(response);

            var status = doc.Root?.Attribute("status")?.Value;
            if (status is not null && !status.StartsWith("2", StringComparison.Ordinal))
            {
                var statusText = doc.Root?.Attribute("status_text")?.Value ?? "Unknown error";
                logger.LogWarning("GMP command {CommandName} returned error status {Status}: {StatusText}", commandName, status, statusText);
                return Result<XDocument>.Failure($"GMP error {status}: {statusText}");
            }

            logger.LogDebug("GMP command {CommandName} completed successfully", commandName);
            return Result<XDocument>.Success(doc);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("GMP command {CommandName} was canceled", commandName);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GMP command {CommandName} failed", commandName);
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

            if (sb.Length > MaxResponseBytes)
            {
                throw new InvalidOperationException("GMP response exceeds maximum allowed size");
            }

            if (!started)
            {
                continue;
            }

            var content = sb.ToString();
            if (LooksLikeCompleteXml(content))
            {
                return content;
            }
        }
    }

    private static bool LooksLikeCompleteXml(string content)
    {
        try
        {
            _ = XDocument.Parse(content);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string BuildCreateNoteCommand(
        string text,
        string nvtOid,
        string? resultId,
        string? taskId,
        string? hosts,
        string? port,
        double? severity,
        int? activeDays)
    {
        var xml = new StringBuilder("<create_note>");
        xml.Append($"<text>{EscapeXml(text)}</text>");
        xml.Append($"<nvt oid=\"{EscapeXml(nvtOid)}\"/>");

        AppendOptionalReference(xml, "result", resultId);
        AppendOptionalReference(xml, "task", taskId);
        AppendOptionalElement(xml, "hosts", hosts);
        AppendOptionalElement(xml, "port", port);
        if (severity.HasValue)
        {
            xml.Append($"<severity>{FormatDouble(severity.Value)}</severity>");
        }

        AppendOptionalActiveWindow(xml, activeDays);

        xml.Append("</create_note>");
        return xml.ToString();
    }

    private static string BuildModifyNoteCommand(
        string noteId,
        string text,
        string? nvtOid,
        string? resultId,
        string? taskId,
        string? hosts,
        string? port,
        double? severity,
        int? activeDays)
    {
        var xml = new StringBuilder();
        xml.Append($"<modify_note note_id=\"{EscapeXml(noteId)}\">");
        xml.Append($"<text>{EscapeXml(text)}</text>");

        if (!string.IsNullOrWhiteSpace(nvtOid))
        {
            xml.Append($"<nvt oid=\"{EscapeXml(nvtOid)}\"/>");
        }

        AppendOptionalReference(xml, "result", resultId);
        AppendOptionalReference(xml, "task", taskId);
        AppendOptionalElement(xml, "hosts", hosts);
        AppendOptionalElement(xml, "port", port);
        if (severity.HasValue)
        {
            xml.Append($"<severity>{FormatDouble(severity.Value)}</severity>");
        }

        AppendOptionalActiveWindow(xml, activeDays);

        xml.Append("</modify_note>");
        return xml.ToString();
    }

    private static string BuildCreateOverrideCommand(
        string text,
        string nvtOid,
        double? newSeverity,
        string? resultId,
        string? taskId,
        string? hosts,
        string? port,
        double? severity,
        int? activeDays)
    {
        var xml = new StringBuilder("<create_override>");
        xml.Append($"<text>{EscapeXml(text)}</text>");
        xml.Append($"<nvt oid=\"{EscapeXml(nvtOid)}\"/>");

        if (newSeverity.HasValue)
        {
            xml.Append($"<new_severity>{FormatDouble(newSeverity.Value)}</new_severity>");
        }

        AppendOptionalReference(xml, "result", resultId);
        AppendOptionalReference(xml, "task", taskId);
        AppendOptionalElement(xml, "hosts", hosts);
        AppendOptionalElement(xml, "port", port);
        if (severity.HasValue)
        {
            xml.Append($"<severity>{FormatDouble(severity.Value)}</severity>");
        }

        AppendOptionalActiveWindow(xml, activeDays);

        xml.Append("</create_override>");
        return xml.ToString();
    }

    private static string BuildModifyOverrideCommand(
        string overrideId,
        string text,
        string? nvtOid,
        double? newSeverity,
        string? resultId,
        string? taskId,
        string? hosts,
        string? port,
        double? severity,
        int? activeDays)
    {
        var xml = new StringBuilder();
        xml.Append($"<modify_override override_id=\"{EscapeXml(overrideId)}\">");
        xml.Append($"<text>{EscapeXml(text)}</text>");

        if (!string.IsNullOrWhiteSpace(nvtOid))
        {
            xml.Append($"<nvt oid=\"{EscapeXml(nvtOid)}\"/>");
        }

        if (newSeverity.HasValue)
        {
            xml.Append($"<new_severity>{FormatDouble(newSeverity.Value)}</new_severity>");
        }

        AppendOptionalReference(xml, "result", resultId);
        AppendOptionalReference(xml, "task", taskId);
        AppendOptionalElement(xml, "hosts", hosts);
        AppendOptionalElement(xml, "port", port);
        if (severity.HasValue)
        {
            xml.Append($"<severity>{FormatDouble(severity.Value)}</severity>");
        }

        AppendOptionalActiveWindow(xml, activeDays);

        xml.Append("</modify_override>");
        return xml.ToString();
    }

    private static void AppendOptionalElement(StringBuilder xml, string elementName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        xml.Append($"<{elementName}>{EscapeXml(value)}</{elementName}>");
    }

    private static void AppendOptionalReference(StringBuilder xml, string elementName, string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        xml.Append($"<{elementName} id=\"{EscapeXml(id)}\"/>");
    }

    private static void AppendOptionalActiveWindow(StringBuilder xml, int? activeDays)
    {
        if (!activeDays.HasValue || activeDays.Value <= 0)
        {
            return;
        }

        var endTime = DateTime.UtcNow.AddDays(activeDays.Value).ToString("o", CultureInfo.InvariantCulture);
        xml.Append("<active>1</active>");
        xml.Append($"<end_time>{endTime}</end_time>");
    }

    private static int NormalizeLimit(int limit) => Math.Clamp(limit, 1, 1000);

    private static string ToBoolAttribute(bool value) => value ? "1" : "0";

    private static string FormatDouble(double value) => value.ToString(CultureInfo.InvariantCulture);

    private static string EscapeXml(string input) =>
        input.Replace("&", "&amp;")
             .Replace("<", "&lt;")
             .Replace(">", "&gt;")
             .Replace("\"", "&quot;")
             .Replace("'", "&apos;");

    private static string ExtractCommandName(string xml)
    {
        var openTagStart = xml.IndexOf('<');

        if (openTagStart < 0 || openTagStart + 1 >= xml.Length)
        {
            return "unknown";
        }

        var start = openTagStart + 1;

        if (xml[start] == '/')
        {
            start++;
        }

        var end = start;
        while (end < xml.Length &&
               xml[end] != '>' &&
               xml[end] != '/' &&
               !char.IsWhiteSpace(xml[end]))
        {
            end++;
        }

        return end > start ? xml[start..end] : "unknown";
    }

    public void Dispose()
    {
        logger.LogDebug("Disposing GMP socket client resources");
        _stream?.Dispose();
        _socket?.Dispose();
        _connectionLock.Dispose();
    }
}
