namespace Gmcps.Core.Abstractions;

public interface IClient
{
    Task<Result<XDocument>> SendCommandAsync(string command, CancellationToken ct);

    Task<Result<string>> GetVersionAsync(CancellationToken ct);

    Task<Result<IReadOnlyList<ScanConfig>>> GetScanConfigsAsync(CancellationToken ct);

    Task<Result<IReadOnlyList<Target>>> GetTargetsAsync(CancellationToken ct);

    Task<Result<string>> CreateTargetAsync(string name, string hosts, string? comment, string? portListId, CancellationToken ct);

    Task<Result<bool>> DeleteTargetAsync(string targetId, bool ultimate, CancellationToken ct);

    Task<Result<IReadOnlyList<GvmTask>>> GetTasksAsync(CancellationToken ct);

    Task<Result<IReadOnlyList<GvmTask>>> GetTasksAsync(int limit, string usageType, CancellationToken ct);

    Task<Result<GvmTask>> GetTaskStatusAsync(string taskId, CancellationToken ct);

    Task<Result<string>> CreateTaskAsync(string name, string targetId, string scanConfigId, string? scannerId, CancellationToken ct);

    Task<Result<string>> StartTaskAsync(string taskId, CancellationToken ct);

    Task<Result<bool>> StopTaskAsync(string taskId, CancellationToken ct);

    Task<Result<string>> ResumeTaskAsync(string taskId, CancellationToken ct);

    Task<Result<bool>> DeleteTaskAsync(string taskId, bool ultimate, CancellationToken ct);

    Task<Result<Report>> GetReportSummaryAsync(string reportId, CancellationToken ct);

    Task<Result<IReadOnlyList<Report>>> GetReportsAsync(int limit, string usageType, CancellationToken ct);

    Task<Result<IReadOnlyList<Finding>>> GetReportFindingsAsync(string reportId, CancellationToken ct);

    Task<Result<bool>> DeleteReportAsync(string reportId, CancellationToken ct);

    Task<Result<IReadOnlyList<ScanResultItem>>> GetResultsAsync(int limit, CancellationToken ct);

    Task<Result<IReadOnlyList<NoteItem>>> GetNotesAsync(int limit, CancellationToken ct);

    Task<Result<string>> CreateNoteAsync(
        string text,
        string nvtOid,
        string? resultId,
        string? taskId,
        string? hosts,
        string? port,
        double? severity,
        int? activeDays,
        CancellationToken ct);

    Task<Result<bool>> ModifyNoteAsync(
        string noteId,
        string text,
        string? nvtOid,
        string? resultId,
        string? taskId,
        string? hosts,
        string? port,
        double? severity,
        int? activeDays,
        CancellationToken ct);

    Task<Result<bool>> DeleteNoteAsync(string noteId, bool ultimate, CancellationToken ct);


    Task<Result<IReadOnlyList<OverrideItem>>> GetOverridesAsync(int limit, CancellationToken ct);

    Task<Result<string>> CreateOverrideAsync(
        string text,
        string nvtOid,
        double? newSeverity,
        string? resultId,
        string? taskId,
        string? hosts,
        string? port,
        double? severity,
        int? activeDays,
        CancellationToken ct);

    Task<Result<bool>> ModifyOverrideAsync(
        string overrideId,
        string text,
        string? nvtOid,
        double? newSeverity,
        string? resultId,
        string? taskId,
        string? hosts,
        string? port,
        double? severity,
        int? activeDays,
        CancellationToken ct);
    Task<Result<bool>> DeleteOverrideAsync(string overrideId, bool ultimate, CancellationToken ct);

    Task<Result<IReadOnlyList<HostAsset>>> GetHostAssetsAsync(int limit, CancellationToken ct);

    Task<Result<IReadOnlyList<OperatingSystemAsset>>> GetOperatingSystemAssetsAsync(int limit, CancellationToken ct);

    Task<Result<IReadOnlyList<TlsCertificateAsset>>> GetTlsCertificatesAsync(int limit, CancellationToken ct);

    Task<Result<IReadOnlyList<SecurityInfoEntry>>> GetSecurityInfosAsync(string type, int limit, CancellationToken ct);

    Task<Result<IReadOnlyList<NvtEntry>>> GetNvtsAsync(int limit, CancellationToken ct);

    Task<Result<IReadOnlyList<GvmPortList>>> GetPortListsAsync(int limit, CancellationToken ct);

    Task<Result<IReadOnlyList<GvmCredential>>> GetCredentialsAsync(int limit, CancellationToken ct);

    Task<Result<IReadOnlyList<GvmAlert>>> GetAlertsAsync(int limit, CancellationToken ct);

    Task<Result<IReadOnlyList<GvmSchedule>>> GetSchedulesAsync(int limit, CancellationToken ct);

    Task<Result<IReadOnlyList<GvmReportConfig>>> GetReportConfigsAsync(int limit, CancellationToken ct);

    Task<Result<IReadOnlyList<GvmReportFormat>>> GetReportFormatsAsync(int limit, CancellationToken ct);

    Task<Result<IReadOnlyList<GvmScanner>>> GetScannersAsync(int limit, CancellationToken ct);

    Task<Result<IReadOnlyList<GvmFilter>>> GetFiltersAsync(int limit, CancellationToken ct);

    Task<Result<IReadOnlyList<GvmTag>>> GetTagsAsync(int limit, CancellationToken ct);

    Task<Result<IReadOnlyList<GvmUser>>> GetUsersAsync(int limit, CancellationToken ct);

    Task<Result<IReadOnlyList<GvmTicket>>> GetTicketsAsync(int limit, CancellationToken ct);

    Task<Result<string>> CreateTicketAsync(string resultId, string assignedToUserId, string openNote, string? comment, CancellationToken ct);

    Task<Result<bool>> ModifyTicketAsync(
        string ticketId,
        string? status,
        string? openNote,
        string? fixedNote,
        string? closedNote,
        string? assignedToUserId,
        string? comment,
        CancellationToken ct);

    Task<Result<bool>> DeleteTicketAsync(string ticketId, bool ultimate, CancellationToken ct);

    Task<Result<IReadOnlyList<GvmComplianceAuditReport>>> GetComplianceAuditReportsAsync(int limit, CancellationToken ct);
}
