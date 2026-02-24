using Gmcps.Domain.Resilience.ComplianceAuditReports.Inputs;
using Gmcps.Domain.Resilience.ComplianceAuditReports.Outputs;

namespace Gmcps.Tools.Resilience.ComplianceAuditReports.ListComplianceAuditReports;

public sealed class ListComplianceAuditReportsTool(
    IClient client)
    : ITool<ListComplianceAuditReportsInput, ListComplianceAuditReportsOutput>
{
    public async Task<Result<ListComplianceAuditReportsOutput>> ExecuteAsync(ListComplianceAuditReportsInput input, CancellationToken ct)
    {

        var response = await client.GetComplianceAuditReportsAsync(input.Limit, ct);

        return response.ToOutput<ListComplianceAuditReportsOutput>();
    }
}
