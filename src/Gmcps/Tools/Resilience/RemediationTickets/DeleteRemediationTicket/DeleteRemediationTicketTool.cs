
namespace Gmcps.Tools.Resilience.RemediationTickets.DeleteRemediationTicket;

public sealed class DeleteRemediationTicketTool(
    IClient client)
    : ITool<DeleteRemediationTicketInput, OperationOutput>
{
    public async Task<Result<OperationOutput>> ExecuteAsync(DeleteRemediationTicketInput input, CancellationToken ct)
    {

        var response = await client.DeleteTicketAsync(input.TicketId, input.Ultimate, ct);

        return response.ToOutput<OperationOutput>();
    }
}
