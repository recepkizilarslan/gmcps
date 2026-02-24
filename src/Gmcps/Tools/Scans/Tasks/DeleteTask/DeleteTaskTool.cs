
namespace Gmcps.Tools.Scans.Tasks.DeleteTask;

public sealed class DeleteTaskTool(
    IClient client)
    : ITool<DeleteTaskInput, OperationOutput>
{
    public async Task<Result<OperationOutput>> ExecuteAsync(DeleteTaskInput input, CancellationToken ct)
    {

        var response = await client.DeleteTaskAsync(input.TaskId, input.Ultimate, ct);

        return response.ToOutput<OperationOutput>();
    }
}
