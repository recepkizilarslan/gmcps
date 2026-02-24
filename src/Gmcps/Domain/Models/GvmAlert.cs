namespace Gmcps.Domain.Models;

public sealed record GvmAlert(
    string Id,
    string Name,
    string Event,
    string? Comment);
