
namespace Gmcps.Domain.Scans.Overrides.Inputs;

public sealed class DeleteOverrideInput
{
    [Required]
    [JsonPropertyName("overrideId")]
    [MaxLength(128)]
    [GvmId]
    public string OverrideId { get; set; } = "";

    [JsonPropertyName("ultimate")]
    public bool Ultimate { get; set; } = true;
}
