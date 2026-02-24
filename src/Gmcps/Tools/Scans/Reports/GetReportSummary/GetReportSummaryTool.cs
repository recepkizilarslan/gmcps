
namespace Gmcps.Tools.Scans.Reports.GetReportSummary;

public sealed class GetReportSummaryTool(
    IClient client)
    : ITool<GetReportSummaryInput, GetReportSummaryOutput>
{
    public async Task<Result<GetReportSummaryOutput>> ExecuteAsync(GetReportSummaryInput input, CancellationToken ct)
    {

        var response = await client.GetReportSummaryAsync(input.ReportId, ct);

        return response.ToOutput<GetReportSummaryOutput>();
    }
}
