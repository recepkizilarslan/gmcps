namespace Gmcps.Domain.Models;

public sealed record GvmCredential(
    string Id,
    string Name,
    string Type,
    string? Comment);
