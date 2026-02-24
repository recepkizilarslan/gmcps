
namespace Gmcps.Tools.Configuration.Targets.Metadata.SetTargetMetadata;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MetadataCriticalityInput
{
    Normal,
    High,
    Critical
}
