
namespace Gmcps.Domain.Scans.Notes.Inputs;

public sealed class ListNotesInput
{
    [JsonPropertyName("limit")]
    [Range(1, 1000)]
    public int Limit { get; set; } = 50;
}
