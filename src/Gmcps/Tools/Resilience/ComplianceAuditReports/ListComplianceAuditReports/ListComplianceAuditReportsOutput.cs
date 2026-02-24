namespace Gmcps.Domain.Resilience.ComplianceAuditReports.Outputs;

public sealed record ComplianceAuditReportSummaryOutput(
    int Yes,
    int No,
    int Incomplete);

public sealed record ComplianceAuditReportOutput(
    string ReportId,
    string TaskId,
    string Timestamp,
    ComplianceAuditReportSummaryOutput Compliance);

public sealed record ListComplianceAuditReportsOutput(
    IReadOnlyList<ComplianceAuditReportOutput> Reports);
