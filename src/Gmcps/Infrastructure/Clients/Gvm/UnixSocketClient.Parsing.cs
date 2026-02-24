
namespace Gmcps.Infrastructure.Clients.Gvm;

public sealed partial class UnixSocketClient
{
    internal static IReadOnlyList<ScanConfig> ParseScanConfigs(XDocument doc)
    {
        return doc.Descendants("config")
            .Select(e => new ScanConfig(
                Id: e.Attribute("id")?.Value ?? "",
                Name: e.Element("name")?.Value ?? "",
                Comment: e.Element("comment")?.Value))
            .ToList();
    }

    internal static IReadOnlyList<Target> ParseTargets(XDocument doc)
    {
        return doc.Descendants("target")
            .Where(e => e.Attribute("id") is not null)
            .Select(e => new Target(
                Id: e.Attribute("id")!.Value,
                Name: e.Element("name")?.Value ?? "",
                Tags: ParseTagHints(e.Element("comment")?.Value),
                HostsCount: ParseInt(e.Element("max_hosts")?.Value, 0),
                OsHint: InferOsFromElement(e)))
            .ToList();
    }

    internal static IReadOnlyList<GvmTask> ParseTasks(XDocument doc)
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

    internal static Result<Report> ParseReportSummary(XDocument doc, string reportId)
    {
        var reportEl = doc.Descendants("report")
            .FirstOrDefault(e => e.Attribute("id")?.Value == reportId)
            ?? doc.Descendants("report").FirstOrDefault();

        if (reportEl is null)
        {
            return Result<Report>.Failure($"Report {reportId} not found in response");
        }

        var innerReport = reportEl.Element("report") ?? reportEl;
        var taskId = innerReport.Ancestors("report")
            .FirstOrDefault()?.Element("task")?.Attribute("id")?.Value
            ?? innerReport.Element("task")?.Attribute("id")?.Value ?? "";

        var timestamp = innerReport.Element("timestamp")?.Value ??
                        innerReport.Element("creation_time")?.Value ?? "";

        return Result<Report>.Success(new Report(
            Id: reportId,
            TaskId: taskId,
            Timestamp: ParseDateTime(timestamp),
            Summary: ParseSeverityCounts(innerReport)));
    }

    internal static IReadOnlyList<Report> ParseReports(XDocument doc)
    {
        var reports = new List<Report>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var reportEl in doc.Descendants("report").Where(e => e.Attribute("id") is not null))
        {
            var id = reportEl.Attribute("id")!.Value;
            if (!seen.Add(id))
            {
                continue;
            }

            var innerReport = reportEl.Element("report") ?? reportEl;
            var taskId = innerReport.Element("task")?.Attribute("id")?.Value
                ?? reportEl.Element("task")?.Attribute("id")?.Value
                ?? "";

            var timestamp = innerReport.Element("timestamp")?.Value
                ?? innerReport.Element("creation_time")?.Value
                ?? innerReport.Element("scan_start")?.Value
                ?? "";

            reports.Add(new Report(
                Id: id,
                TaskId: taskId,
                Timestamp: ParseDateTime(timestamp),
                Summary: ParseSeverityCounts(innerReport)));
        }

