
namespace Gmcps.Toolsets;

[McpServerToolType]
public sealed class ConfigurationToolset(
    ITool<EmptyInput, ListScanConfigsOutput> listScanConfigsTool,
    ITool<ListPortListsInput, ListPortListsOutput> listPortListsTool,
    ITool<ListCredentialsInput, ListCredentialsOutput> listCredentialsTool,
    ITool<ListAlertsInput, ListAlertsOutput> listAlertsTool,
    ITool<ListSchedulesInput, ListSchedulesOutput> listSchedulesTool,
    ITool<ListReportConfigsInput, ListReportConfigsOutput> listReportConfigsTool,
    ITool<ListReportFormatsInput, ListReportFormatsOutput> listReportFormatsTool,
    ITool<ListScannersInput, ListScannersOutput> listScannersTool,
    ITool<ListFiltersInput, ListFiltersOutput> listFiltersTool,
    ITool<ListTagsInput, ListTagsOutput> listTagsTool,
    ITool<EmptyInput, ListTargetsOutput> listTargetsTool,
    ITool<CreateTargetInput, CreateTargetOutput> createTargetTool,
    ITool<SetTargetMetadataInput, SetTargetMetadataOutput> setTargetMetadataTool,
    ITool<GetTargetMetadataInput, GetTargetMetadataOutput> getTargetMetadataTool,
    ILogger<ConfigurationToolset> logger)
    : ToolsetBase
{
    [McpServerTool(Name = "gvm_list_scan_configs"), Description("List available scan configurations")]
    public Task<string> ListScanConfigs(EmptyInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_list_scan_configs", input, ct, logger, listScanConfigsTool);

    [McpServerTool(Name = "gvm_list_port_lists"), Description("List available port lists")]
    public Task<string> ListPortLists(ListPortListsInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_list_port_lists", input, ct, logger, listPortListsTool);

    [McpServerTool(Name = "gvm_list_credentials"), Description("List available credentials")]
    public Task<string> ListCredentials(ListCredentialsInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_list_credentials", input, ct, logger, listCredentialsTool);

    [McpServerTool(Name = "gvm_list_alerts"), Description("List configured alerts")]
    public Task<string> ListAlerts(ListAlertsInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_list_alerts", input, ct, logger, listAlertsTool);

    [McpServerTool(Name = "gvm_list_schedules"), Description("List configured schedules")]
    public Task<string> ListSchedules(ListSchedulesInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_list_schedules", input, ct, logger, listSchedulesTool);

    [McpServerTool(Name = "gvm_list_report_configs"), Description("List available report configs")]
    public Task<string> ListReportConfigs(ListReportConfigsInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_list_report_configs", input, ct, logger, listReportConfigsTool);

    [McpServerTool(Name = "gvm_list_report_formats"), Description("List available report formats")]
    public Task<string> ListReportFormats(ListReportFormatsInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_list_report_formats", input, ct, logger, listReportFormatsTool);

    [McpServerTool(Name = "gvm_list_scanners"), Description("List available scanners")]
    public Task<string> ListScanners(ListScannersInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_list_scanners", input, ct, logger, listScannersTool);

    [McpServerTool(Name = "gvm_list_filters"), Description("List saved filters")]
    public Task<string> ListFilters(ListFiltersInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_list_filters", input, ct, logger, listFiltersTool);

    [McpServerTool(Name = "gvm_list_tags"), Description("List tags")]
    public Task<string> ListTags(ListTagsInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_list_tags", input, ct, logger, listTagsTool);

    [McpServerTool(Name = "gvm_list_targets"), Description("List all scan targets")]
    public Task<string> ListTargets(EmptyInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_list_targets", input, ct, logger, listTargetsTool);

    [McpServerTool(Name = "gvm_create_target"), Description("Create a new scan target")]
    public Task<string> CreateTarget(CreateTargetInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_create_target", input, ct, logger, createTargetTool);

    [McpServerTool(Name = "gvm_set_target_metadata"), Description("Set target metadata (OS, criticality, tags)")]
    public Task<string> SetTargetMetadata(SetTargetMetadataInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_set_target_metadata", input, ct, logger, setTargetMetadataTool);

    [McpServerTool(Name = "gvm_get_target_metadata"), Description("Get target metadata (OS, criticality, tags)")]
    public Task<string> GetTargetMetadata(GetTargetMetadataInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_get_target_metadata", input, ct, logger, getTargetMetadataTool);
}
