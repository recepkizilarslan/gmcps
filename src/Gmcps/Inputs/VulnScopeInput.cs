using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Gmcps.Inputs;

public sealed class VulnScopeInput
{
    [JsonPropertyName("os")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OsFilterInput Os { get; set; } = OsFilterInput.Any;

    [JsonPropertyName("targetIds")]
    [MaxLength(100)]
    public List<string>? TargetIds { get; set; }
}