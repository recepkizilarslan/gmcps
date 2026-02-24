
namespace Gmcps.Domain.Scans.Tasks.Inputs;

public sealed class GetTaskStatusInput
{
    [Required]
    [JsonPropertyName("taskId")]
    [MaxLength(128)]
    [GvmId]
    public string TaskId { get; set; } = "";
}
