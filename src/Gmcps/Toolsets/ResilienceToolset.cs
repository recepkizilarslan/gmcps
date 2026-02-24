
namespace Gmcps.Toolsets;

[McpServerToolType]
public sealed class ResilienceToolset(
    ITool<ListRemediationTicketsInput, ListRemediationTicketsOutput> listRemediationTicketsTool,
    ITool<CreateRemediationTicketInput, CreateRemediationTicketOutput> createRemediationTicketTool,
    ITool<ModifyRemediationTicketInput, OperationOutput> modifyRemediationTicketTool,
    ITool<DeleteRemediationTicketInput, OperationOutput> deleteRemediationTicketTool,
    ITool<ListCompliancePoliciesInput, ListCompliancePoliciesOutput> listCompliancePoliciesTool,
    ITool<ListComplianceAuditsInput, ListComplianceAuditsOutput> listComplianceAuditsTool,
    ITool<ListComplianceAuditReportsInput, ListComplianceAuditReportsOutput> listComplianceAuditReportsTool,
    ITool<IsTargetCompliantInput, IsTargetCompliantOutput> isTargetCompliantTool,
    ILogger<ResilienceToolset> logger)
    : ToolsetBase
{
    [McpServerTool(Name = "gvm_list_remediation_tickets"), Description("List remediation tickets")]
    public Task<string> ListRemediationTickets(ListRemediationTicketsInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_list_remediation_tickets", input, ct, logger, listRemediationTicketsTool);

    [McpServerTool(Name = "gvm_create_remediation_ticket"), Description("Create a remediation ticket")]
    public Task<string> CreateRemediationTicket(CreateRemediationTicketInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_create_remediation_ticket", input, ct, logger, createRemediationTicketTool);

    [McpServerTool(Name = "gvm_modify_remediation_ticket"), Description("Modify a remediation ticket")]
    public Task<string> ModifyRemediationTicket(ModifyRemediationTicketInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_modify_remediation_ticket", input, ct, logger, modifyRemediationTicketTool);

    [McpServerTool(Name = "gvm_delete_remediation_ticket"), Description("Delete a remediation ticket")]
    public Task<string> DeleteRemediationTicket(DeleteRemediationTicketInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_delete_remediation_ticket", input, ct, logger, deleteRemediationTicketTool);

    [McpServerTool(Name = "gvm_list_compliance_policies"), Description("List compliance policies")]
    public Task<string> ListCompliancePolicies(ListCompliancePoliciesInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_list_compliance_policies", input, ct, logger, listCompliancePoliciesTool);

    [McpServerTool(Name = "gvm_list_compliance_audits"), Description("List compliance audits")]
    public Task<string> ListComplianceAudits(ListComplianceAuditsInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_list_compliance_audits", input, ct, logger, listComplianceAuditsTool);

    [McpServerTool(Name = "gvm_list_compliance_audit_reports"), Description("List compliance audit reports")]
    public Task<string> ListComplianceAuditReports(ListComplianceAuditReportsInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_list_compliance_audit_reports", input, ct, logger, listComplianceAuditReportsTool);

    [McpServerTool(Name = "gvm_is_target_compliant"), Description("Check if a target is compliant with a policy")]
    public Task<string> IsTargetCompliant(IsTargetCompliantInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_is_target_compliant", input, ct, logger, isTargetCompliantTool);
}
