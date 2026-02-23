using Gmcps.Core;
using Gmcps.Domain;
using Gmcps.Domain.Scans.Reports.Outputs;
using Gmcps.Models;

namespace Gmcps.Infrastructure.Scans.Reports.GetReportSummary;

public sealed class GetReportSummaryParser : IExample<Report, GetReportSummaryOutput>
{
    public Result<GetReportSummaryOutput> Parse(Report raw) =>
        Result<GetReportSummaryOutput>.Success(
            new GetReportSummaryOutput(
                ReportId: raw.Id,
                TaskId: raw.TaskId,
                Timestamp: raw.Timestamp,
                Summary: new ReportSeveritySummaryOutput(
                    High: raw.Summary.High,
                    Medium: raw.Summary.Medium,
                    Low: raw.Summary.Low,
                    Log: raw.Summary.Log)));
}
