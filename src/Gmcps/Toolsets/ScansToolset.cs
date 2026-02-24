using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Gmcps.Toolsets;

[McpServerToolType]
public sealed class ScansToolset(
    ITool<CreateTaskInput, CreateTaskOutput> createTaskTool,
    ITool<StartTaskInput, StartTaskOutput> startTaskTool,
    ITool<ListTasksInput, ListTasksOutput> listTasksTool,
    ITool<StopTaskInput, OperationOutput> stopTaskTool,
    ITool<ResumeTaskInput, ResumeTaskOutput> resumeTaskTool,
    ITool<DeleteTaskInput, OperationOutput> deleteTaskTool,
    ITool<GetTaskStatusInput, GetTaskStatusOutput> getTaskStatusTool,
    ITool<GetReportSummaryInput, GetReportSummaryOutput> getReportSummaryTool,
    ITool<ListReportsInput, ListReportsOutput> listReportsTool,
    ITool<DeleteReportInput, OperationOutput> deleteReportTool,
    ITool<ListResultsInput, ListResultsOutput> listResultsTool,
    ITool<ListNotesInput, ListNotesOutput> listNotesTool,
    ITool<CreateNoteInput, CreateNoteOutput> createNoteTool,
    ITool<ModifyNoteInput, OperationOutput> modifyNoteTool,
    ITool<DeleteNoteInput, OperationOutput> deleteNoteTool,
    ITool<ListOverridesInput, ListOverridesOutput> listOverridesTool,
    ITool<CreateOverrideInput, CreateOverrideOutput> createOverrideTool,
    ITool<ModifyOverrideInput, OperationOutput> modifyOverrideTool,
    ITool<DeleteOverrideInput, OperationOutput> deleteOverrideTool,
    ITool<GetTargetsStatusInput, GetTargetsStatusOutput> getTargetsStatusTool,
    ITool<ListCriticalTargetsInput, ListCriticalTargetsOutput> listCriticalTargetsTool,
    ITool<ListCriticalVulnerabilitiesInput, ListCriticalVulnerabilitiesOutput> listCriticalVulnerabilitiesTool,
    ITool<ListCriticalPackagesInput, ListCriticalPackagesOutput> listCriticalPackagesTool,
    ILogger<ScansToolset> logger)
    : ToolsetBase
{
    [McpServerTool(Name = "gvm_create_task"), Description("Create a new scan task")]
    public Task<string> CreateTask(CreateTaskInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_create_task", input, ct, logger, createTaskTool);

    [McpServerTool(Name = "gvm_start_task"), Description("Start a scan task")]
    public Task<string> StartTask(StartTaskInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_start_task", input, ct, logger, startTaskTool);

    [McpServerTool(Name = "gvm_list_tasks"), Description("List scan tasks")]
    public Task<string> ListTasks(ListTasksInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_list_tasks", input, ct, logger, listTasksTool);

    [McpServerTool(Name = "gvm_stop_task"), Description("Stop a scan task")]
    public Task<string> StopTask(StopTaskInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_stop_task", input, ct, logger, stopTaskTool);

    [McpServerTool(Name = "gvm_resume_task"), Description("Resume a paused scan task")]
    public Task<string> ResumeTask(ResumeTaskInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_resume_task", input, ct, logger, resumeTaskTool);

    [McpServerTool(Name = "gvm_delete_task"), Description("Delete a scan task")]
    public Task<string> DeleteTask(DeleteTaskInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_delete_task", input, ct, logger, deleteTaskTool);

    [McpServerTool(Name = "gvm_get_task_status"), Description("Get status of a scan task")]
    public Task<string> GetTaskStatus(GetTaskStatusInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_get_task_status", input, ct, logger, getTaskStatusTool);

    [McpServerTool(Name = "gvm_get_report_summary"), Description("Get summary of a scan report")]
    public Task<string> GetReportSummary(GetReportSummaryInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_get_report_summary", input, ct, logger, getReportSummaryTool);

    [McpServerTool(Name = "gvm_list_reports"), Description("List scan reports")]
    public Task<string> ListReports(ListReportsInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_list_reports", input, ct, logger, listReportsTool);

    [McpServerTool(Name = "gvm_delete_report"), Description("Delete a scan report")]
    public Task<string> DeleteReport(DeleteReportInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_delete_report", input, ct, logger, deleteReportTool);

    [McpServerTool(Name = "gvm_list_results"), Description("List scan results")]
    public Task<string> ListResults(ListResultsInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_list_results", input, ct, logger, listResultsTool);

    [McpServerTool(Name = "gvm_list_notes"), Description("List scan notes")]
    public Task<string> ListNotes(ListNotesInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_list_notes", input, ct, logger, listNotesTool);

    [McpServerTool(Name = "gvm_create_note"), Description("Create a scan note")]
    public Task<string> CreateNote(CreateNoteInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_create_note", input, ct, logger, createNoteTool);

    [McpServerTool(Name = "gvm_modify_note"), Description("Modify a scan note")]
    public Task<string> ModifyNote(ModifyNoteInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_modify_note", input, ct, logger, modifyNoteTool);

    [McpServerTool(Name = "gvm_delete_note"), Description("Delete a scan note")]
    public Task<string> DeleteNote(DeleteNoteInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_delete_note", input, ct, logger, deleteNoteTool);

    [McpServerTool(Name = "gvm_list_overrides"), Description("List scan overrides")]
    public Task<string> ListOverrides(ListOverridesInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_list_overrides", input, ct, logger, listOverridesTool);

    [McpServerTool(Name = "gvm_create_override"), Description("Create a scan override")]
    public Task<string> CreateOverride(CreateOverrideInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_create_override", input, ct, logger, createOverrideTool);

    [McpServerTool(Name = "gvm_modify_override"), Description("Modify a scan override")]
    public Task<string> ModifyOverride(ModifyOverrideInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_modify_override", input, ct, logger, modifyOverrideTool);

    [McpServerTool(Name = "gvm_delete_override"), Description("Delete a scan override")]
    public Task<string> DeleteOverride(DeleteOverrideInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_delete_override", input, ct, logger, deleteOverrideTool);

    [McpServerTool(Name = "gvm_get_targets_status"), Description("Get status of targets filtered by OS (e.g., 'all Windows machines')")]
    public Task<string> GetTargetsStatus(GetTargetsStatusInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_get_targets_status", input, ct, logger, getTargetsStatusTool);

    [McpServerTool(Name = "gvm_list_critical_targets"), Description("List targets by criticality (e.g., 'which targets are critical?')")]
    public Task<string> ListCriticalTargets(ListCriticalTargetsInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_list_critical_targets", input, ct, logger, listCriticalTargetsTool);

    [McpServerTool(Name = "gvm_list_critical_vulnerabilities"), Description("List critical vulnerabilities across targets (e.g., 'give me critical vulns')")]
    public Task<string> ListCriticalVulnerabilities(ListCriticalVulnerabilitiesInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_list_critical_vulnerabilities", input, ct, logger, listCriticalVulnerabilitiesTool);

    [McpServerTool(Name = "gvm_list_critical_packages"), Description("List critical packages from scan data (best-effort extraction)")]
    public Task<string> ListCriticalPackages(ListCriticalPackagesInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_list_critical_packages", input, ct, logger, listCriticalPackagesTool);
}
