using Gmcps.Domain.Resilience.RemediationTickets.Inputs;
using Gmcps.Domain.Resilience.RemediationTickets.Outputs;

namespace Gmcps.Tools.Resilience.RemediationTickets.CreateRemediationTicket;

public sealed class CreateRemediationTicketTool(
    IClient client)
    : ITool<CreateRemediationTicketInput, CreateRemediationTicketOutput>
{
    public async Task<Result<CreateRemediationTicketOutput>> ExecuteAsync(CreateRemediationTicketInput input, CancellationToken ct)
    {

        var response = await client.CreateTicketAsync(
            input.ResultId,
            input.AssignedToUserId,
            input.OpenNote,
            input.Comment,
            ct);

        return response.ToOutput<CreateRemediationTicketOutput>();
    }
}
