
namespace Gmcps.Domain.Scans.Tasks.Inputs;

public sealed class CreateTaskInput
{
    [Required]
    [JsonPropertyName("name")]
    [MaxLength(256)]
    public string Name { get; set; } = "";

    [Required]
    [JsonPropertyName("targetId")]
    [MaxLength(128)]
    [GvmId]
    public string TargetId { get; set; } = "";

    [Required]
    [JsonPropertyName("scanConfigId")]
    [MaxLength(128)]
    [GvmId]
    public string ScanConfigId { get; set; } = "";

    [JsonPropertyName("scannerId")]
    [MaxLength(128)]
    [GvmId]
    public string ScannerId { get; set; } = "08b69003-5fc2-4037-a479-93b440211c73";
}
