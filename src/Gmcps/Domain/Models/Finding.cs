namespace Gmcps.Domain.Models;

public sealed record Finding(
    string Name,
    double Severity,
    int? Qod,
    IReadOnlyList<string> Cves,
    string Host,
    string Port,
    string NvtOid,
    string? Description);
