using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Gmcps.Inputs;

public sealed class CreateTargetInput : ToolInput
{
    [Required]
    [JsonPropertyName("name")]
    [MaxLength(256)]
    public string Name { get; set; } = "";

    [Required]
    [JsonPropertyName("hosts")]
    [MaxLength(4096)]
    public string Hosts { get; set; } = "";

    [JsonPropertyName("comment")]
    [MaxLength(1024)]
    public string? Comment { get; set; }
}