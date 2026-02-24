namespace Gmcps.Domain.Models;

public sealed record GvmScanner(
    string Id,
    string Name,
    string Type,
    bool Active);
