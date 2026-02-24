
namespace Gmcps.Tools.Configuration.Targets.Metadata.SetTargetMetadata;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MetadataOsInput
{
    Unknown,
    Windows,
    Linux
}
