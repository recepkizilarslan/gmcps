namespace Gmcps.Domain.Models;

public sealed record NoteItem(
    string Id,
    string Text,
    bool Active);
