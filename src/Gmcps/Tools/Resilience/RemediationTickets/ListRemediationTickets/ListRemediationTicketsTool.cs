
namespace Gmcps.Tools.Resilience.RemediationTickets.ListRemediationTickets;

public sealed class ListRemediationTicketsTool(
    IClient client)
    : ITool<ListRemediationTicketsInput, ListRemediationTicketsOutput>
{
    public async Task<Result<ListRemediationTicketsOutput>> ExecuteAsync(ListRemediationTicketsInput input, CancellationToken ct)
    {

        var response = await client.GetTicketsAsync(input.Limit, ct);

        return response.ToOutput<ListRemediationTicketsOutput>();
    }
}
