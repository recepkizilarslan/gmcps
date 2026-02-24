
namespace Gmcps.Tools.Scans.Overrides.ModifyOverride;

public sealed class ModifyOverrideTool(
    IClient client)
    : ITool<ModifyOverrideInput, OperationOutput>
{
    public async Task<Result<OperationOutput>> ExecuteAsync(ModifyOverrideInput input, CancellationToken ct)
    {

        var response = await client.ModifyOverrideAsync(
            input.OverrideId,
            input.Text,
            input.NvtOid,
            input.NewSeverity,
            input.ResultId,
            input.TaskId,
            input.Hosts,
            input.Port,
            input.Severity,
            input.ActiveDays,
            ct);

        return response.ToOutput<OperationOutput>();
    }
}
