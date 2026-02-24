
namespace Gmcps.Domain.Scans.Results.Inputs;

public sealed class ListResultsInput
{
    [JsonPropertyName("limit")]
    [Range(1, 1000)]
    public int Limit { get; set; } = 50;
}
