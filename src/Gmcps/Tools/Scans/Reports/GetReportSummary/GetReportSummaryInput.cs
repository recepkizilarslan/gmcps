
namespace Gmcps.Domain.Scans.Reports.Inputs;

public sealed class GetReportSummaryInput
{
    [Required]
    [JsonPropertyName("reportId")]
    [MaxLength(128)]
    [GvmId]
    public string ReportId { get; set; } = "";
}
