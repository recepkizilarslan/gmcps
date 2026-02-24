using Gmcps.Domain.Scans.Targets.Inputs;
using Gmcps.Domain.Scans.Targets.Outputs;
using Gmcps.Tools.Core.Enums;

namespace Gmcps.Tools.Scans.Targets.ListCriticalTargets;

public sealed class ListCriticalTargetsTool(
    IClient client,
    ITargetMetadataStore metadataStore)
    : ITool<ListCriticalTargetsInput, ListCriticalTargetsOutput>
{
    public async Task<Result<ListCriticalTargetsOutput>> ExecuteAsync(ListCriticalTargetsInput input, CancellationToken ct)
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

        var reportSummariesByTargetId = new Dictionary<string, ReportSummary>();
        var tasksResult = await client.GetTasksAsync(ct);

        if (tasksResult.IsFailure)
        {
            throw new InvalidOperationException(tasksResult.Error);
        }

        var latestTaskByTarget = tasksResult.Value
            .GroupBy(task => task.TargetId)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(task => task.LastReportId).First());

        foreach (var (targetId, task) in latestTaskByTarget)
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

            reportSummariesByTargetId[targetId] = reportResult.Value.Summary;
        }

        var results = new List<CriticalTargetOutput>();

        foreach (var target in targetsResult.Value)
        {
            metadataByTarget.TryGetValue(target.Id, out var metadata);

            var summary = reportSummariesByTargetId.TryGetValue(target.Id, out var reportSummary)
                ? new ReportSeveritySummaryOutput(
                    reportSummary.High,
                    reportSummary.Medium,
                    reportSummary.Low,
                    reportSummary.Log)
                : null;

            var riskScore = summary is null
                ? 0
                : ComputeRiskScore(summary);

            var noData = summary is null;

            results.Add(new CriticalTargetOutput(
                TargetId: target.Id,
                Name: target.Name,
                Criticality: metadata?.Criticality.ToString() ?? "Normal",
                Os: metadata?.Os.ToString() ?? target.OsHint,
                RiskScore: riskScore,
                NoData: noData,
                LastSummary: summary));
        }

        var ordered = input.SortBy == SortBy.Risk
            ? results.OrderByDescending(result => result.RiskScore).ToList()
            : results.OrderBy(result => result.Name).ToList();

        return Result<ListCriticalTargetsOutput>.Success(new ListCriticalTargetsOutput(ordered));
    }

    private static double ComputeRiskScore(ReportSeveritySummaryOutput summary) =>
        summary.High * 10.0 + summary.Medium * 3.0 + summary.Low;
}
