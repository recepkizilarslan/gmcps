
namespace Gmcps.Tools.Assets.TlsCertificates.ListTlsCertificates;

public sealed class ListTlsCertificatesTool(
    IClient client)
    : ITool<ListTlsCertificatesInput, ListTlsCertificatesOutput>
{
    public async Task<Result<ListTlsCertificatesOutput>> ExecuteAsync(ListTlsCertificatesInput input, CancellationToken ct)
    {

        var response = await client.GetTlsCertificatesAsync(input.Limit, ct);

        return response.ToOutput<ListTlsCertificatesOutput>();
    }
}
