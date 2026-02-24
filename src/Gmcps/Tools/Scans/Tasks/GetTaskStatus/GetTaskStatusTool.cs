
namespace Gmcps.Tools.Scans.Tasks.GetTaskStatus;

public sealed class GetTaskStatusTool(
    IClient client)
    : ITool<GetTaskStatusInput, GetTaskStatusOutput>
{
    public async Task<Result<GetTaskStatusOutput>> ExecuteAsync(GetTaskStatusInput input, CancellationToken ct)
    {

        var response = await client.GetTaskStatusAsync(input.TaskId, ct);

        return response.ToOutput<GetTaskStatusOutput>();
    }
}
