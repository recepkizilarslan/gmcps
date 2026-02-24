
namespace Gmcps.Domain.Resilience.Compliance.Inputs;

public sealed class IsTargetCompliantInput
{
    [Required]
    [JsonPropertyName("targetId")]
    [MaxLength(128)]
    [GvmId]
    public string TargetId { get; set; } = "";

    [Required]
    [JsonPropertyName("policyId")]
    [MaxLength(128)]
    [GvmId]
    public string PolicyId { get; set; } = "";
}
