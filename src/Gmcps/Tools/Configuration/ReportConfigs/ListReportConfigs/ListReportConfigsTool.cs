
namespace Gmcps.Tools.Configuration.ReportConfigs.ListReportConfigs;

public sealed class ListReportConfigsTool(
    IClient client)
    : ITool<ListReportConfigsInput, ListReportConfigsOutput>
{
    public async Task<Result<ListReportConfigsOutput>> ExecuteAsync(ListReportConfigsInput input, CancellationToken ct)
    {

        var response = await client.GetReportConfigsAsync(input.Limit, ct);

        return response.ToOutput<ListReportConfigsOutput>();
    }
}
