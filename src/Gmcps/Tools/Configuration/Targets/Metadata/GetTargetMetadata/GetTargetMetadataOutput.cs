namespace Gmcps.Tools.Configuration.Targets.Metadata.GetTargetMetadata;

public sealed record GetTargetMetadataOutput(
    string TargetId,
    string Os,
    string Criticality,
    IReadOnlyList<string> Tags);
