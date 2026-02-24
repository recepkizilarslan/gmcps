
namespace Gmcps.Infrastructure.Stores;

public interface ICompliancePolicyStore
{
    Task<Result<CompliancePolicy>> GetPolicyAsync(string policyId, CancellationToken ct);

    Task<Result<IReadOnlyList<CompliancePolicy>>> GetAllPoliciesAsync(CancellationToken ct);
}
