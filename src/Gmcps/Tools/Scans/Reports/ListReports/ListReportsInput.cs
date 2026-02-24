
namespace Gmcps.Domain.Scans.Reports.Inputs;

public sealed class ListReportsInput
{
    [JsonPropertyName("limit")]
    [Range(1, 1000)]
    public int Limit { get; set; } = 50;
}
