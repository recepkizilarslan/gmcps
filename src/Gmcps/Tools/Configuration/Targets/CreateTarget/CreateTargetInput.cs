
namespace Gmcps.Tools.Configuration.Targets.CreateTarget;

public sealed class CreateTargetInput
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
