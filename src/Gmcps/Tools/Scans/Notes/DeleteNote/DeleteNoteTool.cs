
namespace Gmcps.Tools.Scans.Notes.DeleteNote;

public sealed class DeleteNoteTool(
    IClient client)
    : ITool<DeleteNoteInput, OperationOutput>
{
    public async Task<Result<OperationOutput>> ExecuteAsync(DeleteNoteInput input, CancellationToken ct)
    {

        var response = await client.DeleteNoteAsync(input.NoteId, input.Ultimate, ct);

        return response.ToOutput<OperationOutput>();
    }
}
