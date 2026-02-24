namespace Gmcps.Domain.Scans.Reports.Outputs;

public sealed record ReportSeveritySummaryOutput(
    int High,
    int Medium,
    int Low,
    int Log);

public sealed record GetReportSummaryOutput(
    string ReportId,
    string TaskId,
    DateTime Timestamp,
    ReportSeveritySummaryOutput Summary);
