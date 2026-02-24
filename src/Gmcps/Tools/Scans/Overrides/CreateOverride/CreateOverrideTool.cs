
namespace Gmcps.Tools.Scans.Overrides.CreateOverride;

public sealed class CreateOverrideTool(
    IClient client)
    : ITool<CreateOverrideInput, CreateOverrideOutput>
{
    public async Task<Result<CreateOverrideOutput>> ExecuteAsync(CreateOverrideInput input, CancellationToken ct)
    {

        var response = await client.CreateOverrideAsync(
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

        return response.ToOutput<CreateOverrideOutput>();
    }
}
