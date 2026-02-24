
namespace Gmcps.Domain.Scans.Reports.Inputs;

public sealed class DeleteReportInput
{
    [Required]
    [MaxLength(128)]
    [JsonPropertyName("reportId")]
    [GvmId]
    public string ReportId { get; set; } = "";
}
