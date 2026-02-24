
namespace Gmcps.Domain.SecurityInformation.DfnCertAdvisories.Inputs;

public sealed class ListDfnCertAdvisoriesInput
{
    [JsonPropertyName("limit")]
    [Range(1, 1000)]
    public int Limit { get; set; } = 50;
}
