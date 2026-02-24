
namespace Gmcps.Domain.Scans.Tasks.Inputs;

public sealed class DeleteTaskInput
{
    [Required]
    [MaxLength(128)]
    [JsonPropertyName("taskId")]
    [GvmId]
    public string TaskId { get; set; } = "";

    [JsonPropertyName("ultimate")]
    public bool Ultimate { get; set; } = true;
}
