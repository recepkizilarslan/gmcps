using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Gmcps.Domain.Scans.Reports.Inputs;

public sealed class GetReportSummaryInput
{
    [Required]
    [JsonPropertyName("reportId")]
    [MaxLength(128)]
    public string ReportId { get; set; } = "";
}
