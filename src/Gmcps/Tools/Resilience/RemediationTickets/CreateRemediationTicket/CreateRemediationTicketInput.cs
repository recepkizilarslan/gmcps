
namespace Gmcps.Domain.Resilience.RemediationTickets.Inputs;

public sealed class CreateRemediationTicketInput
{
    [Required]
    [JsonPropertyName("resultId")]
    [MaxLength(128)]
    [GvmId]
    public string ResultId { get; set; } = "";

    [Required]
    [JsonPropertyName("assignedToUserId")]
    [MaxLength(128)]
    [GvmId]
    public string AssignedToUserId { get; set; } = "";

    [Required]
    [JsonPropertyName("openNote")]
    [MaxLength(2048)]
    public string OpenNote { get; set; } = "";

    [JsonPropertyName("comment")]
    [MaxLength(2048)]
    public string? Comment { get; set; }
}
