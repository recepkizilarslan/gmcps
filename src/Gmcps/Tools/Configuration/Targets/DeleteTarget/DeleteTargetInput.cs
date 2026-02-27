
namespace Gmcps.Tools.Configuration.Targets.DeleteTarget;

public sealed class DeleteTargetInput
{
    [Required]
    [MaxLength(128)]
    [JsonPropertyName("targetId")]
    [GvmId]
    public string TargetId { get; set; } = "";

    [JsonPropertyName("ultimate")]
    public bool Ultimate { get; set; } = true;
}

