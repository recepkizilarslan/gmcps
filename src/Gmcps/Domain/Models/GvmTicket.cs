namespace Gmcps.Domain.Models;

public sealed record GvmTicket(
    string Id,
    string Name,
    string Status,
    double Severity,
    string Host,
    string Location,
    string? ResultId,
    string? AssignedToUserId);
