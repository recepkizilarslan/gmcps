namespace Gmcps.Domain.Models;

public sealed record ComplianceResult(
    string TargetId,
    string PolicyId,
    bool Compliant,
    IReadOnlyList<ComplianceEvidence> Evidence);

public sealed record ComplianceEvidence(
    string CheckId,
    string Title,
    string Expected,
    string Observed,
    bool Passed,
    string Reference);
