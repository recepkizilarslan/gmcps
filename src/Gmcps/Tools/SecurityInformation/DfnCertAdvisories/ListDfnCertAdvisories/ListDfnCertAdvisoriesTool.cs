using Gmcps.Domain.SecurityInformation.DfnCertAdvisories.Inputs;
using Gmcps.Domain.SecurityInformation.DfnCertAdvisories.Outputs;

namespace Gmcps.Tools.SecurityInformation.DfnCertAdvisories.ListDfnCertAdvisories;

public sealed class ListDfnCertAdvisoriesTool(
    IClient client)
    : ITool<ListDfnCertAdvisoriesInput, ListDfnCertAdvisoriesOutput>
{
    public async Task<Result<ListDfnCertAdvisoriesOutput>> ExecuteAsync(ListDfnCertAdvisoriesInput input, CancellationToken ct)
    {

        var response = await client.GetSecurityInfosAsync("DFN_CERT_ADV", input.Limit, ct);

        return response.ToOutput<ListDfnCertAdvisoriesOutput>();
    }
}
