
namespace Gmcps.Tools.Assets.Hosts.ListHostAssets;

public sealed class ListHostAssetsTool(
    IClient client)
    : ITool<ListHostAssetsInput, ListHostAssetsOutput>
{
    public async Task<Result<ListHostAssetsOutput>> ExecuteAsync(ListHostAssetsInput input, CancellationToken ct)
    {

        var response = await client.GetHostAssetsAsync(input.Limit, ct);

        return Result<ListHostAssetsOutput>.Success(new ListHostAssetsOutput(
            response.Value.Select(h => new HostAssetOutput(
                Id: h.Id,
                Name: h.Name,
                Ip: h.Ip,
                OperatingSystem: h.OperatingSystem,
                Severity: h.Severity)).ToList()));
    }
}
