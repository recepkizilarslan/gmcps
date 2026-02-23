using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Gmcps.Inputs;

public sealed class ListCriticalVulnerabilitiesInput : ToolInput
{
    [JsonPropertyName("scope")]
    public VulnScopeInput? Scope { get; set; }

    [JsonPropertyName("minSeverity")]
    [Range(0.0, 10.0)]
    public double MinSeverity { get; set; } = 9.0;

    [JsonPropertyName("limit")]
    [Range(1, 1000)]
    public int Limit { get; set; } = 50;
}