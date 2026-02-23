using System.ComponentModel;
using System.Text.RegularExpressions;
using Gmcps.Inputs;
using Gmcps.Validation;
using Gmcps.Domain.Interfaces;
using Gmcps.Domain.Models;
using ModelContextProtocol.Server;

namespace Gmcps.Tools;

[McpServerToolType]
public sealed class AnalyticsTools(
    IRateLimiter rateLimiter,
    IGmpClient gmpClient,
    ITargetMetadataStore metadataStore,
    ICompliancePolicyStore policyStore)
    : ToolBase(rateLimiter)
{
    [McpServerTool(Name = "gvm_get_targets_status"), Description("Get status of targets filtered by OS (e.g., 'all Windows machines')")]
    public async Task<string> GetTargetsStatus(GetTargetsStatusInput input, CancellationToken ct)
    {
        var rateLimit = AcquireRateLimit();
        if (rateLimit.IsFailure)
        {
            return ErrorJson(rateLimit.Error);
        }

        var targetsResult = await gmpClient.GetTargetsAsync(ct);

        if (targetsResult.IsFailure)
        {
            return ErrorJson(targetsResult.Error);
        }

        var allMetadata = await metadataStore.GetAllAsync(ct);
        var metadataMap = allMetadata.IsSuccess
            ? allMetadata.Value.ToDictionary(m => m.TargetId)
            : new Dictionary<string, TargetMetadata>();

        var tasksResult = input.IncludeTasks ? await gmpClient.GetTasksAsync(ct) : null;
        var tasksByTarget = tasksResult?.IsSuccess == true
            ? tasksResult.Value.GroupBy(t => t.TargetId).ToDictionary(g => g.Key, g => g.OrderByDescending(t => t.LastReportId).First())
            : new Dictionary<string, GvmTask>();

        var targets = new List<object>();

        foreach (var target in targetsResult.Value)
        {
            var meta = metadataMap.GetValueOrDefault(target.Id);
            var osLabel = meta?.Os.ToString() ?? target.OsHint;

            if (input.Os != OsFilterInput.Any &&
                !string.Equals(osLabel, input.Os.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            
            var entry = new Dictionary<string, object?>
            {
                ["targetId"] = target.Id,
                ["name"] = target.Name,
                ["os"] = osLabel,
                ["criticality"] = meta?.Criticality.ToString() ?? "Normal"
            };

            if (input.IncludeTasks && tasksByTarget.TryGetValue(target.Id, out var task))
            {
                entry["lastScan"] = new
                {
                    taskId = task.Id,
                    status = task.Status,
                    progress = task.Progress >= 0 ? (int?)task.Progress : null,
                    lastReportId = task.LastReportId
                };

                if (input.IncludeLastReportSummary && task.LastReportId is not null)
                {
                    var reportResult = await gmpClient.GetReportSummaryAsync(task.LastReportId, ct);
                    
                    if (reportResult.IsSuccess)
                    {
                        entry["lastSummary"] = reportResult.Value.Summary;
                    }
                }
            }

            targets.Add(entry);
        }

        return ToJson(new { targets });
    }

    [McpServerTool(Name = "gvm_list_critical_targets"), 
     Description("List targets by criticality (e.g., 'which targets are critical?')")]
    public async Task<string> ListCriticalTargets(ListCriticalTargetsInput input, CancellationToken ct)
    {
        var rateLimit = AcquireRateLimit();
        if (rateLimit.IsFailure)
        {
            return ErrorJson(rateLimit.Error);
        }

        var targetsResult = await gmpClient.GetTargetsAsync(ct);

        if (targetsResult.IsFailure)
        {
            return ErrorJson(targetsResult.Error);
        }

        var allMetadata = await metadataStore.GetAllAsync(ct);
        var metadataMap = allMetadata.IsSuccess
            ? allMetadata.Value.ToDictionary(m => m.TargetId)
            : new Dictionary<string, TargetMetadata>();

        var tasksResult = await gmpClient.GetTasksAsync(ct);
        var tasksByTarget = tasksResult.IsSuccess
            ? tasksResult.Value.GroupBy(t => t.TargetId).ToDictionary(g => g.Key, g => g.OrderByDescending(t => t.LastReportId).First())
            : new Dictionary<string, GvmTask>();

        var results = new List<object>();

        foreach (var target in targetsResult.Value)
        {
            var meta = metadataMap.GetValueOrDefault(target.Id);
            var criticality = meta?.Criticality ?? Criticality.Normal;

            ReportSummary? summary = null;
            double riskScore = 0;
            bool noData = true;

            if (tasksByTarget.TryGetValue(target.Id, out var task) && task.LastReportId is not null)
            {
                var reportResult = await gmpClient.GetReportSummaryAsync(task.LastReportId, ct);
                
                if (reportResult.IsSuccess)
                {
                    summary = reportResult.Value.Summary;
                    riskScore = ComputeRiskScore(summary);
                    noData = false;
                }
            }

            results.Add(new
            {
                targetId = target.Id,
                name = target.Name,
                criticality = criticality.ToString(),
                os = meta?.Os.ToString() ?? target.OsHint,
                riskScore,
                noData,
                lastSummary = summary
            });
        }

        if (input.SortBy == SortByInput.Risk)
        {
            results = results.OrderByDescending(r => ((dynamic)r).riskScore).ToList();
        }
        else
        {
            results = results.OrderBy(r => ((dynamic)r).name).ToList();  
        }
        
        return ToJson(new { targets = results });
    }

    [McpServerTool(Name = "gvm_list_critical_vulnerabilities"), Description("List critical vulnerabilities across targets (e.g., 'give me critical vulns')")]
    public async Task<string> ListCriticalVulnerabilities(ListCriticalVulnerabilitiesInput input, CancellationToken ct)
    {
        var rateLimit = AcquireRateLimit();
        if (rateLimit.IsFailure)
        {
            return ErrorJson(rateLimit.Error);
        }

        var targetsResult = await gmpClient.GetTargetsAsync(ct);

        if (targetsResult.IsFailure)
        {
            return ErrorJson(targetsResult.Error);
        }

        var allMetadata = await metadataStore.GetAllAsync(ct);
        var metadataMap = allMetadata.IsSuccess
            ? allMetadata.Value.ToDictionary(m => m.TargetId)
            : new Dictionary<string, TargetMetadata>();
        
        var scopeTargets = FilterTargetsByScope(targetsResult.Value, metadataMap, input.Scope);

        var tasksResult = await gmpClient.GetTasksAsync(ct);
        var tasksByTarget = tasksResult.IsSuccess
            ? tasksResult.Value.GroupBy(t => t.TargetId).ToDictionary(g => g.Key, g => g.ToList())
            : new Dictionary<string, List<GvmTask>>();
        
        var allFindings = new List<Finding>();
        
        foreach (var target in scopeTargets)
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
                
                var findings = await gmpClient.GetReportFindingsAsync(task.LastReportId, ct);

                if (findings.IsSuccess)
                {
                    allFindings.AddRange(findings.Value); 
                }
            }
        }
        
        var grouped = allFindings
            .Where(f => f.Severity >= input.MinSeverity)
            .GroupBy(f => f.NvtOid)
            .Select(g => new
            {
                name = g.First().Name,
                severity = g.Max(f => f.Severity),
                qod = g.First().Qod,
                cves = g.SelectMany(f => f.Cves).Distinct().ToList(),
                affectedHosts = g.Select(f => f.Host).Distinct().Count(),
                topHosts = g.Select(f => f.Host).Distinct().Take(5).ToList(),
                nvtOid = g.Key
            })
            .OrderByDescending(f => f.severity)
            .ThenByDescending(f => f.affectedHosts)
            .Take(input.Limit)
            .ToList();

        return ToJson(new { findings = grouped });
    }

    [McpServerTool(Name = "gvm_is_target_compliant"), Description("Check if a target is compliant with a policy")]
    public async Task<string> IsTargetCompliant(IsTargetCompliantInput input, CancellationToken ct)
    {
        var rateLimit = AcquireRateLimit();
        if (rateLimit.IsFailure)
        {
            return ErrorJson(rateLimit.Error);
        }

        var validation = InputValidator.Validate(input);

        if (validation.IsFailure)
        {
            return ErrorJson(validation.Error);
        }

        var policyResult = await policyStore.GetPolicyAsync(input.PolicyId, ct);

        if (policyResult.IsFailure)
        {
            return ErrorJson(policyResult.Error);
        }

        var policy = policyResult.Value;
        
        var tasksResult = await gmpClient.GetTasksAsync(ct);

        if (tasksResult.IsFailure)
        {
            return ErrorJson(tasksResult.Error);
        }

        var targetTask = tasksResult.Value
            .Where(t => t.TargetId == input.TargetId && t.LastReportId is not null)
            .OrderByDescending(t => t.LastReportId)
            .FirstOrDefault();

        if (targetTask?.LastReportId is null)
        {
            return ToJson(new
            {
                targetId = input.TargetId,
                policyId = input.PolicyId,
                compliant = false,
                status = "NoData",
                evidence = Array.Empty<object>()
            });
        }

        var findingsResult = await gmpClient.GetReportFindingsAsync(targetTask.LastReportId, ct);

        if (findingsResult.IsFailure)
        {
            return ErrorJson(findingsResult.Error);
        }

        var findings = findingsResult.Value;
        var evidence = new List<object>();
        bool allPassed = true;

        foreach (var rule in policy.Rules)
        {
            bool passed;
            string observed;
            string expected;

            switch (rule.RuleType)
            {
                case ComplianceRuleType.MaxSeverity:
                    var maxSeverity = findings.Any() ? findings.Max(f => f.Severity) : 0;
                    expected = $"Max severity <= {rule.MaxSeverityThreshold}";
                    observed = $"Max severity = {maxSeverity:F1}";
                    passed = maxSeverity <= (rule.MaxSeverityThreshold ?? 0);
                    break;

                case ComplianceRuleType.RequiredCheck:
                    var hasCheck = findings.Any(f => f.NvtOid == rule.RequiredNvtOid);
                    expected = $"NVT {rule.RequiredNvtOid} present in results";
                    observed = hasCheck ? "Found" : "Not found";
                    passed = hasCheck;
                    break;

                default:
                    continue;
            }

            if (!passed)
            {
                allPassed = false;
            }

            evidence.Add(new
            {
                checkId = rule.CheckId,
                title = rule.Title,
                expected,
                observed,
                passed,
                reference = $"reportId:{targetTask.LastReportId}"
            });
        }

        return ToJson(new
        {
            targetId = input.TargetId,
            policyId = input.PolicyId,
            compliant = allPassed,
            status = allPassed ? "Pass" : "Fail",
            evidence
        });
    }

    [McpServerTool(Name = "gvm_list_critical_packages"), Description("List critical packages from scan data (best-effort extraction)")]
    public async Task<string> ListCriticalPackages(ListCriticalPackagesInput input, CancellationToken ct)
    {
        var rateLimit = AcquireRateLimit();
        if (rateLimit.IsFailure)
        {
            return ErrorJson(rateLimit.Error);
        }

        var targetsResult = await gmpClient.GetTargetsAsync(ct);

        if (targetsResult.IsFailure)
        {
            return ErrorJson(targetsResult.Error);
        }

        var allMetadata = await metadataStore.GetAllAsync(ct);
        var metadataMap = allMetadata.IsSuccess
            ? allMetadata.Value.ToDictionary(m => m.TargetId)
            : new Dictionary<string, TargetMetadata>();

        var scope = new VulnScopeInput(){ Os = input.Os };
        var scopeTargets = FilterTargetsByScope(targetsResult.Value, metadataMap, scope);

        var tasksResult = await gmpClient.GetTasksAsync(ct);
        var tasksByTarget = tasksResult.IsSuccess
            ? tasksResult.Value.GroupBy(t => t.TargetId).ToDictionary(g => g.Key, g => g.ToList())
            : new Dictionary<string, List<GvmTask>>();

        var packageFindings = new List<(string Package, double Severity, string Host, string FindingName, IReadOnlyList<string> Cves)>();

        foreach (var target in scopeTargets)
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
                
                var findings = await gmpClient.GetReportFindingsAsync(task.LastReportId, ct);

                if (findings.IsFailure)
                {
                    continue;
                }

                foreach (var finding in findings.Value.Where(f => f.Severity >= input.MinSeverity))
                {
                    var packages = ExtractPackageNames(finding);
                    
                    foreach (var pkg in packages)
                    {
                        packageFindings.Add((pkg, finding.Severity, finding.Host, finding.Name, finding.Cves));
                    }
                }
            }
        }

        if (packageFindings.Count == 0)
        {
            return ToJson(new
            {
                packages = Array.Empty<object>(),
                support = "bestEffort",
                explanation = "No package information could be extracted from scan findings. GVM may not provide package names directly in all scan configurations."
            });
        }

        var grouped = packageFindings
            .GroupBy(p => p.Package)
            .Select(g => new
            {
                packageName = g.Key,
                severity = g.Max(p => p.Severity),
                affectedHosts = g.Select(p => p.Host).Distinct().Count(),
                evidence = g.Select(p => new
                {
                    host = p.Host,
                    findingName = p.FindingName,
                    cves = p.Cves
                }).Take(10).ToList()
            })
            .OrderByDescending(p => p.severity)
            .ThenByDescending(p => p.affectedHosts)
            .Take(input.Limit)
            .ToList();

        return ToJson(new { packages = grouped, support = "bestEffort" });
    }

    private static double ComputeRiskScore(ReportSummary summary) =>
        summary.High * 10.0 + summary.Medium * 3.0 + summary.Low * 1.0;

    private static List<Target> FilterTargetsByScope(
        IReadOnlyList<Target> targets,
        Dictionary<string, TargetMetadata> metadataMap,
        VulnScopeInput? scope)
    {
        if (scope is null)
        {
            return targets.ToList();
        }

        var filtered = targets.AsEnumerable();

        if (scope.TargetIds is { Count: > 0 })
        {
            var ids = new HashSet<string>(scope.TargetIds);
            filtered = filtered.Where(t => ids.Contains(t.Id));
        }

        if (scope.Os == OsFilterInput.Any)
        {
            return filtered.ToList();
        }
        
        filtered = filtered.Where(t =>
        {
            var meta = metadataMap.GetValueOrDefault(t.Id);
            var osLabel = meta?.Os.ToString() ?? t.OsHint;
            return string.Equals(osLabel, scope.Os.ToString(), StringComparison.OrdinalIgnoreCase);
        });

        return filtered.ToList();
    }

    private static readonly Regex PackageRegex = new(
        @"(?:package|pkg|software)[\s:=]+([a-zA-Z0-9\-_.]+(?:[\s]+[0-9][a-zA-Z0-9\-_.]*)?)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex CpeRegex = new(
        @"cpe:/[ao]:([^:]+):([^:\s]+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

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
                var pkg = match.Groups[1].Value.Trim();

                if (pkg.Length > 2 && !pkg.All(char.IsDigit))
                {
                    packages.Add(pkg);
                }
            }
        }

        return packages.ToList();
    }
}
