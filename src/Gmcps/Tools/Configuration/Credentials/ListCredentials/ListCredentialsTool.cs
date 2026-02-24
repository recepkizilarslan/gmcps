
namespace Gmcps.Tools.Configuration.Credentials.ListCredentials;

public sealed class ListCredentialsTool(
    IClient client)
    : ITool<ListCredentialsInput, ListCredentialsOutput>
{
    public async Task<Result<ListCredentialsOutput>> ExecuteAsync(ListCredentialsInput input, CancellationToken ct)
    {

        var response = await client.GetCredentialsAsync(input.Limit, ct);

        return response.ToOutput<ListCredentialsOutput>();
    }
}
