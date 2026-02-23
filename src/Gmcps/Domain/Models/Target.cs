namespace Gmcps.Domain.Models;

public sealed record Target(
    string Id,
    string Name,
    IReadOnlyList<string> Tags,
    int HostsCount,
    string OsHint);
