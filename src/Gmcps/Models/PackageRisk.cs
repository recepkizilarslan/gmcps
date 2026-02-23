namespace Gmcps.Models;

public sealed record PackageRisk(
    string PackageName,
    double Severity,
    string Host,
    string Evidence);
