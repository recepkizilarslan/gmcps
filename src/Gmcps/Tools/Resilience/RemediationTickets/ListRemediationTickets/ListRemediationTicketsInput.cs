
namespace Gmcps.Domain.Resilience.RemediationTickets.Inputs;

public sealed class ListRemediationTicketsInput
{
    [JsonPropertyName("limit")]
    [Range(1, 1000)]
    public int Limit { get; set; } = 50;
}
