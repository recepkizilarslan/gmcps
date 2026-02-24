using Gmcps.Domain.SecurityInformation.Cpes.Inputs;
using Gmcps.Domain.SecurityInformation.Cpes.Outputs;

namespace Gmcps.Tools.SecurityInformation.Cpes.ListCpes;

public sealed class ListCpesTool(
    IClient client)
    : ITool<ListCpesInput, ListCpesOutput>
{
    public async Task<Result<ListCpesOutput>> ExecuteAsync(ListCpesInput input, CancellationToken ct)
    {

        var response = await client.GetSecurityInfosAsync("CPE", input.Limit, ct);

        return response.ToOutput<ListCpesOutput>();
    }
}
