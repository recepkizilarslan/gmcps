using Gmcps.Core;
using Gmcps.Domain;
using Gmcps.Domain.Scans.Reports.Inputs;
using Gmcps.Domain.Scans.Reports.Outputs;
using Gmcps.Infrastructure.Security;
using Gmcps.Models;

namespace Gmcps.Infrastructure.Scans.Reports.GetReportSummary;

public sealed class GetReportSummaryTool(
    IClient<GetReportSummaryClientRequest, Report> client,
    IExample<Report, GetReportSummaryOutput> parser)
    : ITool<GetReportSummaryInput, GetReportSummaryOutput>
{
    public async Task<Result<GetReportSummaryOutput>> ExecuteAsync(GetReportSummaryInput input, CancellationToken ct)
    {
        var idValidation = InputValidator.ValidateId(input.ReportId, "reportId");
        if (idValidation.IsFailure)
        {
            return Result<GetReportSummaryOutput>.Failure(idValidation.Error);
        }

        var response = await client.SendAsync(new GetReportSummaryClientRequest(input.ReportId), ct);
        if (response.IsFailure)
        {
            return Result<GetReportSummaryOutput>.Failure(response.Error);
        }

        return parser.Parse(response.Value);
    }
}
