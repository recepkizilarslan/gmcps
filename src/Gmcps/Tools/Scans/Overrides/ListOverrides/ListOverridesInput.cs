
namespace Gmcps.Domain.Scans.Overrides.Inputs;

public sealed class ListOverridesInput
{
    [JsonPropertyName("limit")]
    [Range(1, 1000)]
    public int Limit { get; set; } = 50;
}
