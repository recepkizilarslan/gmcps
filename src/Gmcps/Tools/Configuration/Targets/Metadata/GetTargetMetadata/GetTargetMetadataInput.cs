using Gmcps.Domain.Validation;

namespace Gmcps.Tools.Configuration.Targets.Metadata.GetTargetMetadata;

public sealed class GetTargetMetadataInput
{
    [Required]
    [JsonPropertyName("targetId")]
    [MaxLength(128)]
    [GvmId]
    public string TargetId { get; set; } = "";
}
