namespace Gmcps.Domain.Models;

public sealed record OperatingSystemAsset(
    string Id,
    string Name,
    string Title,
    int Hosts,
    int AllHosts,
    double AverageSeverity,
    double HighestSeverity);
