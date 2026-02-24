using Gmcps.Domain.SecurityInformation.Cves.Inputs;
using Gmcps.Domain.SecurityInformation.Cves.Outputs;

namespace Gmcps.Tools.SecurityInformation.Cves.ListCves;

public sealed class ListCvesTool(
    IClient client)
    : ITool<ListCvesInput, ListCvesOutput>
{
    public async Task<Result<ListCvesOutput>> ExecuteAsync(ListCvesInput input, CancellationToken ct)
    {

        var response = await client.GetSecurityInfosAsync("CVE", input.Limit, ct);

        return response.ToOutput<ListCvesOutput>();
    }
}
