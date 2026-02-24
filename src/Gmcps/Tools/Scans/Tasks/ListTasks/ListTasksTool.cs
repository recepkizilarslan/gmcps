
namespace Gmcps.Tools.Scans.Tasks.ListTasks;

public sealed class ListTasksTool(
    IClient client)
    : ITool<ListTasksInput, ListTasksOutput>
{
    public async Task<Result<ListTasksOutput>> ExecuteAsync(ListTasksInput input, CancellationToken ct)
    {

        var response = await client.GetTasksAsync(input.Limit, "scan", ct);

        return response.ToOutput<ListTasksOutput>();
    }
}
