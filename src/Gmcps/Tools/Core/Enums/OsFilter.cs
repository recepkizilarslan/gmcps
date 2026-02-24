
namespace Gmcps.Tools.Core.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OsFilter
{
    Any,
    Windows,
    Linux,
    Unknown
}
