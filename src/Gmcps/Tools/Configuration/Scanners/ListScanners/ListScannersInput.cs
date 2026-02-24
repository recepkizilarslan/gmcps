
namespace Gmcps.Tools.Configuration.Scanners.ListScanners;

public sealed class ListScannersInput
{
    [JsonPropertyName("limit")]
    [Range(1, 1000)]
    public int Limit { get; set; } = 50;
}
