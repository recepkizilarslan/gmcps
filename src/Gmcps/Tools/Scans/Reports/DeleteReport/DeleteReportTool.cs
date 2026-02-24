
namespace Gmcps.Tools.Scans.Reports.DeleteReport;

public sealed class DeleteReportTool(
    IClient client)
    : ITool<DeleteReportInput, OperationOutput>
{
    public async Task<Result<OperationOutput>> ExecuteAsync(DeleteReportInput input, CancellationToken ct)
    {

        var response = await client.DeleteReportAsync(input.ReportId, ct);

        return response.ToOutput<OperationOutput>();
    }
}
