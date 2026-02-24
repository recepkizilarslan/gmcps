namespace Gmcps.Domain.SecurityInformation.Shared;

public sealed record SecurityInfoItemOutput(
    string Id,
    string Name,
    string Type,
    double? Score,
    string Summary);
