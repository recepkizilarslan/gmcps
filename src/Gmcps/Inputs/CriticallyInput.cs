using System.Text.Json.Serialization;

namespace Gmcps.Inputs;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CriticalityInput
{
    Normal, 
    High, 
    Critical
}
