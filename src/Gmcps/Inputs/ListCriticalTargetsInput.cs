using System.Text.Json.Serialization;

namespace Gmcps.Inputs;

public sealed class ListCriticalTargetsInput : ToolInput
{
    [JsonPropertyName("sortBy")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SortByInput SortBy { get; set; } = SortByInput.Risk;
}