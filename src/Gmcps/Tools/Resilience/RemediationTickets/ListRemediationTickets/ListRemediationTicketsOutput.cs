namespace Gmcps.Domain.Resilience.RemediationTickets.Outputs;

public sealed record RemediationTicketOutput(
    string TicketId,
    string Name,
    string Status,
    double Severity,
    string Host,
    string Location,
    string? ResultId,
    string? AssignedToUserId);

public sealed record ListRemediationTicketsOutput(
    IReadOnlyList<RemediationTicketOutput> Tickets);
