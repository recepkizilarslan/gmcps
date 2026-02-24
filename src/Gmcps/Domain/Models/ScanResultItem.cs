namespace Gmcps.Domain.Models;

public sealed record ScanResultItem(
    string Id,
    string Name,
    string Host,
    string Port,
    double Severity,
    string Threat,
    string NvtOid);
