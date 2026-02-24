namespace Gmcps.Domain.Scans.Reports.Outputs;

public sealed record ReportListItemOutput(
    string ReportId,
    string TaskId,
    string Timestamp,
    ReportSeveritySummaryOutput Summary);

public sealed record ListReportsOutput(
    IReadOnlyList<ReportListItemOutput> Reports);
