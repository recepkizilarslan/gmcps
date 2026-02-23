using System.ComponentModel;
using Gmcps.Domain;
using Gmcps.Domain.Interfaces;
using Gmcps.Domain.Models;
using Gmcps.Inputs;
using Gmcps.Validation;
using ModelContextProtocol.Server;

namespace Gmcps.Tools;

[McpServerToolType]
public sealed class LowLevelTools(
    IRateLimiter rateLimiter,
    IGmpClient gmpClient)
    : ToolBase(rateLimiter)
{
    [McpServerTool(Name = "gvm_get_version"), Description("Get GVM protocol version")]
    public Task<string> GetVersion(ToolInput input, CancellationToken ct) =>
        ExecuteUnvalidatedAsync(
            input,
            ct,
            gmpClient.GetVersionAsync,
            static version => new VersionResponse(version));

    [McpServerTool(Name = "gvm_list_scan_configs"), Description("List available scan configurations")]
    public Task<string> ListScanConfigs(ToolInput input, CancellationToken ct) =>
        ExecuteUnvalidatedAsync(
            input,
            ct,
            gmpClient.GetScanConfigsAsync,
            static configs => new ScanConfigsResponse(configs));

    [McpServerTool(Name = "gvm_list_targets"), Description("List all scan targets")]
    public Task<string> ListTargets(ToolInput input, CancellationToken ct) =>
        ExecuteUnvalidatedAsync(
            input,
            ct,
            gmpClient.GetTargetsAsync,
            static targets => new TargetsResponse(targets));

    [McpServerTool(Name = "gvm_create_target"), Description("Create a new scan target")]
    public Task<string> CreateTarget(CreateTargetInput input, CancellationToken ct) =>
        ExecuteValidatedAsync(
            input,
            ct,
            ValidateCreateTargetInput,
            (request, token) => gmpClient.CreateTargetAsync(request.Name, request.Hosts, request.Comment, token),
            static targetId => new TargetCreatedResponse(targetId));

    [McpServerTool(Name = "gvm_create_task"), Description("Create a new scan task")]
    public Task<string> CreateTask(CreateTaskInput input, CancellationToken ct) =>
        ExecuteValidatedAsync(
            input,
            ct,
            ValidateCreateTaskInput,
            (request, token) => gmpClient.CreateTaskAsync(request.Name, request.TargetId, request.ScanConfigId, request.ScannerId, token),
            static taskId => new TaskCreatedResponse(taskId));

    [McpServerTool(Name = "gvm_start_task"), Description("Start a scan task")]
    public Task<string> StartTask(TaskIdInput input, CancellationToken ct) =>
        ExecuteValidatedAsync(
            input,
            ct,
            ValidateTaskIdInput,
            (request, token) => gmpClient.StartTaskAsync(request.TaskId, token),
            static reportId => new TaskStartedResponse(reportId));

    [McpServerTool(Name = "gvm_get_task_status"), Description("Get status of a scan task")]
    public Task<string> GetTaskStatus(TaskIdInput input, CancellationToken ct) =>
        ExecuteValidatedAsync(
            input,
            ct,
            ValidateTaskIdInput,
            (request, token) => gmpClient.GetTaskStatusAsync(request.TaskId, token),
            static task => new TaskStatusResponse(
                TaskId: task.Id,
                Name: task.Name,
                Status: task.Status,
                Progress: task.Progress,
                LastReportId: task.LastReportId));

    [McpServerTool(Name = "gvm_get_report_summary"), Description("Get summary of a scan report")]
    public Task<string> GetReportSummary(ReportIdInput input, CancellationToken ct) =>
        ExecuteValidatedAsync(
            input,
            ct,
            ValidateReportIdInput,
            (request, token) => gmpClient.GetReportSummaryAsync(request.ReportId, token),
            static report => new ReportSummaryResponse(
                ReportId: report.Id,
                TaskId: report.TaskId,
                Timestamp: report.Timestamp,
                Summary: report.Summary));

    private Task<string> ExecuteUnvalidatedAsync<TResult>(
        ToolInput input,
        CancellationToken ct,
        Func<CancellationToken, Task<Result<TResult>>> operation,
        Func<TResult, object> mapSuccess) =>
        ExecuteAsync(
            input,
            ct,
            validate: null,
            operation: (_, token) => operation(token),
            mapSuccess: mapSuccess);

    private Task<string> ExecuteValidatedAsync<TInput, TResult>(
        TInput input,
        CancellationToken ct,
        Func<TInput, Result<bool>> validate,
        Func<TInput, CancellationToken, Task<Result<TResult>>> operation,
        Func<TResult, object> mapSuccess)
        where TInput : ToolInput =>
        ExecuteAsync(
            input,
            ct,
            validate,
            operation,
            mapSuccess);

    private async Task<string> ExecuteAsync<TInput, TResult>(
        TInput input,
        CancellationToken ct,
        Func<TInput, Result<bool>>? validate,
        Func<TInput, CancellationToken, Task<Result<TResult>>> operation,
        Func<TResult, object> mapSuccess)
        where TInput : ToolInput
    {
        var rateLimit = AcquireRateLimit();
        if (rateLimit.IsFailure)
        {
            return ErrorJson(rateLimit.Error);
        }

        if (validate is not null)
        {
            var validation = validate(input);
            if (validation.IsFailure)
            {
                return ErrorJson(validation.Error);
            }
        }

        var result = await operation(input, ct);
        return result.IsSuccess ? ToJson(mapSuccess(result.Value)) : ErrorJson(result.Error);
    }

    private static Result<bool> ValidateCreateTargetInput(CreateTargetInput input) =>
        ValidateAnnotatedInput(input);

    private static Result<bool> ValidateCreateTaskInput(CreateTaskInput input)
    {
        var validation = ValidateAnnotatedInput(input);
        if (validation.IsFailure)
        {
            return validation;
        }

        var targetIdValidation = ValidateId(input.TargetId, "targetId");
        if (targetIdValidation.IsFailure)
        {
            return targetIdValidation;
        }

        var scanConfigValidation = ValidateId(input.ScanConfigId, "scanConfigId");
        if (scanConfigValidation.IsFailure)
        {
            return scanConfigValidation;
        }

        return string.IsNullOrWhiteSpace(input.ScannerId)
            ? Result<bool>.Success(true)
            : ValidateId(input.ScannerId, "scannerId");
    }

    private static Result<bool> ValidateTaskIdInput(TaskIdInput input) =>
        ValidateId(input.TaskId, "taskId");

    private static Result<bool> ValidateReportIdInput(ReportIdInput input) =>
        ValidateId(input.ReportId, "reportId");

    private static Result<bool> ValidateAnnotatedInput<T>(T input) where T : class
    {
        var validation = InputValidator.Validate(input);
        return validation.IsSuccess
            ? Result<bool>.Success(true)
            : Result<bool>.Failure(validation.Error);
    }

    private static Result<bool> ValidateId(string value, string fieldName)
    {
        var validation = InputValidator.ValidateId(value, fieldName);
        return validation.IsSuccess
            ? Result<bool>.Success(true)
            : Result<bool>.Failure(validation.Error);
    }

    private sealed record VersionResponse(string Version);
    private sealed record ScanConfigsResponse(IReadOnlyList<ScanConfig> Configs);
    private sealed record TargetsResponse(IReadOnlyList<Target> Targets);
    private sealed record TargetCreatedResponse(string TargetId);
    private sealed record TaskCreatedResponse(string TaskId);
    private sealed record TaskStartedResponse(string ReportId);
    private sealed record TaskStatusResponse(
        string TaskId,
        string Name,
        string Status,
        int Progress,
        string? LastReportId);
    private sealed record ReportSummaryResponse(
        string ReportId,
        string TaskId,
        DateTime Timestamp,
        ReportSummary Summary);
}
