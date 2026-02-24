
namespace Gmcps.Tools.Configuration.Scanners.ListScanners;

public sealed class ListScannersTool(
    IClient client)
    : ITool<ListScannersInput, ListScannersOutput>
{
    public async Task<Result<ListScannersOutput>> ExecuteAsync(ListScannersInput input, CancellationToken ct)
    {

        var response = await client.GetScannersAsync(input.Limit, ct);

        return response.ToOutput<ListScannersOutput>();
    }
}
