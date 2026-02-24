
namespace Gmcps.Tools.Configuration.Credentials.ListCredentials;

public sealed class ListCredentialsInput
{
    [JsonPropertyName("limit")]
    [Range(1, 1000)]
    public int Limit { get; set; } = 50;
}
