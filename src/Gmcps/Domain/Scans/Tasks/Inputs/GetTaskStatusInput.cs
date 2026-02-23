using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Gmcps.Domain.Scans.Tasks.Inputs;

public sealed class GetTaskStatusInput
{
    [Required]
    [JsonPropertyName("taskId")]
    [MaxLength(128)]
    public string TaskId { get; set; } = "";
}
