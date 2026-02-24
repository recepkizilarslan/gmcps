
namespace Gmcps.Domain.SecurityInformation.CertBundAdvisories.Outputs;

public sealed record ListCertBundAdvisoriesOutput(
    IReadOnlyList<SecurityInfoItemOutput> Advisories);
