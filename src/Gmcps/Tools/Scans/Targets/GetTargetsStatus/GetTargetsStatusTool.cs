
namespace Gmcps.Tools.Scans.Targets.GetTargetsStatus;

public sealed class GetTargetsStatusTool(
    IClient client,
    ITargetMetadataStore metadataStore)
    : ITool<GetTargetsStatusInput, GetTargetsStatusOutput>
{
    public async Task<Result<GetTargetsStatusOutput>> ExecuteAsync(GetTargetsStatusInput input, CancellationToken ct)
    {

        var targetsResult = await client.GetTargetsAsync(ct);
        if (targetsResult.IsFailure)
        {
            throw new InvalidOperationException(targetsResult.Error);
        }

        var metadataResult = await metadataStore.GetAllAsync(ct);
        var metadataByTarget = metadataResult.IsSuccess
            ? metadataResult.Value.ToDictionary(metadata => metadata.TargetId)
            : new Dictionary<string, TargetMetadata>();

        var latestTaskByTarget = new Dictionary<string, GvmTask>();
        var reportSummariesByReportId = new Dictionary<string, ReportSummary>();

        if (input.IncludeTasks)
        {
            var tasksResult = await client.GetTasksAsync(ct);
            if (tasksResult.IsFailure)
            {
                throw new InvalidOperationException(tasksResult.Error);
            }

            latestTaskByTarget = tasksResult.Value
                .GroupBy(task => task.TargetId)
                .ToDictionary(group => group.Key, group => group.OrderByDescending(task => task.LastReportId).First());

            if (input.IncludeLastReportSummary)
            {
                foreach (var task in latestTaskByTarget.Values)
                {
                    if (task.LastReportId is null)
                    {
                        continue;
                    }

                    var reportResult = await client.GetReportSummaryAsync(task.LastReportId, ct);
                    if (reportResult.IsFailure)
                    {
                        throw new InvalidOperationException(reportResult.Error);
                    }

                    reportSummariesByReportId[task.LastReportId] = reportResult.Value.Summary;
                }
            }
        }

        var targets = new List<TargetStatusOutput>();

        foreach (var target in targetsResult.Value)
        {
            metadataByTarget.TryGetValue(target.Id, out var metadata);

            var osLabel = metadata?.Os.ToString() ?? target.OsHint;
            if (input.Os != OsFilter.Any &&
                !string.Equals(osLabel, input.Os.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            LastScanOutput? lastScan = null;
            ReportSeveritySummaryOutput? lastSummary = null;

            if (input.IncludeTasks && latestTaskByTarget.TryGetValue(target.Id, out var task))
            {
                lastScan = new LastScanOutput(
                    TaskId: task.Id,
                    Status: task.Status,
                    Progress: task.Progress >= 0 ? task.Progress : null,
                    LastReportId: task.LastReportId);

                if (input.IncludeLastReportSummary &&
                    task.LastReportId is not null &&
                    reportSummariesByReportId.TryGetValue(task.LastReportId, out var reportSummary))
                {
                    lastSummary = new ReportSeveritySummaryOutput(
                        reportSummary.High,
                        reportSummary.Medium,
                        reportSummary.Low,
                        reportSummary.Log);
                }
            }

            targets.Add(new TargetStatusOutput(
                TargetId: target.Id,
                Name: target.Name,
                Os: osLabel,
                Criticality: metadata?.Criticality.ToString() ?? "Normal",
                LastScan: lastScan,
                LastSummary: lastSummary));
        }

        return Result<GetTargetsStatusOutput>.Success(new GetTargetsStatusOutput(targets));
    }
}
