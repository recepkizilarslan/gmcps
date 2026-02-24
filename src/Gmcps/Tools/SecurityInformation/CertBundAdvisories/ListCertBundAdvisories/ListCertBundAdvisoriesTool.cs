using Gmcps.Domain.SecurityInformation.CertBundAdvisories.Inputs;
using Gmcps.Domain.SecurityInformation.CertBundAdvisories.Outputs;

namespace Gmcps.Tools.SecurityInformation.CertBundAdvisories.ListCertBundAdvisories;

public sealed class ListCertBundAdvisoriesTool(
    IClient client)
    : ITool<ListCertBundAdvisoriesInput, ListCertBundAdvisoriesOutput>
{
    public async Task<Result<ListCertBundAdvisoriesOutput>> ExecuteAsync(ListCertBundAdvisoriesInput input, CancellationToken ct)
    {

        var response = await client.GetSecurityInfosAsync("CERT_BUND_ADV", input.Limit, ct);

        return response.ToOutput<ListCertBundAdvisoriesOutput>();
    }
}
