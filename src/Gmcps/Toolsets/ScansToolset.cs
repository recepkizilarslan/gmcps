using System.ComponentModel;
using Gmcps.Core;
using Gmcps.Domain.Scans.Reports.Inputs;
using Gmcps.Domain.Scans.Reports.Outputs;
using Gmcps.Domain.Scans.Tasks.Inputs;
using Gmcps.Domain.Scans.Tasks.Outputs;
using ModelContextProtocol.Server;

namespace Gmcps.Toolsets;

[McpServerToolType]
public sealed class ScansToolset(
    ITool<GetTaskStatusInput, GetTaskStatusOutput> getTaskStatusTool,
    ITool<GetReportSummaryInput, GetReportSummaryOutput> getReportSummaryTool,
    ILogger<ScansToolset> logger)
    : ToolsetBase
{
    [McpServerTool(Name = "gvm_get_task_status"), Description("Get status of a scan task")]
    public Task<string> GetTaskStatus(GetTaskStatusInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_get_task_status", input, ct, logger, getTaskStatusTool);

    [McpServerTool(Name = "gvm_get_report_summary"), Description("Get summary of a scan report")]
    public Task<string> GetReportSummary(GetReportSummaryInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_get_report_summary", input, ct, logger, getReportSummaryTool);
}
