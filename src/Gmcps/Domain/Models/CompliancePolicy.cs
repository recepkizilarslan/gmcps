namespace Gmcps.Domain.Models;

public sealed record CompliancePolicy(
    string PolicyId,
    string Name,
    IReadOnlyList<ComplianceRule> Rules);

public sealed record ComplianceRule(
    string CheckId,
    string Title,
    ComplianceRuleType RuleType,
    double? MaxSeverityThreshold,
    string? RequiredNvtOid);

public enum ComplianceRuleType
{
    MaxSeverity,
    RequiredCheck
}
