namespace Gmcps.Domain.Models;

public sealed record GvmFilter(
    string Id,
    string Name,
    string? Term,
    string? Type);
