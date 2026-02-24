
namespace Gmcps.Tools.Configuration.ScanConfigs.ListScanConfigs;

public sealed class ListScanConfigsTool(
    IClient client)
    : ITool<EmptyInput, ListScanConfigsOutput>
{
    public async Task<Result<ListScanConfigsOutput>> ExecuteAsync(EmptyInput input, CancellationToken ct)
    {
        var response = await client.GetScanConfigsAsync(ct);

        return response.ToOutput<ListScanConfigsOutput>();
    }
}
