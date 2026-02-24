
namespace Gmcps.Domain.Scans.Overrides.Inputs;

public sealed class CreateOverrideInput
{
    [Required]
    [JsonPropertyName("text")]
    [MaxLength(4096)]
    public string Text { get; set; } = "";

    [Required]
    [JsonPropertyName("nvtOid")]
    [MaxLength(128)]
    [GvmOid]
    public string NvtOid { get; set; } = "";

    [JsonPropertyName("newSeverity")]
    [Range(0.0, 10.0)]
    public double? NewSeverity { get; set; }

    [JsonPropertyName("resultId")]
    [MaxLength(128)]
    [GvmId]
    public string? ResultId { get; set; }

    [JsonPropertyName("taskId")]
    [MaxLength(128)]
    [GvmId]
    public string? TaskId { get; set; }

    [JsonPropertyName("hosts")]
    [MaxLength(1024)]
    public string? Hosts { get; set; }

    [JsonPropertyName("port")]
    [MaxLength(256)]
    public string? Port { get; set; }

    [JsonPropertyName("severity")]
    [Range(0.0, 10.0)]
    public double? Severity { get; set; }

    [JsonPropertyName("activeDays")]
    [Range(-1, 3650)]
    public int? ActiveDays { get; set; }
}
