namespace Gmcps.Domain.Models;

public sealed record HostAsset(
    string Id,
    string Name,
    string Ip,
    string OperatingSystem,
    double Severity);
