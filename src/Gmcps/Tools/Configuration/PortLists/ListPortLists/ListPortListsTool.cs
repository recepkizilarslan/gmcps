
namespace Gmcps.Tools.Configuration.PortLists.ListPortLists;

public sealed class ListPortListsTool(
    IClient client)
    : ITool<ListPortListsInput, ListPortListsOutput>
{
    public async Task<Result<ListPortListsOutput>> ExecuteAsync(ListPortListsInput input, CancellationToken ct)
    {

        var response = await client.GetPortListsAsync(input.Limit, ct);

        return response.ToOutput<ListPortListsOutput>();
    }
}
