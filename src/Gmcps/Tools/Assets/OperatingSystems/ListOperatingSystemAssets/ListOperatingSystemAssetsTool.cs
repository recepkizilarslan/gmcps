
namespace Gmcps.Tools.Assets.OperatingSystems.ListOperatingSystemAssets;

public sealed class ListOperatingSystemAssetsTool(
    IClient client)
    : ITool<ListOperatingSystemAssetsInput, ListOperatingSystemAssetsOutput>
{
    public async Task<Result<ListOperatingSystemAssetsOutput>> ExecuteAsync(ListOperatingSystemAssetsInput input, CancellationToken ct)
    {

        var response = await client.GetOperatingSystemAssetsAsync(input.Limit, ct);

        return response.ToOutput<ListOperatingSystemAssetsOutput>();
    }
}
