namespace Gmcps.Domain.Models;

public sealed record GvmComplianceAuditReport(
    string Id,
    string TaskId,
    string Timestamp,
    int Yes,
    int No,
    int Incomplete);
