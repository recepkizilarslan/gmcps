
namespace Gmcps.Tools.Configuration.Targets.Metadata.SetTargetMetadata;

public sealed class SetTargetMetadataInput
{
    [Required]
    [JsonPropertyName("targetId")]
    [MaxLength(128)]
    [GvmId]
    public string TargetId { get; set; } = "";

    [Required]
    [JsonPropertyName("os")]
    public MetadataOsInput Os { get; set; }

    [Required]
    [JsonPropertyName("criticality")]
    public MetadataCriticalityInput Criticality { get; set; }

    [JsonPropertyName("tags")]
    [MaxLength(50)]
    public List<string>? Tags { get; set; }
}
