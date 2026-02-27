
namespace Gmcps.Tools.Configuration.Users.ListUsers;

public sealed class ListUsersTool(
    IClient client)
    : ITool<ListUsersInput, ListUsersOutput>
{
    public async Task<Result<ListUsersOutput>> ExecuteAsync(ListUsersInput input, CancellationToken ct)
    {
        var response = await client.GetUsersAsync(input.Limit, ct);
        return response.ToOutput<ListUsersOutput>();
    }
}

