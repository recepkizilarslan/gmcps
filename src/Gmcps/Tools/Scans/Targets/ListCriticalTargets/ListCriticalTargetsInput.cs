
namespace Gmcps.Domain.Scans.Targets.Inputs;

public sealed class ListCriticalTargetsInput
{
    [JsonPropertyName("sortBy")]
    public SortBy SortBy { get; set; } = SortBy.Risk;
}
