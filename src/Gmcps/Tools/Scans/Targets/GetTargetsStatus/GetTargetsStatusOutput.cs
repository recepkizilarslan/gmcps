
namespace Gmcps.Domain.Scans.Targets.Outputs;

public sealed record LastScanOutput(
    string TaskId,
    string Status,
    int? Progress,
    string? LastReportId);

public sealed record TargetStatusOutput(
    string TargetId,
    string Name,
    string Os,
    string Criticality,
    LastScanOutput? LastScan,
    ReportSeveritySummaryOutput? LastSummary);

public sealed record GetTargetsStatusOutput(
    IReadOnlyList<TargetStatusOutput> Targets);
