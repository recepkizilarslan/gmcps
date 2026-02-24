
namespace Gmcps.Tools.Assets.Hosts.ListHostAssets;

public sealed class ListHostAssetsInput
{
    [JsonPropertyName("limit")]
    [Range(1, 1000)]
    public int Limit { get; set; } = 50;
}
