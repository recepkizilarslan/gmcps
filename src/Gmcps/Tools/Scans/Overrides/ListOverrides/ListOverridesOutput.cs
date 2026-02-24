namespace Gmcps.Domain.Scans.Overrides.Outputs;

public sealed record OverrideOutput(
    string Id,
    string Text,
    double? NewSeverity,
    bool Active);

public sealed record ListOverridesOutput(
    IReadOnlyList<OverrideOutput> Overrides);
