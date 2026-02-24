namespace Gmcps.Tools.Configuration.ScanConfigs.ListScanConfigs;

public sealed record ScanConfigOutput(
    string Id,
    string Name,
    string? Comment);

public sealed record ListScanConfigsOutput(
    IReadOnlyList<ScanConfigOutput> Configs);
