
namespace Gmcps.Tools.Resilience.RemediationTickets.ModifyRemediationTicket;

public sealed class ModifyRemediationTicketTool(
    IClient client)
    : ITool<ModifyRemediationTicketInput, OperationOutput>
{
    public async Task<Result<OperationOutput>> ExecuteAsync(ModifyRemediationTicketInput input, CancellationToken ct)
    {

        var response = await client.ModifyTicketAsync(
            input.TicketId,
            input.Status,
            input.OpenNote,
            input.FixedNote,
            input.ClosedNote,
            input.AssignedToUserId,
            input.Comment,
            ct);

        return response.ToOutput<OperationOutput>();
    }
}
