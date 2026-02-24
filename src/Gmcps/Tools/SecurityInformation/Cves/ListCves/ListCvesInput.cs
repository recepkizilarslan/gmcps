
namespace Gmcps.Domain.SecurityInformation.Cves.Inputs;

public sealed class ListCvesInput
{
    [JsonPropertyName("limit")]
    [Range(1, 1000)]
    public int Limit { get; set; } = 50;
}
