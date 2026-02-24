
namespace Gmcps.Domain.SecurityInformation.CertBundAdvisories.Inputs;

public sealed class ListCertBundAdvisoriesInput
{
    [JsonPropertyName("limit")]
    [Range(1, 1000)]
    public int Limit { get; set; } = 50;
}
