using Gmcps.Domain.Scans.Notes.Inputs;

namespace Gmcps.Tools.Scans.Notes.ModifyNote;

public sealed class ModifyNoteTool(
    IClient client)
    : ITool<ModifyNoteInput, OperationOutput>
{
    public async Task<Result<OperationOutput>> ExecuteAsync(ModifyNoteInput input, CancellationToken ct)
    {

        var response = await client.ModifyNoteAsync(
            input.NoteId,
            input.Text,
            input.NvtOid,
            input.ResultId,
            input.TaskId,
            input.Hosts,
            input.Port,
            input.Severity,
            input.ActiveDays,
            ct);

        return response.ToOutput<OperationOutput>();
    }
}
