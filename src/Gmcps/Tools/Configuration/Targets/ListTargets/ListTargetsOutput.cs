namespace Gmcps.Tools.Configuration.Targets.ListTargets;

public sealed record TargetOutput(
    string Id,
    string Name,
    IReadOnlyList<string> Tags,
    int HostsCount,
    string OsHint);

public sealed record ListTargetsOutput(
    IReadOnlyList<TargetOutput> Targets);
