namespace Gmcps.Domain.Scans.Vulnerabilities.Outputs;

public sealed record CriticalPackageEvidenceOutput(
    string Host,
    string FindingName,
    IReadOnlyList<string> Cves);

public sealed record CriticalPackageOutput(
    string PackageName,
    double Severity,
    int AffectedHosts,
    IReadOnlyList<CriticalPackageEvidenceOutput> Evidence);

public sealed record ListCriticalPackagesOutput(
    IReadOnlyList<CriticalPackageOutput> Packages,
    string Support,
    string? Explanation);
