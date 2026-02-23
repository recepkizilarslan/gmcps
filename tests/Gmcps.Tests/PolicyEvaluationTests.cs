using System.Text.Json;
using Gmcps.Domain.Configuration;
using Gmcps.Domain.Models;
using Gmcps.Infrastructure.Stores;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Gmcps.Tests;

public class PolicyEvaluationTests : IDisposable
{
    private readonly string _policiesPath;

    public PolicyEvaluationTests()
    {
        _policiesPath = Path.Combine(Path.GetTempPath(), $"test-policies-{Guid.NewGuid()}.json");
    }

    [Fact]
    public async Task GetPolicy_ExistingPolicy_ReturnsSuccess()
    {
        var policies = new[]
        {
            new CompliancePolicy(
                PolicyId: "pol-1",
                Name: "No Critical Vulns",
                Rules:
                [
                    new ComplianceRule(
                        CheckId: "check-1",
                        Title: "Max severity must be below 7.0",
                        RuleType: ComplianceRuleType.MaxSeverity,
                        MaxSeverityThreshold: 7.0,
                        RequiredNvtOid: null)
                ])
        };

        WritePolicies(policies);
        var store = CreateStore();

        var result = await store.GetPolicyAsync("pol-1", CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Equal("pol-1", result.Value.PolicyId);
        Assert.Single(result.Value.Rules);
    }

    [Fact]
    public async Task GetPolicy_NonExistentPolicy_ReturnsFailure()
    {
        WritePolicies(Array.Empty<CompliancePolicy>());
        var store = CreateStore();

        var result = await store.GetPolicyAsync("nonexistent", CancellationToken.None);
        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public void ComplianceEvaluation_AllRulesPass_IsCompliant()
    {
        var policy = new CompliancePolicy("pol-1", "Safe Policy",
        [
            new ComplianceRule("c1", "Max sev below 5", ComplianceRuleType.MaxSeverity, 5.0, null)
        ]);

        var findings = new List<Finding>
        {
            new("Low Finding", 3.0, null, [], "host1", "80/tcp", "oid-1", null)
        };

        bool allPassed = EvaluatePolicy(policy, findings);
        Assert.True(allPassed);
    }

    [Fact]
    public void ComplianceEvaluation_RuleFails_IsNonCompliant()
    {
        var policy = new CompliancePolicy("pol-1", "Strict Policy",
        [
            new ComplianceRule("c1", "Max sev below 5", ComplianceRuleType.MaxSeverity, 5.0, null)
        ]);

        var findings = new List<Finding>
        {
            new("Critical Finding", 9.8, null, [], "host1", "443/tcp", "oid-1", null)
        };

        bool allPassed = EvaluatePolicy(policy, findings);
        Assert.False(allPassed);
    }

    // Mirror logic from AnalyticsTools.IsTargetCompliant
    private static bool EvaluatePolicy(CompliancePolicy policy, List<Finding> findings)
    {
        foreach (var rule in policy.Rules)
        {
            switch (rule.RuleType)
            {
                case ComplianceRuleType.MaxSeverity:
                    var maxSeverity = findings.Any() ? findings.Max(f => f.Severity) : 0;
                    if (maxSeverity > (rule.MaxSeverityThreshold ?? 0))
                    {
                        return false;
                    }
                    break;

                case ComplianceRuleType.RequiredCheck:
                    if (!findings.Any(f => f.NvtOid == rule.RequiredNvtOid))
                    {
                        return false;
                    }
                    break;
            }
        }
        return true;
    }

    private void WritePolicies(object policies)
    {
        var json = JsonSerializer.Serialize(policies, new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        });
        File.WriteAllText(_policiesPath, json);
    }

    private JsonCompliancePolicyStore CreateStore()
    {
        var options = Options.Create(new StoreOptions { PoliciesPath = _policiesPath });
        return new JsonCompliancePolicyStore(options, NullLogger<JsonCompliancePolicyStore>.Instance);
    }

    public void Dispose()
    {
        if (File.Exists(_policiesPath))
        {
            File.Delete(_policiesPath);
        }
    }
}
