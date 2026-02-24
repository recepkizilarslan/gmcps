
namespace Gmcps.Tools.Core.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SortBy
{
    Risk,
    Name
}
