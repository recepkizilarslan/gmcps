using Gmcps.Domain;
using Gmcps.Core.Abstractions;
using Gmcps.Domain.Models;
using Gmcps.Tools.Core;

namespace Gmcps.Tools.Configuration.Filters.ListFilters;

public sealed class ListFiltersTool(
    IClient client)
    : ITool<ListFiltersInput, ListFiltersOutput>
{
    public async Task<Result<ListFiltersOutput>> ExecuteAsync(ListFiltersInput input, CancellationToken ct)
    {

        var response = await client.GetFiltersAsync(input.Limit, ct);

        return response.ToOutput<ListFiltersOutput>();
    }
}
