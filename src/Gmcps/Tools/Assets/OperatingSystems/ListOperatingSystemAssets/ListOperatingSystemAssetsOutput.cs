namespace Gmcps.Tools.Assets.OperatingSystems.ListOperatingSystemAssets;

public sealed record OperatingSystemAssetOutput(
    string Id,
    string Name,
    string Title,
    int Hosts,
    int AllHosts,
    double AverageSeverity,
    double HighestSeverity);

public sealed record ListOperatingSystemAssetsOutput(
    IReadOnlyList<OperatingSystemAssetOutput> OperatingSystems);
