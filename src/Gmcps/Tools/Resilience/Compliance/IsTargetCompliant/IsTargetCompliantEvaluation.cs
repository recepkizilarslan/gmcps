using Gmcps.Domain.Resilience.Compliance.Outputs;

namespace Gmcps.Tools.Resilience.Compliance.IsTargetCompliant;

public sealed record IsTargetCompliantEvaluation(
    string TargetId,
    string PolicyId,
    bool Compliant,
    string Status,
    IReadOnlyList<ComplianceEvidenceOutput> Evidence);
