
namespace Gmcps.Tools.Assets.OperatingSystems.ListOperatingSystemAssets;

public sealed class ListOperatingSystemAssetsInput
{
    [JsonPropertyName("limit")]
    [Range(1, 1000)]
    public int Limit { get; set; } = 50;
}
