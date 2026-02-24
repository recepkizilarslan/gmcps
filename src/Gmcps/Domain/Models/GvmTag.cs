namespace Gmcps.Domain.Models;

public sealed record GvmTag(
    string Id,
    string Name,
    string? Value,
    string? Comment);
