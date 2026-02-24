
namespace Gmcps.Tools.Configuration.ReportFormats.ListReportFormats;

public sealed class ListReportFormatsInput
{
    [JsonPropertyName("limit")]
    [Range(1, 1000)]
    public int Limit { get; set; } = 50;
}
