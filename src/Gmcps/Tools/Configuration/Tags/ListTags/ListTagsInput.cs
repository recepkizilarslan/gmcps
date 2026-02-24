
namespace Gmcps.Tools.Configuration.Tags.ListTags;

public sealed class ListTagsInput
{
    [JsonPropertyName("limit")]
    [Range(1, 1000)]
    public int Limit { get; set; } = 50;
}