        return reports;
    }

    internal static IReadOnlyList<Finding> ParseFindings(XDocument doc)
    {
        var results = new List<Finding>();

        foreach (var resultEl in doc.Descendants("result"))
        {
            if (resultEl.Attribute("id") is null)
            {
                continue;
            }

            var nvt = resultEl.Element("nvt");
            var severity = ParseDouble(
                resultEl.Element("severity")?.Value
                ?? nvt?.Element("cvss_base")?.Value, 0);

            results.Add(new Finding(
                Name: nvt?.Element("name")?.Value ?? resultEl.Element("name")?.Value ?? "Unknown",
                Severity: severity,
                Qod: ParseNullableInt(resultEl.Element("qod")?.Element("value")?.Value),
                Cves: ParseCves(nvt),
                Host: resultEl.Element("host")?.Value ?? "",
                Port: resultEl.Element("port")?.Value ?? "",
                NvtOid: nvt?.Attribute("oid")?.Value ?? "",
                Description: resultEl.Element("description")?.Value));
        }

        return results;
    }

    internal static IReadOnlyList<ScanResultItem> ParseResults(XDocument doc)
    {
        return doc.Descendants("result")
            .Where(e => e.Attribute("id") is not null)
            .Select(e => new ScanResultItem(
                Id: e.Attribute("id")!.Value,
                Name: e.Element("name")?.Value
                    ?? e.Element("nvt")?.Element("name")?.Value
                    ?? "Unknown",
                Host: e.Element("host")?.Value ?? "",
                Port: e.Element("port")?.Value ?? "",
                Severity: ParseDouble(
                    e.Element("severity")?.Value
                    ?? e.Element("nvt")?.Element("cvss_base")?.Value, 0),
                Threat: e.Element("threat")?.Value ?? "unknown",
                NvtOid: e.Element("nvt")?.Attribute("oid")?.Value ?? ""))
            .ToList();
    }

    internal static IReadOnlyList<NoteItem> ParseNotes(XDocument doc)
    {
        return doc.Descendants("note")
            .Where(e => e.Attribute("id") is not null)
            .Select(e => new NoteItem(
                Id: e.Attribute("id")!.Value,
                Text: e.Element("text")?.Value ?? "",
                Active: ParseBool(e.Element("active")?.Value, false)))
            .ToList();
    }

    internal static IReadOnlyList<OverrideItem> ParseOverrides(XDocument doc)
    {
        return doc.Descendants("override")
            .Where(e => e.Attribute("id") is not null)
            .Select(e => new OverrideItem(
                Id: e.Attribute("id")!.Value,
                Text: e.Element("text")?.Value ?? "",
                NewSeverity: ParseNullableDouble(
                    e.Element("new_severity")?.Value
                    ?? e.Element("severity")?.Value),
                Active: ParseBool(e.Element("active")?.Value, false)))
            .ToList();
    }

    internal static IReadOnlyList<HostAsset> ParseHostAssets(XDocument doc)
    {
        return doc.Descendants("asset")
            .Where(e => e.Attribute("id") is not null)
            .Select(e =>
            {
                var host = e.Element("host");
                var ip = ExtractHostDetail(host, "ip");

                if (string.IsNullOrWhiteSpace(ip))
                {
                    ip = e.Element("hosts")?.Value ?? "";
                }

                var os = ExtractHostDetail(host, "os");

                if (string.IsNullOrWhiteSpace(os))
                {
                    os = ExtractHostDetail(host, "best_os_cpe");
                }

                var severity = ParseDouble(
                    host?.Element("severity")?.Element("value")?.Value
                    ?? host?.Element("severity")?.Value, 0);

                return new HostAsset(
                    Id: e.Attribute("id")!.Value,
                    Name: e.Element("name")?.Value ?? "",
                    Ip: ip,
                    OperatingSystem: string.IsNullOrWhiteSpace(os) ? "Unknown" : os,
                    Severity: severity);
            })
            .ToList();
    }

    internal static IReadOnlyList<OperatingSystemAsset> ParseOperatingSystemAssets(XDocument doc)
    {
        return doc.Descendants("asset")
            .Where(e => e.Attribute("id") is not null)
            .Select(e =>
            {
                var os = e.Element("os");

                var averageSeverity = ParseDouble(
                    os?.Element("average_severity")?.Element("value")?.Value
                    ?? os?.Element("average_severity")?.Value, 0);

                var highestSeverity = ParseDouble(
                    os?.Element("highest_severity")?.Element("value")?.Value
                    ?? os?.Element("highest_severity")?.Value
                    ?? os?.Element("latest_severity")?.Element("value")?.Value
                    ?? os?.Element("latest_severity")?.Value, 0);

                var hosts = ParseInt(
                    os?.Element("hosts")?.Value
                    ?? os?.Element("installs")?.Value, 0);

                var allHosts = ParseInt(
                    os?.Element("all_hosts")?.Value
                    ?? os?.Element("all_installs")?.Value, hosts);

                return new OperatingSystemAsset(
                    Id: e.Attribute("id")!.Value,
                    Name: e.Element("name")?.Value ?? "",
                    Title: os?.Element("title")?.Value ?? "",
                    Hosts: hosts,
                    AllHosts: allHosts,
                    AverageSeverity: averageSeverity,
                    HighestSeverity: highestSeverity);
            })
            .ToList();
    }

    internal static IReadOnlyList<TlsCertificateAsset> ParseTlsCertificates(XDocument doc)
    {
        return doc.Descendants("tls_certificate")
            .Where(e => e.Attribute("id") is not null)
            .Select(e => new TlsCertificateAsset(
                Id: e.Attribute("id")!.Value,
                Name: e.Element("name")?.Value ?? "",
                SubjectDn: e.Element("subject_dn")?.Value ?? "",
                IssuerDn: e.Element("issuer_dn")?.Value ?? "",
                TimeStatus: e.Element("time_status")?.Value ?? "unknown",
                Sha256Fingerprint: e.Element("sha256_fingerprint")?.Value ?? "",
                LastSeen: e.Element("last_seen")?.Value ?? ""))
            .ToList();
    }

    internal static IReadOnlyList<SecurityInfoEntry> ParseSecurityInfos(XDocument doc, string expectedType)
    {
        var normalizedExpectedType = expectedType.ToLowerInvariant();

        return doc.Descendants("info")
            .Select(e =>
            {
                var actualType = ResolveSecurityInfoType(e, normalizedExpectedType);
                var typeElement = e.Element(actualType);

                var score = ParseNullableDouble(typeElement?.Element("score")?.Value);
                var summary = typeElement?.Element("summary")?.Value
                    ?? typeElement?.Element("description")?.Value
                    ?? typeElement?.Element("title")?.Value
                    ?? "";

                return new SecurityInfoEntry(
                    Id: e.Attribute("id")?.Value ?? e.Element("name")?.Value ?? "",
                    Name: e.Element("name")?.Value ?? "",
                    Type: actualType.ToUpperInvariant(),
                    Score: score,
                    Summary: summary);
            })
            .ToList();
    }

    internal static IReadOnlyList<NvtEntry> ParseNvts(XDocument doc)
    {
        return doc.Descendants("nvt")
            .Where(e => e.Attribute("oid") is not null)
            .Select(e => new NvtEntry(
                Oid: e.Attribute("oid")!.Value,
                Name: e.Element("name")?.Value ?? "",
                Family: e.Element("family")?.Value ?? "",
                Severity: ParseDouble(
                    e.Element("severities")?.Attribute("score")?.Value
                    ?? e.Element("cvss_base")?.Value, 0),
                Summary: e.Element("summary")?.Value
                    ?? e.Element("description")?.Value
                    ?? ""))
            .ToList();
    }

    internal static IReadOnlyList<GvmPortList> ParsePortLists(XDocument doc)
    {
        return doc.Descendants("port_list")
            .Where(e => e.Attribute("id") is not null)
            .Select(e => new GvmPortList(
                Id: e.Attribute("id")!.Value,
                Name: e.Element("name")?.Value ?? "",
                Comment: e.Element("comment")?.Value))
            .ToList();
    }

    internal static IReadOnlyList<GvmCredential> ParseCredentials(XDocument doc)
    {
        return doc.Descendants("credential")
            .Where(e => e.Attribute("id") is not null)
            .Select(e => new GvmCredential(
                Id: e.Attribute("id")!.Value,
                Name: e.Element("name")?.Value ?? "",
                Type: e.Element("type")?.Element("name")?.Value
                    ?? e.Element("type")?.Value
                    ?? "unknown",
                Comment: e.Element("comment")?.Value))
            .ToList();
    }

    internal static IReadOnlyList<GvmAlert> ParseAlerts(XDocument doc)
    {
        return doc.Descendants("alert")
            .Where(e => e.Attribute("id") is not null)
            .Select(e => new GvmAlert(
                Id: e.Attribute("id")!.Value,
                Name: e.Element("name")?.Value ?? "",
                Event: e.Element("event")?.Element("name")?.Value
                    ?? e.Element("event")?.Value
                    ?? "unknown",
                Comment: e.Element("comment")?.Value))
            .ToList();
    }

    internal static IReadOnlyList<GvmSchedule> ParseSchedules(XDocument doc)
    {
        return doc.Descendants("schedule")
            .Where(e => e.Attribute("id") is not null)
            .Select(e => new GvmSchedule(
                Id: e.Attribute("id")!.Value,
                Name: e.Element("name")?.Value ?? "",
                Timezone: e.Element("timezone")?.Value,
                Icalendar: e.Element("icalendar")?.Value))
            .ToList();
    }

    internal static IReadOnlyList<GvmReportConfig> ParseReportConfigs(XDocument doc)
    {
        return doc.Descendants("report_config")
            .Where(e => e.Attribute("id") is not null)
            .Select(e => new GvmReportConfig(
                Id: e.Attribute("id")!.Value,
                Name: e.Element("name")?.Value ?? "",
                Comment: e.Element("comment")?.Value))
            .ToList();
    }

    internal static IReadOnlyList<GvmReportFormat> ParseReportFormats(XDocument doc)
    {
        return doc.Descendants("report_format")
            .Where(e => e.Attribute("id") is not null)
            .Select(e => new GvmReportFormat(
                Id: e.Attribute("id")!.Value,
                Name: e.Element("name")?.Value ?? "",
                Extension: e.Element("extension")?.Value,
                Active: ParseBool(e.Element("active")?.Value, true)))
            .ToList();
    }

    internal static IReadOnlyList<GvmScanner> ParseScanners(XDocument doc)
    {
        return doc.Descendants("scanner")
            .Where(e => e.Attribute("id") is not null)
            .Select(e => new GvmScanner(
                Id: e.Attribute("id")!.Value,
                Name: e.Element("name")?.Value ?? "",
                Type: e.Element("type")?.Value ?? "unknown",
                Active: ParseBool(e.Element("active")?.Value, true)))
            .ToList();
    }

    internal static IReadOnlyList<GvmFilter> ParseFilters(XDocument doc)
    {
        return doc.Descendants("filter")
            .Where(e => e.Attribute("id") is not null)
            .Select(e => new GvmFilter(
                Id: e.Attribute("id")!.Value,
                Name: e.Element("name")?.Value ?? "",
                Term: e.Element("term")?.Value,
                Type: e.Element("type")?.Value))
            .ToList();
    }

    internal static IReadOnlyList<GvmTag> ParseTags(XDocument doc)
    {
        return doc.Descendants("tag")
            .Where(e => e.Attribute("id") is not null)
            .Select(e => new GvmTag(
                Id: e.Attribute("id")!.Value,
                Name: e.Element("name")?.Value ?? "",
                Value: e.Element("value")?.Value,
                Comment: e.Element("comment")?.Value))
            .ToList();
    }

    internal static IReadOnlyList<GvmTicket> ParseTickets(XDocument doc)
    {
        return doc.Descendants("ticket")
            .Where(e => e.Attribute("id") is not null)
            .Select(e => new GvmTicket(
                Id: e.Attribute("id")!.Value,
                Name: e.Element("name")?.Value ?? "",
                Status: e.Element("status")?.Value ?? "unknown",
                Severity: ParseDouble(e.Element("severity")?.Value, 0),
                Host: e.Element("host")?.Value ?? "",
                Location: e.Element("location")?.Value ?? "",
                ResultId: e.Element("result")?.Attribute("id")?.Value,
                AssignedToUserId: e.Element("assigned_to")?.Element("user")?.Attribute("id")?.Value))
            .ToList();
    }

    internal static IReadOnlyList<GvmComplianceAuditReport> ParseComplianceAuditReports(XDocument doc)
    {
        var reports = new List<GvmComplianceAuditReport>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var reportEl in doc.Descendants("report").Where(e => e.Attribute("id") is not null))
        {
            var id = reportEl.Attribute("id")!.Value;
            if (!seen.Add(id))
            {
                continue;
            }

            var innerReport = reportEl.Element("report") ?? reportEl;
            var taskId = innerReport.Element("task")?.Attribute("id")?.Value
                ?? reportEl.Element("task")?.Attribute("id")?.Value
                ?? "";

            var timestamp = innerReport.Element("timestamp")?.Value
                ?? innerReport.Element("creation_time")?.Value
                ?? innerReport.Element("scan_start")?.Value
                ?? "";

            var compliance = innerReport.Element("compliance_count");
            var yes = ParseInt(compliance?.Element("yes")?.Value, 0);
            var no = ParseInt(compliance?.Element("no")?.Value, 0);
            var incomplete = ParseInt(compliance?.Element("incomplete")?.Value, 0);

            reports.Add(new GvmComplianceAuditReport(
                Id: id,
                TaskId: taskId,
                Timestamp: timestamp,
                Yes: yes,
                No: no,
                Incomplete: incomplete));
        }

        return reports;
    }

    private static int ParseInt(string? value, int defaultValue)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        return int.TryParse(value, CultureInfo.InvariantCulture, out var result) ? result : defaultValue;
    }

    private static double ParseDouble(string? value, double defaultValue)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        return double.TryParse(value, CultureInfo.InvariantCulture, out var result) ? result : defaultValue;
    }

    private static int? ParseNullableInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return int.TryParse(value, CultureInfo.InvariantCulture, out var result) ? result : null;
    }

    private static double? ParseNullableDouble(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return double.TryParse(value, CultureInfo.InvariantCulture, out var result) ? result : null;
    }

    private static bool ParseBool(string? value, bool defaultValue)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        if (bool.TryParse(value, out var boolResult))
        {
            return boolResult;
        }

        if (value == "1")
        {
            return true;
        }

        if (value == "0")
        {
            return false;
        }

        return defaultValue;
    }

    private static DateTime ParseDateTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return DateTime.MinValue;
        }

        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var result)
            ? result.ToUniversalTime()
            : DateTime.MinValue;
    }

    private static IReadOnlyList<string> ParseTagHints(string? comment)
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

    private static string ExtractHostDetail(XElement? hostElement, string detailName)
    {
        if (hostElement is null)
        {
            return "";
        }

        var detail = hostElement.Elements("detail")
            .FirstOrDefault(e =>
                string.Equals(
                    e.Element("name")?.Value,
                    detailName,
                    StringComparison.OrdinalIgnoreCase));

        return detail?.Element("value")?.Value ?? "";
    }

    private static string ResolveSecurityInfoType(XElement infoElement, string fallbackType)
    {
        string[] knownTypes = ["cve", "cpe", "cert_bund_adv", "dfn_cert_adv", "nvt"];

        foreach (var type in knownTypes)
        {
            if (infoElement.Element(type) is not null)
            {
                return type;
            }
        }

        return fallbackType;
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

        return cveText
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(c => c.StartsWith("CVE-", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private static ReportSummary ParseSeverityCounts(XElement reportElement)
    {
        int high = 0, medium = 0, low = 0, log = 0;

        foreach (var result in reportElement.Descendants("result"))
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
}
