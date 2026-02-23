using Gmcps.Core;
using Gmcps.Domain;
using Gmcps.Domain.Interfaces;
using Gmcps.Models;

namespace Gmcps.Infrastructure.Scans.Reports.GetReportSummary;

public sealed class GetReportSummaryClient(IGmpClient gmpClient)
    : IClient<GetReportSummaryClientRequest, Report>
{
    public Task<Result<Report>> SendAsync(GetReportSummaryClientRequest request, CancellationToken ct) =>
        gmpClient.GetReportSummaryAsync(request.ReportId, ct);
}
