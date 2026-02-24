
namespace Gmcps.Tools.Scans.Targets.ListCriticalTargets;

public sealed record ListCriticalTargetsClientData(
    IReadOnlyList<Target> Targets,
    IReadOnlyDictionary<string, TargetMetadata> MetadataByTarget,
    IReadOnlyDictionary<string, ReportSummary> ReportSummariesByTargetId);
