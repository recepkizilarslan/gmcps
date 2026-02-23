using Gmcps.Core;
using Gmcps.Domain;
using Gmcps.Domain.Interfaces;
using Gmcps.Models;

namespace Gmcps.Infrastructure.Scans.Tasks.GetTaskStatus;

public sealed class GetTaskStatusClient(IGmpClient gmpClient)
    : IClient<GetTaskStatusClientRequest, GvmTask>
{
    public Task<Result<GvmTask>> SendAsync(GetTaskStatusClientRequest request, CancellationToken ct) =>
        gmpClient.GetTaskStatusAsync(request.TaskId, ct);
}
