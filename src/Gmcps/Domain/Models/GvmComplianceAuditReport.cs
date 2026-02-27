namespace Gmcps.Domain.Models;

public sealed record GvmComplianceAuditSummary(
    int Yes,
    int No,
    int Incomplete);

public sealed record GvmComplianceAuditReport(
    string Id,
    string TaskId,
    string Timestamp,
    int Yes,
    int No,
    int Incomplete)
{
    public GvmComplianceAuditSummary Compliance => new(
        Yes: Yes,
        No: No,
        Incomplete: Incomplete);
}
