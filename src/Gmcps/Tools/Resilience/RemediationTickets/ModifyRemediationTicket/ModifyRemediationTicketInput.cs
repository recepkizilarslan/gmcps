
namespace Gmcps.Domain.Resilience.RemediationTickets.Inputs;

public sealed class ModifyRemediationTicketInput
{
    [Required]
    [JsonPropertyName("ticketId")]
    [MaxLength(128)]
    [GvmId]
    public string TicketId { get; set; } = "";

    [JsonPropertyName("status")]
    [MaxLength(32)]
    public string? Status { get; set; }

    [JsonPropertyName("openNote")]
    [MaxLength(2048)]
    public string? OpenNote { get; set; }

    [JsonPropertyName("fixedNote")]
    [MaxLength(2048)]
    public string? FixedNote { get; set; }

    [JsonPropertyName("closedNote")]
    [MaxLength(2048)]
    public string? ClosedNote { get; set; }

    [JsonPropertyName("assignedToUserId")]
    [MaxLength(128)]
    [GvmId]
    public string? AssignedToUserId { get; set; }

    [JsonPropertyName("comment")]
    [MaxLength(2048)]
    public string? Comment { get; set; }
}
