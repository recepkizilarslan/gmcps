using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Gmcps.Inputs;

public sealed class ListCriticalPackagesInput : ToolInput
{
    [JsonPropertyName("os")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OsFilterInput  Os { get; set; } = OsFilterInput.Any;

    [JsonPropertyName("minSeverity")]
    [Range(0.0, 10.0)]
    public double MinSeverity { get; set; } = 7.0;

    [JsonPropertyName("limit")]
    [Range(1, 1000)]
    public int Limit { get; set; } = 50;
}