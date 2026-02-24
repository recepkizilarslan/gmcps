
namespace Gmcps.Tools.Configuration.PortLists.ListPortLists;

public sealed class ListPortListsInput
{
    [JsonPropertyName("limit")]
    [Range(1, 1000)]
    public int Limit { get; set; } = 50;
}
