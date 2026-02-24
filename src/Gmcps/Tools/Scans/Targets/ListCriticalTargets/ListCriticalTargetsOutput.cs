using Gmcps.Domain.Scans.Reports.Outputs;

namespace Gmcps.Domain.Scans.Targets.Outputs;

public sealed record CriticalTargetOutput(
    string TargetId,
    string Name,
    string Criticality,
    string Os,
    double RiskScore,
    bool NoData,
    ReportSeveritySummaryOutput? LastSummary);

public sealed record ListCriticalTargetsOutput(
    IReadOnlyList<CriticalTargetOutput> Targets);
