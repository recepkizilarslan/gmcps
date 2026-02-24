namespace Gmcps.Tools.Configuration.Scanners.ListScanners;

public sealed record ScannerOutput(
    string Id,
    string Name,
    string Type,
    bool Active);

public sealed record ListScannersOutput(
    IReadOnlyList<ScannerOutput> Scanners);
