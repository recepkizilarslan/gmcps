namespace Gmcps.Domain.Scans.Notes.Outputs;

public sealed record NoteOutput(
    string Id,
    string Text,
    bool Active);

public sealed record ListNotesOutput(
    IReadOnlyList<NoteOutput> Notes);
