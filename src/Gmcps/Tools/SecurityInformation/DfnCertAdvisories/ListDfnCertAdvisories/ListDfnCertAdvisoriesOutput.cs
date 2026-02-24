
namespace Gmcps.Domain.SecurityInformation.DfnCertAdvisories.Outputs;

public sealed record ListDfnCertAdvisoriesOutput(
    IReadOnlyList<SecurityInfoItemOutput> Advisories);
