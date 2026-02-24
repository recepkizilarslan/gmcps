
namespace Gmcps.Tools.Configuration.ReportConfigs.ListReportConfigs;

public sealed class ListReportConfigsInput
{
    [JsonPropertyName("limit")]
    [Range(1, 1000)]
    public int Limit { get; set; } = 50;
}
