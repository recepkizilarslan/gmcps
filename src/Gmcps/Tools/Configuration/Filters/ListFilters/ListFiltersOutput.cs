namespace Gmcps.Tools.Configuration.Filters.ListFilters;

public sealed record FilterOutput(
    string Id,
    string Name,
    string? Term,
    string? Type);

public sealed record ListFiltersOutput(
    IReadOnlyList<FilterOutput> Filters);
