namespace Gmcps.Domain.Models;

public sealed record SecurityInfoEntry(
    string Id,
    string Name,
    string Type,
    double? Score,
    string Summary);
