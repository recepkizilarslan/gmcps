namespace Gmcps.Domain.Models;

public sealed record Report(
    string Id,
    string TaskId,
    DateTime Timestamp,
    ReportSummary Summary);

public sealed record ReportSummary(
    int High,
    int Medium,
    int Low,
    int Log);
