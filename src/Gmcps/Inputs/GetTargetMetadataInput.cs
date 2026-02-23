using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Gmcps.Inputs;

public sealed class GetTargetMetadataInput : ToolInput
{
    [Required]
    [JsonPropertyName("targetId")]
    [MaxLength(128)]
    public string TargetId { get; set; } = "";
}