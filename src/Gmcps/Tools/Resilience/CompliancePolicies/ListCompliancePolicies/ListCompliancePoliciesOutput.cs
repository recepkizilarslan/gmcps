namespace Gmcps.Domain.Resilience.CompliancePolicies.Outputs;

public sealed record CompliancePolicyRuleOutput(
    string CheckId,
    string Title,
    string RuleType,
    double? MaxSeverityThreshold,
    string? RequiredNvtOid);

public sealed record CompliancePolicyOutput(
    string PolicyId,
    string Name,
    IReadOnlyList<CompliancePolicyRuleOutput> Rules);

public sealed record ListCompliancePoliciesOutput(
    IReadOnlyList<CompliancePolicyOutput> Policies);
