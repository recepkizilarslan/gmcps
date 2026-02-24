namespace Gmcps.Domain.Scans.Tasks.Outputs;

public sealed record TaskListItemOutput(
    string TaskId,
    string Name,
    string TargetId,
    string ScanConfigId,
    string Status,
    int Progress,
    string? LastReportId);

public sealed record ListTasksOutput(
    IReadOnlyList<TaskListItemOutput> Tasks);
