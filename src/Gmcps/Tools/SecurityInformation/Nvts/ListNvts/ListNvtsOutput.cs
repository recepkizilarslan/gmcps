namespace Gmcps.Domain.SecurityInformation.Nvts.Outputs;

public sealed record NvtOutput(
    string Oid,
    string Name,
    string Family,
    double Severity,
    string Summary);

public sealed record ListNvtsOutput(
    IReadOnlyList<NvtOutput> Nvts);
