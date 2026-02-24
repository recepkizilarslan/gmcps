
namespace Gmcps.Domain.Resilience.ComplianceAudits.Inputs;

public sealed class ListComplianceAuditsInput
{
    [JsonPropertyName("limit")]
    [Range(1, 1000)]
    public int Limit { get; set; } = 50;
}
