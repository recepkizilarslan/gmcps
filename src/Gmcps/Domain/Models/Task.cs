namespace Gmcps.Domain.Models;

public sealed record GvmTask(
    string Id,
    string Name,
    string TargetId,
    string ScanConfigId,
    string Status,
    int Progress,
    string? LastReportId);
