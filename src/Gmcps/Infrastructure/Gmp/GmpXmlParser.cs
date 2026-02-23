using System.Globalization;
using System.Xml.Linq;
using Gmcps.Domain;
using Gmcps.Models;

namespace Gmcps.Infrastructure.Gmp;

public static class GmpXmlParser
{
    public static IReadOnlyList<ScanConfig> ParseScanConfigs(XDocument doc)
    {
        return doc.Descendants("config")
            .Select(e => new ScanConfig(
                Id: e.Attribute("id")?.Value ?? "",
                Name: e.Element("name")?.Value ?? "",
                Comment: e.Element("comment")?.Value))
            .ToList();
    }

    public static IReadOnlyList<Target> ParseTargets(XDocument doc)
    {
        return doc.Descendants("target")
            .Where(e => e.Attribute("id") is not null)
            .Select(e => new Target(
                Id: e.Attribute("id")!.Value,
                Name: e.Element("name")?.Value ?? "",
                Tags: ParseTags(e.Element("comment")?.Value),
                HostsCount: ParseInt(e.Element("max_hosts")?.Value, 0),
                OsHint: InferOsFromElement(e)))
            .ToList();
    }

    public static IReadOnlyList<GvmTask> ParseTasks(XDocument doc)
    {
        return doc.Descendants("task")
            .Where(e => e.Attribute("id") is not null)
            .Select(e =>
            {
                var lastReport = e.Element("last_report")?.Element("report");
                return new GvmTask(
                    Id: e.Attribute("id")!.Value,
                    Name: e.Element("name")?.Value ?? "",
                    TargetId: e.Element("target")?.Attribute("id")?.Value ?? "",
                    ScanConfigId: e.Element("config")?.Attribute("id")?.Value ?? "",
                    Status: e.Element("status")?.Value ?? "Unknown",
                    Progress: ParseInt(e.Element("progress")?.Value, -1),
                    LastReportId: lastReport?.Attribute("id")?.Value);
            })
            .ToList();
    }

    public static Result<Report> ParseReportSummary(XDocument doc, string reportId)
    {
        var reportEl = doc.Descendants("report")
            .FirstOrDefault(e => e.Attribute("id")?.Value == reportId);

        if (reportEl is null)
        {
            reportEl = doc.Descendants("report").FirstOrDefault();
        }

        if (reportEl is null)
        {
            return Result<Report>.Failure($"Report {reportId} not found in response");
        }

        var innerReport = reportEl.Element("report") ?? reportEl;

        var resultCount = innerReport.Element("result_count");
        var severityCounts = innerReport.Element("severity")?.Element("filtered");
        var taskId = innerReport.Ancestors("report")
            .FirstOrDefault()?.Element("task")?.Attribute("id")?.Value
            ?? innerReport.Element("task")?.Attribute("id")?.Value ?? "";

        var timestamp = innerReport.Element("timestamp")?.Value ??
                        innerReport.Element("creation_time")?.Value ?? "";

        var summary = ParseSeverityCounts(innerReport);

        return Result<Report>.Success(new Report(
            Id: reportId,
            TaskId: taskId,
            Timestamp: ParseDateTime(timestamp),
            Summary: summary));
    }

    public static ReportSummary ParseSeverityCounts(XElement reportElement)
    {
        int high = 0, medium = 0, low = 0, log = 0;

        var results = reportElement.Descendants("result");
        foreach (var result in results)
        {
            var severity = ParseDouble(result.Element("severity")?.Value, 0);
            var threat = result.Element("threat")?.Value?.ToLowerInvariant();

            switch (threat)
            {
                case "high":
                    high++;
                    break;
                case "medium":
                    medium++;
                    break;
                case "low":
                    low++;
                    break;
                case "log":
                    log++;
                    break;
                default:
                    if (severity >= 7.0)
                    {
                        high++;
                    }
                    else if (severity >= 4.0)
                    {
                        medium++;
                    }
                    else if (severity > 0)
                    {
                        low++;
                    }
                    else
                    {
                        log++;
                    }
                    break;
            }
        }

        // Also try severity counts element
        var countEl = reportElement.Element("result_count");
        if (countEl is not null && high == 0 && medium == 0 && low == 0)
        {
            high = ParseInt(countEl.Element("hole")?.Value, 0) +
                   ParseInt(countEl.Element("warning")?.Value, 0) > 0
                ? ParseInt(countEl.Element("hole")?.Value, 0)
                : high;
        }

        return new ReportSummary(High: high, Medium: medium, Low: low, Log: log);
    }

    public static IReadOnlyList<Finding> ParseFindings(XDocument doc)
    {
        var results = new List<Finding>();

        foreach (var resultEl in doc.Descendants("result"))
        {
            if (resultEl.Attribute("id") is null)
            {
                continue;
            }

            var nvt = resultEl.Element("nvt");
            var cves = ParseCves(nvt);
            var severity = ParseDouble(
                resultEl.Element("severity")?.Value
                ?? nvt?.Element("cvss_base")?.Value, 0);

            results.Add(new Finding(
                Name: nvt?.Element("name")?.Value ?? resultEl.Element("name")?.Value ?? "Unknown",
                Severity: severity,
                Qod: ParseNullableInt(resultEl.Element("qod")?.Element("value")?.Value),
                Cves: cves,
                Host: resultEl.Element("host")?.Value ?? "",
                Port: resultEl.Element("port")?.Value ?? "",
                NvtOid: nvt?.Attribute("oid")?.Value ?? "",
                Description: resultEl.Element("description")?.Value));
        }

        return results;
    }

    private static IReadOnlyList<string> ParseCves(XElement? nvtElement)
    {
        if (nvtElement is null)
        {
            return [];
        }

        var cveElement = nvtElement.Element("cve");
        if (cveElement is null)
        {
            return [];
        }

        var cveText = cveElement.Value;
        if (string.IsNullOrWhiteSpace(cveText) || cveText == "NOCVE")
        {
            return [];
        }

        return cveText.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(c => c.StartsWith("CVE-", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private static IReadOnlyList<string> ParseTags(string? comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
        {
            return [];
        }
        return comment.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static string InferOsFromElement(XElement targetElement)
    {
        var hosts = targetElement.Element("hosts")?.Value ?? "";
        var name = targetElement.Element("name")?.Value ?? "";
        var comment = targetElement.Element("comment")?.Value ?? "";

        var all = $"{name} {comment} {hosts}".ToLowerInvariant();
        if (all.Contains("windows") || all.Contains("win"))
        {
            return "Windows";
        }
        if (all.Contains("linux") || all.Contains("ubuntu") || all.Contains("centos") ||
            all.Contains("debian") || all.Contains("rhel"))
        {
            return "Linux";
        }
        return "Unknown";
    }

    internal static int ParseInt(string? value, int defaultValue)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }
        return int.TryParse(value, CultureInfo.InvariantCulture, out var result) ? result : defaultValue;
    }

    internal static double ParseDouble(string? value, double defaultValue)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }
        return double.TryParse(value, CultureInfo.InvariantCulture, out var result) ? result : defaultValue;
    }

    internal static int? ParseNullableInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }
        return int.TryParse(value, CultureInfo.InvariantCulture, out var result) ? result : null;
    }

    internal static DateTime ParseDateTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return DateTime.MinValue;
        }
        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var result)
            ? result.ToUniversalTime()
            : DateTime.MinValue;
    }
}
