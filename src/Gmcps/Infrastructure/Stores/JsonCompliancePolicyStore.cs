using System.Text.Json;
using System.Text.Json.Serialization;
using Gmcps.Configuration;
using Gmcps.Domain;
using Gmcps.Domain.Interfaces;
using Gmcps.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gmcps.Infrastructure.Stores;

public sealed class JsonCompliancePolicyStore : ICompliancePolicyStore
{
    private readonly string _policiesPath;
    private readonly ILogger<JsonCompliancePolicyStore> _logger;
    private List<CompliancePolicy>? _cachedPolicies;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        WriteIndented = true
    };

    public JsonCompliancePolicyStore(IOptions<StoreOptions> options, ILogger<JsonCompliancePolicyStore> logger)
    {
        _policiesPath = options.Value.PoliciesPath;
        _logger = logger;
    }

    public Task<Result<CompliancePolicy>> GetPolicyAsync(string policyId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var policies = LoadPolicies();
        var policy = policies.FirstOrDefault(p => p.PolicyId == policyId);
        return Task.FromResult(policy is null
            ? Result<CompliancePolicy>.Failure($"Policy '{policyId}' not found")
            : Result<CompliancePolicy>.Success(policy));
    }

    public Task<Result<IReadOnlyList<CompliancePolicy>>> GetAllPoliciesAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var policies = LoadPolicies();
        return Task.FromResult(Result<IReadOnlyList<CompliancePolicy>>.Success(policies));
    }

    private List<CompliancePolicy> LoadPolicies()
    {
        if (_cachedPolicies is not null)
        {
            return _cachedPolicies;
        }

        if (!File.Exists(_policiesPath))
        {
            _logger.LogWarning("Policies file not found at {Path}, returning empty list", _policiesPath);
            _cachedPolicies = [];
            return _cachedPolicies;
        }

        try
        {
            var json = File.ReadAllText(_policiesPath);
            _cachedPolicies = JsonSerializer.Deserialize<List<CompliancePolicy>>(json, JsonOptions) ?? [];
            _logger.LogInformation("Loaded {Count} compliance policies from {Path}", _cachedPolicies.Count, _policiesPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load policies from {Path}", _policiesPath);
            _cachedPolicies = [];
        }

        return _cachedPolicies;
    }
}
