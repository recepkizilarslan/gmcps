namespace Gmcps.Tools.Configuration.PortLists.ListPortLists;

public sealed record PortListOutput(
    string Id,
    string Name,
    string? Comment);

public sealed record ListPortListsOutput(
    IReadOnlyList<PortListOutput> PortLists);
