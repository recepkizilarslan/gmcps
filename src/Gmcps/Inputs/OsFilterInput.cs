using System.Text.Json.Serialization;

namespace Gmcps.Inputs;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OsFilterInput
{
    Any, 
    Windows, 
    Linux, 
    Unknown
}