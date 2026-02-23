using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Gmcps.Inputs;

public sealed class GetTargetsStatusInput : ToolInput
{
    [Required]
    [JsonPropertyName("os")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OsFilterInput Os { get; set; }

    [JsonPropertyName("includeTasks")]
    public bool IncludeTasks { get; set; }

    [JsonPropertyName("includeLastReportSummary")]
    public bool IncludeLastReportSummary { get; set; }
}