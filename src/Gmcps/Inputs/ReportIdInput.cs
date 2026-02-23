using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Gmcps.Inputs;

public sealed class ReportIdInput : ToolInput
{
    [Required]
    [JsonPropertyName("reportId")]
    [MaxLength(128)]
    public string ReportId { get; set; } = "";
}