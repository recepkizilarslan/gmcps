using Gmcps.Domain.SecurityInformation.Nvts.Inputs;
using Gmcps.Domain.SecurityInformation.Nvts.Outputs;

namespace Gmcps.Tools.SecurityInformation.Nvts.ListNvts;

public sealed class ListNvtsTool(
    IClient client)
    : ITool<ListNvtsInput, ListNvtsOutput>
{
    public async Task<Result<ListNvtsOutput>> ExecuteAsync(ListNvtsInput input, CancellationToken ct)
    {

        var response = await client.GetNvtsAsync(input.Limit, ct);

        return response.ToOutput<ListNvtsOutput>();
    }
}
