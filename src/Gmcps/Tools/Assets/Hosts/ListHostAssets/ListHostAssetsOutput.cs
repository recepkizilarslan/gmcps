namespace Gmcps.Tools.Assets.Hosts.ListHostAssets;

public sealed record HostAssetOutput(
    string Id,
    string Name,
    string Ip,
    string OperatingSystem,
    double Severity);

public sealed record ListHostAssetsOutput(
    IReadOnlyList<HostAssetOutput> Hosts);
