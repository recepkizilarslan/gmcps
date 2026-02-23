using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Gmcps.Inputs;

public sealed class CreateTaskInput : ToolInput
{
    [Required]
    [JsonPropertyName("name")]
    [MaxLength(256)]
    public string Name { get; set; } = "";

    [Required]
    [JsonPropertyName("targetId")]
    [MaxLength(128)]
    public string TargetId { get; set; } = "";

    [Required]
    [JsonPropertyName("scanConfigId")]
    [MaxLength(128)]
    public string ScanConfigId { get; set; } = "";

    [JsonPropertyName("scannerId")]
    [MaxLength(128)]
    public string ScannerId { get; set; } = "08b69003-5fc2-4037-a479-93b440211c73"; // OpenVAS default
}