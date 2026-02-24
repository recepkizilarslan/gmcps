
namespace Gmcps.Domain.Scans.Tasks.Inputs;

public sealed class ResumeTaskInput
{
    [Required]
    [MaxLength(128)]
    [JsonPropertyName("taskId")]
    [GvmId]
    public string TaskId { get; set; } = "";
}
