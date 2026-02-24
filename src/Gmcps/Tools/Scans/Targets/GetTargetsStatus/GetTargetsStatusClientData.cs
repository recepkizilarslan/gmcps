
namespace Gmcps.Tools.Scans.Targets.GetTargetsStatus;

public sealed record GetTargetsStatusClientData(
    IReadOnlyList<Target> Targets,
    IReadOnlyDictionary<string, TargetMetadata> MetadataByTarget,
    IReadOnlyDictionary<string, GvmTask> LatestTaskByTarget,
    IReadOnlyDictionary<string, ReportSummary> ReportSummariesByReportId);
