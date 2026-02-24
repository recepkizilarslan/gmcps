using Gmcps.Domain.Scans.Tasks.Inputs;
using Gmcps.Domain.Scans.Tasks.Outputs;

namespace Gmcps.Tools.Scans.Tasks.StartTask;

public sealed class StartTaskTool(
    IClient client)
    : ITool<StartTaskInput, StartTaskOutput>
{
    public async Task<Result<StartTaskOutput>> ExecuteAsync(StartTaskInput input, CancellationToken ct)
    {

        var response = await client.StartTaskAsync(input.TaskId, ct);

        return response.ToOutput<StartTaskOutput>();
    }
}
