
namespace Gmcps.Domain.Resilience.ComplianceAuditReports.Inputs;

public sealed class ListComplianceAuditReportsInput
{
    [JsonPropertyName("limit")]
    [Range(1, 1000)]
    public int Limit { get; set; } = 50;
}
