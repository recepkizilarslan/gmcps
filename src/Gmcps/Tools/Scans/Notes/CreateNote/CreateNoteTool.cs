using Gmcps.Domain.Scans.Notes.Outputs;

namespace Gmcps.Tools.Scans.Notes.CreateNote;

public sealed class CreateNoteTool(
    IClient client)
    : ITool<CreateNoteInput, CreateNoteOutput>
{
    public async Task<Result<CreateNoteOutput>> ExecuteAsync(CreateNoteInput input, CancellationToken ct)
    {

        var response = await client.CreateNoteAsync(
            input.Text,
            input.NvtOid,
            input.ResultId,
            input.TaskId,
            input.Hosts,
            input.Port,
            input.Severity,
            input.ActiveDays,
            ct);

        return response.ToOutput<CreateNoteOutput>();
    }
}
