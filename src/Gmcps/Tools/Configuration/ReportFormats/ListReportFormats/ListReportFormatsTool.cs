
namespace Gmcps.Tools.Configuration.ReportFormats.ListReportFormats;

public sealed class ListReportFormatsTool(
    IClient client)
    : ITool<ListReportFormatsInput, ListReportFormatsOutput>
{
    public async Task<Result<ListReportFormatsOutput>> ExecuteAsync(ListReportFormatsInput input, CancellationToken ct)
    {

        var response = await client.GetReportFormatsAsync(input.Limit, ct);

        return response.ToOutput<ListReportFormatsOutput>();
    }
}
