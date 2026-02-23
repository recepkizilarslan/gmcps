using Gmcps.Core;
using Gmcps.Domain;
using Gmcps.Domain.Scans.Tasks.Outputs;
using Gmcps.Models;

namespace Gmcps.Infrastructure.Scans.Tasks.GetTaskStatus;

public sealed class GetTaskStatusParser : IExample<GvmTask, GetTaskStatusOutput>
{
    public Result<GetTaskStatusOutput> Parse(GvmTask raw) =>
        Result<GetTaskStatusOutput>.Success(
            new GetTaskStatusOutput(
                TaskId: raw.Id,
                Name: raw.Name,
                Status: raw.Status,
                Progress: raw.Progress,
                LastReportId: raw.LastReportId));
}
