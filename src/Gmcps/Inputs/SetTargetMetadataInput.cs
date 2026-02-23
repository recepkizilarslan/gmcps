using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Gmcps.Inputs;

public sealed class SetTargetMetadataInput : ToolInput
{
    [Required]
    [JsonPropertyName("targetId")]
    [MaxLength(128)]
    public string TargetId { get; set; } = "";

    [Required]
    [JsonPropertyName("os")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OsTypeInput Os { get; set; }

    [Required]
    [JsonPropertyName("criticality")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CriticalityInput Criticality { get; set; }

    [JsonPropertyName("tags")]
    [MaxLength(50)]
    public List<string>? Tags { get; set; }
}