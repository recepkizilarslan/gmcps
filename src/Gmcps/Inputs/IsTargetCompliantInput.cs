using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Gmcps.Inputs;

public sealed class IsTargetCompliantInput : ToolInput
{
    [Required]
    [JsonPropertyName("targetId")]
    [MaxLength(128)]
    public string TargetId { get; set; } = "";

    [Required]
    [JsonPropertyName("policyId")]
    [MaxLength(128)]
    public string PolicyId { get; set; } = "";
}
