namespace Gmcps.Models;

public sealed record TargetMetadata(
    string TargetId,
    OsType Os,
    Criticality Criticality,
    IReadOnlyList<string> Tags,
    IReadOnlyList<string> CompliancePolicies);

public enum OsType
{
    Unknown,
    Windows,
    Linux
}

public enum Criticality
{
    Normal,
    High,
    Critical
}
