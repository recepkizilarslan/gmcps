using Gmcps.Domain.Resilience.ComplianceAudits.Inputs;
using Gmcps.Domain.Resilience.ComplianceAudits.Outputs;

namespace Gmcps.Tools.Resilience.ComplianceAudits.ListComplianceAudits;

public sealed class ListComplianceAuditsTool(
    IClient client)
    : ITool<ListComplianceAuditsInput, ListComplianceAuditsOutput>
{
    public async Task<Result<ListComplianceAuditsOutput>> ExecuteAsync(ListComplianceAuditsInput input, CancellationToken ct)
    {

        var response = await client.GetTasksAsync(input.Limit, "audit", ct);

        return response.ToOutput<ListComplianceAuditsOutput>();
    }
}
