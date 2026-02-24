namespace Gmcps.Tools.Configuration.ReportFormats.ListReportFormats;

public sealed record ReportFormatOutput(
    string Id,
    string Name,
    string? Extension,
    bool Active);

public sealed record ListReportFormatsOutput(
    IReadOnlyList<ReportFormatOutput> ReportFormats);
