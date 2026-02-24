namespace Gmcps.Domain.Models;

public sealed record GvmReportFormat(
    string Id,
    string Name,
    string? Extension,
    bool Active);
