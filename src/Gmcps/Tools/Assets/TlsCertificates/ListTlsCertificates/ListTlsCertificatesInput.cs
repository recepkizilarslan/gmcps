
namespace Gmcps.Tools.Assets.TlsCertificates.ListTlsCertificates;

public sealed class ListTlsCertificatesInput
{
    [JsonPropertyName("limit")]
    [Range(1, 1000)]
    public int Limit { get; set; } = 50;
}
