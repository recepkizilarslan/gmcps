using Gmcps.Domain.Scans.Overrides.Outputs;

namespace Gmcps.Tools.Scans.Overrides.ListOverrides;

public sealed class ListOverridesTool(
    IClient client)
    : ITool<ListOverridesInput, ListOverridesOutput>
{
    public async Task<Result<ListOverridesOutput>> ExecuteAsync(ListOverridesInput input, CancellationToken ct)
    {

        var response = await client.GetOverridesAsync(input.Limit, ct);

        return response.ToOutput<ListOverridesOutput>();
    }
}
