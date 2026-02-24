namespace Gmcps.Domain.Scans.Tasks.Outputs;

public sealed record GetTaskStatusOutput(
    string TaskId,
    string Name,
    string Status,
    int Progress,
    string? LastReportId);
