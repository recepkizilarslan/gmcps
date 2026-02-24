
namespace Gmcps.Domain.Scans.Tasks.Inputs;

public sealed class ListTasksInput
{
    [JsonPropertyName("limit")]
    [Range(1, 1000)]
    public int Limit { get; set; } = 50;
}
