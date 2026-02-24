
namespace Gmcps.Tools.Scans.Tasks.CreateTask;

public sealed class CreateTaskTool(
    IClient client)
    : ITool<CreateTaskInput, CreateTaskOutput>
{
    public async Task<Result<CreateTaskOutput>> ExecuteAsync(CreateTaskInput input, CancellationToken ct)
    {

        var response = await client.CreateTaskAsync(
            input.Name,
            input.TargetId,
            input.ScanConfigId,
            input.ScannerId,
            ct);

        return response.ToOutput<CreateTaskOutput>();
    }
}
