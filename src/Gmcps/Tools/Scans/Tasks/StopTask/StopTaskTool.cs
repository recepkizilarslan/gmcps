
namespace Gmcps.Tools.Scans.Tasks.StopTask;

public sealed class StopTaskTool(
    IClient client)
    : ITool<StopTaskInput, OperationOutput>
{
    public async Task<Result<OperationOutput>> ExecuteAsync(StopTaskInput input, CancellationToken ct)
    {

        var response = await client.StopTaskAsync(input.TaskId, ct);

        return response.ToOutput<OperationOutput>();
    }
}
