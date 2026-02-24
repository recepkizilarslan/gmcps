using Gmcps.Domain.Resilience.Compliance.Inputs;

namespace Gmcps.Tools.Resilience.Compliance.IsTargetCompliant;

public sealed class IsTargetCompliantTool(
    IClient client,
    ICompliancePolicyStore policyStore)
    : ITool<IsTargetCompliantInput, IsTargetCompliantOutput>
{
    public async Task<Result<IsTargetCompliantOutput>> ExecuteAsync(IsTargetCompliantInput input, CancellationToken ct)
    {

        var policyResult = await policyStore.GetPolicyAsync(input.PolicyId, ct);
        if (policyResult.IsFailure)
        {
            throw new InvalidOperationException(policyResult.Error);
        }

        var tasksResult = await client.GetTasksAsync(ct);
        if (tasksResult.IsFailure)
        {
            throw new InvalidOperationException(tasksResult.Error);
        }

        var targetTask = tasksResult.Value
            .Where(task => task.TargetId == input.TargetId && task.LastReportId is not null)
            .OrderByDescending(task => task.LastReportId)
            .FirstOrDefault();

        if (targetTask?.LastReportId is null)
        {
            return Result<IsTargetCompliantOutput>.Success(new IsTargetCompliantOutput(
                TargetId: input.TargetId,
                PolicyId: input.PolicyId,
                Compliant: false,
                Status: "NoData",
                Evidence: []));
        }

        var evidence = new List<ComplianceEvidenceOutput>();
        var compliant = true;

        var findingsResult = await client.GetReportFindingsAsync(targetTask.LastReportId, ct);
        if (findingsResult.IsFailure)
        {
            throw new InvalidOperationException(findingsResult.Error);
        }

        var findings = findingsResult.Value;

        foreach (var rule in policyResult.Value.Rules)
        {
            bool passed;
            string observed;
            string expected;

            switch (rule.RuleType)
            {
                case ComplianceRuleType.MaxSeverity:
                    var maxSeverity = findings.Any() ? findings.Max(finding => finding.Severity) : 0;
                    expected = $"Max severity <= {rule.MaxSeverityThreshold}";
                    observed = $"Max severity = {maxSeverity:F1}";
                    passed = maxSeverity <= (rule.MaxSeverityThreshold ?? 0);
                    break;

                case ComplianceRuleType.RequiredCheck:
                    var hasCheck = findings.Any(finding => finding.NvtOid == rule.RequiredNvtOid);
                    expected = $"NVT {rule.RequiredNvtOid} present in results";
                    observed = hasCheck ? "Found" : "Not found";
                    passed = hasCheck;
                    break;

                default:
                    continue;
            }

            if (!passed)
            {
                compliant = false;
            }

            evidence.Add(new ComplianceEvidenceOutput(
                CheckId: rule.CheckId,
                Title: rule.Title,
                Expected: expected,
                Observed: observed,
                Passed: passed,
                Reference: $"reportId:{targetTask.LastReportId}"));
        }

        return Result<IsTargetCompliantOutput>.Success(new IsTargetCompliantOutput(
            TargetId: input.TargetId,
            PolicyId: input.PolicyId,
            Compliant: compliant,
            Status: compliant ? "Pass" : "Fail",
            Evidence: evidence));
    }
}
