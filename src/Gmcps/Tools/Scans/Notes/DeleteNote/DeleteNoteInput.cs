
namespace Gmcps.Domain.Scans.Notes.Inputs;

public sealed class DeleteNoteInput
{
    [Required]
    [JsonPropertyName("noteId")]
    [MaxLength(128)]
    [GvmId]
    public string NoteId { get; set; } = "";

    [JsonPropertyName("ultimate")]
    public bool Ultimate { get; set; } = true;
}
