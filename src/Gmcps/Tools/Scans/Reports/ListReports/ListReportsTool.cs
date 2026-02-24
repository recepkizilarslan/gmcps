using Gmcps.Domain.Scans.Reports.Inputs;

namespace Gmcps.Tools.Scans.Reports.ListReports;

public sealed class ListReportsTool(
    IClient client)
    : ITool<ListReportsInput, ListReportsOutput>
{
    public async Task<Result<ListReportsOutput>> ExecuteAsync(ListReportsInput input, CancellationToken ct)
    {

        var response = await client.GetReportsAsync(input.Limit, "scan", ct);

        return response.ToOutput<ListReportsOutput>();
    }
}
