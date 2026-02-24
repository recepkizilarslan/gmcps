namespace Gmcps.Tools.Configuration.ReportConfigs.ListReportConfigs;

public sealed record ReportConfigOutput(
    string Id,
    string Name,
    string? Comment);

public sealed record ListReportConfigsOutput(
    IReadOnlyList<ReportConfigOutput> ReportConfigs);
