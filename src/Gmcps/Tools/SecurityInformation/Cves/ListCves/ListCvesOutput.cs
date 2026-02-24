
namespace Gmcps.Domain.SecurityInformation.Cves.Outputs;

public sealed record ListCvesOutput(
    IReadOnlyList<SecurityInfoItemOutput> Cves);
