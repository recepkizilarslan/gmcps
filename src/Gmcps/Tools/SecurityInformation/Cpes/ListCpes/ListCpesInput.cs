
namespace Gmcps.Domain.SecurityInformation.Cpes.Inputs;

public sealed class ListCpesInput
{
    [JsonPropertyName("limit")]
    [Range(1, 1000)]
    public int Limit { get; set; } = 50;
}
