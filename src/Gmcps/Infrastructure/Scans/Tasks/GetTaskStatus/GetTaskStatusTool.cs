using Gmcps.Core;
using Gmcps.Domain;
using Gmcps.Domain.Scans.Tasks.Inputs;
using Gmcps.Domain.Scans.Tasks.Outputs;
using Gmcps.Infrastructure.Security;
using Gmcps.Models;

namespace Gmcps.Infrastructure.Scans.Tasks.GetTaskStatus;

public sealed class GetTaskStatusTool(
    IClient<GetTaskStatusClientRequest, GvmTask> client,
    IExample<GvmTask, GetTaskStatusOutput> parser)
    : ITool<GetTaskStatusInput, GetTaskStatusOutput>
{
    public async Task<Result<GetTaskStatusOutput>> ExecuteAsync(GetTaskStatusInput input, CancellationToken ct)
    {
        var idValidation = InputValidator.ValidateId(input.TaskId, "taskId");
        if (idValidation.IsFailure)
        {
            return Result<GetTaskStatusOutput>.Failure(idValidation.Error);
        }

        var response = await client.SendAsync(new GetTaskStatusClientRequest(input.TaskId), ct);
        if (response.IsFailure)
        {
            return Result<GetTaskStatusOutput>.Failure(response.Error);
        }

        return parser.Parse(response.Value);
    }
}
