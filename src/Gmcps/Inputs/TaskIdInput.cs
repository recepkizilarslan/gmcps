using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Gmcps.Inputs;

public sealed class TaskIdInput : ToolInput
{
    [Required]
    [JsonPropertyName("taskId")]
    [MaxLength(128)]
    public string TaskId { get; set; } = "";
}