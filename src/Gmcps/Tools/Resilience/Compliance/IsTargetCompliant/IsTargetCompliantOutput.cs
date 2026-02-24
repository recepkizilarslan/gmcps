namespace Gmcps.Domain.Resilience.Compliance.Outputs;

public sealed record ComplianceEvidenceOutput(
    string CheckId,
    string Title,
    string Expected,
    string Observed,
    bool Passed,
    string Reference);

public sealed record IsTargetCompliantOutput(
    string TargetId,
    string PolicyId,
    bool Compliant,
    string Status,
    IReadOnlyList<ComplianceEvidenceOutput> Evidence);
