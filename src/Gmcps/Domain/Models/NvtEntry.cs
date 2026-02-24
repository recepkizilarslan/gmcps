namespace Gmcps.Domain.Models;

public sealed record NvtEntry(
    string Oid,
    string Name,
    string Family,
    double Severity,
    string Summary);
