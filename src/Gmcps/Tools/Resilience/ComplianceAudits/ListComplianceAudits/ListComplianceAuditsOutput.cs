namespace Gmcps.Domain.Resilience.ComplianceAudits.Outputs;

public sealed record ComplianceAuditTaskOutput(
    string TaskId,
    string Name,
    string Status,
    int Progress,
    string? LastReportId);

public sealed record ListComplianceAuditsOutput(
    IReadOnlyList<ComplianceAuditTaskOutput> Audits);
