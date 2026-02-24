
namespace Gmcps.Tools.Scans.Tasks.ResumeTask;

public sealed class ResumeTaskTool(
    IClient client)
    : ITool<ResumeTaskInput, ResumeTaskOutput>
{
    public async Task<Result<ResumeTaskOutput>> ExecuteAsync(ResumeTaskInput input, CancellationToken ct)
    {

        var response = await client.ResumeTaskAsync(input.TaskId, ct);

        return response.ToOutput<ResumeTaskOutput>();
    }
}
