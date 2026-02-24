
namespace Gmcps.Tools.Configuration.Tags.ListTags;

public sealed class ListTagsTool(
    IClient client)
    : ITool<ListTagsInput, ListTagsOutput>
{
    public async Task<Result<ListTagsOutput>> ExecuteAsync(ListTagsInput input, CancellationToken ct)
    {

        var response = await client.GetTagsAsync(input.Limit, ct);

        return response.ToOutput<ListTagsOutput>();
    }
}
