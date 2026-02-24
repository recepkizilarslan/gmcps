
namespace Gmcps.Tools.Scans.Notes.ListNotes;

public sealed class ListNotesTool(
    IClient client)
    : ITool<ListNotesInput, ListNotesOutput>
{
    public async Task<Result<ListNotesOutput>> ExecuteAsync(ListNotesInput input, CancellationToken ct)
    {

        var response = await client.GetNotesAsync(input.Limit, ct);

        return response.ToOutput<ListNotesOutput>();
    }
}
