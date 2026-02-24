namespace Gmcps.Domain.Scans.Vulnerabilities.Outputs;

public sealed record CriticalVulnerabilityFindingOutput(
    string Name,
    double Severity,
    int? Qod,
    IReadOnlyList<string> Cves,
    int AffectedHosts,
    IReadOnlyList<string> TopHosts,
    string NvtOid);

public sealed record ListCriticalVulnerabilitiesOutput(
    IReadOnlyList<CriticalVulnerabilityFindingOutput> Findings);
