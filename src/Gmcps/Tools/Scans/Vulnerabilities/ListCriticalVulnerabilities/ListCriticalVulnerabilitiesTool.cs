using Gmcps.Domain.Scans.Vulnerabilities.Inputs;
using Gmcps.Domain.Scans.Vulnerabilities.Outputs;

namespace Gmcps.Tools.Scans.Vulnerabilities.ListCriticalVulnerabilities;

public sealed class ListCriticalVulnerabilitiesTool(
    IClient client,
    ITargetMetadataStore metadataStore)
    : ITool<ListCriticalVulnerabilitiesInput, ListCriticalVulnerabilitiesOutput>
{
    public async Task<Result<ListCriticalVulnerabilitiesOutput>> ExecuteAsync(ListCriticalVulnerabilitiesInput input, CancellationToken ct)
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

        var scopedTargets = FilterTargetsByScope(targetsResult.Value, metadataByTarget, input.Scope);

        var tasksResult = await client.GetTasksAsync(ct);
        if (tasksResult.IsFailure)
        {
            throw new InvalidOperationException(tasksResult.Error);
        }

        var tasksByTarget = tasksResult.Value
            .GroupBy(task => task.TargetId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var findings = new List<Finding>();

        foreach (var target in scopedTargets)
        {
            if (!tasksByTarget.TryGetValue(target.Id, out var tasks))
            {
                continue;
            }

            foreach (var task in tasks)
            {
                if (task.LastReportId is null)
                {
                    continue;
                }

                var findingsResult = await client.GetReportFindingsAsync(task.LastReportId, ct);
                if (findingsResult.IsFailure)
                {
                    throw new InvalidOperationException(findingsResult.Error);
                }

                findings.AddRange(findingsResult.Value);
            }
        }

        var groupedFindings = findings
            .Where(finding => finding.Severity >= input.MinSeverity)
            .GroupBy(finding => finding.NvtOid)
            .Select(group => new CriticalVulnerabilityFindingOutput(
                Name: group.First().Name,
                Severity: group.Max(finding => finding.Severity),
                Qod: group.First().Qod,
                Cves: group.SelectMany(finding => finding.Cves).Distinct().ToList(),
                AffectedHosts: group.Select(finding => finding.Host).Distinct().Count(),
                TopHosts: group.Select(finding => finding.Host).Distinct().Take(5).ToList(),
                NvtOid: group.Key))
            .OrderByDescending(finding => finding.Severity)
            .ThenByDescending(finding => finding.AffectedHosts)
            .Take(input.Limit)
            .ToList();

        return Result<ListCriticalVulnerabilitiesOutput>.Success(
            new ListCriticalVulnerabilitiesOutput(groupedFindings));
    }

    private static List<Target> FilterTargetsByScope(
        IReadOnlyList<Target> targets,
        IReadOnlyDictionary<string, TargetMetadata> metadataByTarget,
        VulnerabilityScopeInput? scope)
    {
        if (scope is null)
        {
            return targets.ToList();
        }

        var filtered = targets.AsEnumerable();

        if (scope.TargetIds is { Count: > 0 })
        {
            var ids = new HashSet<string>(scope.TargetIds);
            filtered = filtered.Where(target => ids.Contains(target.Id));
        }

        if (scope.Os == OsFilter.Any)
        {
            return filtered.ToList();
        }

        filtered = filtered.Where(target =>
        {
            metadataByTarget.TryGetValue(target.Id, out var metadata);
            var osLabel = metadata?.Os.ToString() ?? target.OsHint;
            return string.Equals(osLabel, scope.Os.ToString(), StringComparison.OrdinalIgnoreCase);
        });

        return filtered.ToList();
    }
}
