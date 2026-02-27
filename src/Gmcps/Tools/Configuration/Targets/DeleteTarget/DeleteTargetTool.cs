
namespace Gmcps.Tools.Configuration.Targets.DeleteTarget;

public sealed class DeleteTargetTool(
    IClient client)
    : ITool<DeleteTargetInput, OperationOutput>
{
    public async Task<Result<OperationOutput>> ExecuteAsync(DeleteTargetInput input, CancellationToken ct)
    {
        var response = await client.DeleteTargetAsync(input.TargetId, input.Ultimate, ct);
        return response.ToOutput<OperationOutput>();
    }
}

