using Gmcps.Domain.Scans.Overrides.Inputs;

namespace Gmcps.Tools.Scans.Overrides.DeleteOverride;

public sealed class DeleteOverrideTool(
    IClient client)
    : ITool<DeleteOverrideInput, OperationOutput>
{
    public async Task<Result<OperationOutput>> ExecuteAsync(DeleteOverrideInput input, CancellationToken ct)
    {

        var response = await client.DeleteOverrideAsync(input.OverrideId, input.Ultimate, ct);

        return response.ToOutput<OperationOutput>();
    }
}
