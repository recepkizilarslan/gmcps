
namespace Gmcps.Domain.Scans.Targets.Inputs;

public sealed class GetTargetsStatusInput
{
    [Required]
    [JsonPropertyName("os")]
    public OsFilter Os { get; set; } = OsFilter.Any;

    [JsonPropertyName("includeTasks")]
    public bool IncludeTasks { get; set; }

    [JsonPropertyName("includeLastReportSummary")]
    public bool IncludeLastReportSummary { get; set; }
}
