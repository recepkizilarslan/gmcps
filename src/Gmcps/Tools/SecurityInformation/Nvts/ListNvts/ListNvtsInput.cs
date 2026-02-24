
namespace Gmcps.Domain.SecurityInformation.Nvts.Inputs;

public sealed class ListNvtsInput
{
    [JsonPropertyName("limit")]
    [Range(1, 1000)]
    public int Limit { get; set; } = 50;
}
