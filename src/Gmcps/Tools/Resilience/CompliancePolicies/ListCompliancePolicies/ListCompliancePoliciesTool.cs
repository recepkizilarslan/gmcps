using Gmcps.Domain.Resilience.CompliancePolicies.Inputs;
using Gmcps.Domain.Resilience.CompliancePolicies.Outputs;

namespace Gmcps.Tools.Resilience.CompliancePolicies.ListCompliancePolicies;

public sealed class ListCompliancePoliciesTool(
    ICompliancePolicyStore policyStore)
    : ITool<ListCompliancePoliciesInput, ListCompliancePoliciesOutput>
{
    public async Task<Result<ListCompliancePoliciesOutput>> ExecuteAsync(ListCompliancePoliciesInput input, CancellationToken ct)
    {
        var response = await policyStore.GetAllPoliciesAsync(ct);

        return response.ToOutput<ListCompliancePoliciesOutput>();
    }
}
