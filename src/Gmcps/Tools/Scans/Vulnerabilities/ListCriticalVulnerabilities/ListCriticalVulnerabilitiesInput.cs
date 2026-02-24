
namespace Gmcps.Domain.Scans.Vulnerabilities.Inputs;

public sealed class ListCriticalVulnerabilitiesInput
{
    [JsonPropertyName("scope")]
    public VulnerabilityScopeInput? Scope { get; set; }

    [JsonPropertyName("minSeverity")]
    [Range(0.0, 10.0)]
    public double MinSeverity { get; set; } = 9.0;

    [JsonPropertyName("limit")]
    [Range(1, 1000)]
    public int Limit { get; set; } = 50;
}
