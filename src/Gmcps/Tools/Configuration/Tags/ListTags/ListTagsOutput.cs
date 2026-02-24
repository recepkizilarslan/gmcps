namespace Gmcps.Tools.Configuration.Tags.ListTags;

public sealed record TagOutput(
    string Id,
    string Name,
    string? Value,
    string? Comment);

public sealed record ListTagsOutput(
    IReadOnlyList<TagOutput> Tags);
