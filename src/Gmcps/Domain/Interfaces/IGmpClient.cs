using Gmcps.Domain.Models;

namespace Gmcps.Domain.Interfaces;

public interface IGmpClient
{
    Task<Result<string>> GetVersionAsync(CancellationToken ct);
    
    Task<Result<IReadOnlyList<ScanConfig>>> GetScanConfigsAsync(CancellationToken ct);
    
    Task<Result<IReadOnlyList<Target>>> GetTargetsAsync(CancellationToken ct);
    
    Task<Result<string>> CreateTargetAsync(string name, string hosts, string? comment, CancellationToken ct);
    
    Task<Result<string>> CreateTaskAsync(string name, string targetId, string scanConfigId, string scannerId, CancellationToken ct);
    
    Task<Result<string>> StartTaskAsync(string taskId, CancellationToken ct);
    
    Task<Result<GvmTask>> GetTaskStatusAsync(string taskId, CancellationToken ct);
    
    Task<Result<IReadOnlyList<GvmTask>>> GetTasksAsync(CancellationToken ct);
    
    Task<Result<Report>> GetReportSummaryAsync(string reportId, CancellationToken ct);
    
    Task<Result<IReadOnlyList<Finding>>> GetReportFindingsAsync(string reportId, CancellationToken ct);
}
