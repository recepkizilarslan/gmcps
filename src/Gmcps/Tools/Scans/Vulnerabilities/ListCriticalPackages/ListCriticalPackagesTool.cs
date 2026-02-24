using System.Text.RegularExpressions;

namespace Gmcps.Tools.Scans.Vulnerabilities.ListCriticalPackages;

public sealed class ListCriticalPackagesTool(
    IClient client,
    ITargetMetadataStore metadataStore)
    : ITool<ListCriticalPackagesInput, ListCriticalPackagesOutput>
{
    private static readonly Regex PackageRegex = new(
        @"(?:package|pkg|software)[\s:=]+([a-zA-Z0-9\-_.]+(?:[\s]+[0-9][a-zA-Z0-9\-_.]*)?)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex CpeRegex = new(
        @"cpe:/[ao]:([^:]+):([^:\s]+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public async Task<Result<ListCriticalPackagesOutput>> ExecuteAsync(ListCriticalPackagesInput input, CancellationToken ct)
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

        var scopedTargets = FilterTargetsByOs(targetsResult.Value, metadataByTarget, input.Os);

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

        var packageFindings = new List<(string Package, double Severity, string Host, string FindingName, IReadOnlyList<string> Cves)>();

        foreach (var finding in findings.Where(finding => finding.Severity >= input.MinSeverity))
        {
            var packages = ExtractPackageNames(finding);

            foreach (var packageName in packages)
            {
                packageFindings.Add((packageName, finding.Severity, finding.Host, finding.Name, finding.Cves));
            }
        }

        if (packageFindings.Count == 0)
        {
            return Result<ListCriticalPackagesOutput>.Success(new ListCriticalPackagesOutput(
                Packages: Array.Empty<CriticalPackageOutput>(),
                Support: "bestEffort",
                Explanation: "No package information could be extracted from scan findings. GVM may not provide package names directly in all scan configurations."));
        }

        var packagesOutput = packageFindings
            .GroupBy(packageFinding => packageFinding.Package)
            .Select(group => new CriticalPackageOutput(
                PackageName: group.Key,
                Severity: group.Max(packageFinding => packageFinding.Severity),
                AffectedHosts: group.Select(packageFinding => packageFinding.Host).Distinct().Count(),
                Evidence: group
                    .Select(packageFinding => new CriticalPackageEvidenceOutput(
                        Host: packageFinding.Host,
                        FindingName: packageFinding.FindingName,
                        Cves: packageFinding.Cves))
                    .Take(10)
                    .ToList()))
            .OrderByDescending(packageRisk => packageRisk.Severity)
            .ThenByDescending(packageRisk => packageRisk.AffectedHosts)
            .Take(input.Limit)
            .ToList();

        return Result<ListCriticalPackagesOutput>.Success(new ListCriticalPackagesOutput(
            Packages: packagesOutput,
            Support: "bestEffort",
            Explanation: null));
    }

    private static List<Target> FilterTargetsByOs(
        IReadOnlyList<Target> targets,
        IReadOnlyDictionary<string, TargetMetadata> metadataByTarget,
        OsFilter os)
    {
        if (os == OsFilter.Any)
        {
            return targets.ToList();
        }

        return targets
            .Where(target =>
            {
                metadataByTarget.TryGetValue(target.Id, out var metadata);
                var osLabel = metadata?.Os.ToString() ?? target.OsHint;
                return string.Equals(osLabel, os.ToString(), StringComparison.OrdinalIgnoreCase);
            })
            .ToList();
    }

    private static List<string> ExtractPackageNames(Finding finding)
    {
        var packages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var text = $"{finding.Name} {finding.Description}";

        foreach (Match match in CpeRegex.Matches(text))
        {
            if (match.Groups.Count >= 3)
            {
                packages.Add(match.Groups[2].Value);
            }
        }

        foreach (Match match in PackageRegex.Matches(text))
        {
            if (match.Groups.Count >= 2)
            {
                var packageName = match.Groups[1].Value.Trim();

                if (packageName.Length > 2 && !packageName.All(char.IsDigit))
                {
                    packages.Add(packageName);
                }
            }
        }

        return packages.ToList();
    }
}
