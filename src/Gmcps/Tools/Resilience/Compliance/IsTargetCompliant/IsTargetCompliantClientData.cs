
namespace Gmcps.Tools.Resilience.Compliance.IsTargetCompliant;

public sealed record IsTargetCompliantClientData(
    CompliancePolicy Policy,
    string? ReportId,
    IReadOnlyList<Finding> Findings);
