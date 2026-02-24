namespace Gmcps.Domain.Scans.Results.Outputs;

public sealed record ResultItemOutput(
    string Id,
    string Name,
    string Host,
    string Port,
    double Severity,
    string Threat,
    string NvtOid);

public sealed record ListResultsOutput(
    IReadOnlyList<ResultItemOutput> Results);
