using Gmcps.Domain.Scans.Results.Inputs;
using Gmcps.Domain.Scans.Results.Outputs;

namespace Gmcps.Tools.Scans.Results.ListResults;

public sealed class ListResultsTool(
    IClient client)
    : ITool<ListResultsInput, ListResultsOutput>
{
    public async Task<Result<ListResultsOutput>> ExecuteAsync(ListResultsInput input, CancellationToken ct)
    {

        var response = await client.GetResultsAsync(input.Limit, ct);

        return response.ToOutput<ListResultsOutput>();
    }
}
