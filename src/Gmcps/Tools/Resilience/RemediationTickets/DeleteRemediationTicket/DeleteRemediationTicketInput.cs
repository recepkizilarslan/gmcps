
namespace Gmcps.Domain.Resilience.RemediationTickets.Inputs;

public sealed class DeleteRemediationTicketInput
{
    [Required]
    [JsonPropertyName("ticketId")]
    [MaxLength(128)]
    [GvmId]
    public string TicketId { get; set; } = "";

    [JsonPropertyName("ultimate")]
    public bool Ultimate { get; set; } = true;
}
