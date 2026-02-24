
namespace Gmcps.Tools.Configuration.Targets.ListTargets;

public sealed class ListTargetsTool(
    IClient client)
    : ITool<EmptyInput, ListTargetsOutput>
{
    public async Task<Result<ListTargetsOutput>> ExecuteAsync(EmptyInput input, CancellationToken ct)
    {
        var response = await client.GetTargetsAsync(ct);

        return response.ToOutput<ListTargetsOutput>();
    }
}
