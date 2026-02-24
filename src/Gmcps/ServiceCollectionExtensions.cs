using Gmcps.Domain.Resilience.Compliance.Inputs;
using Gmcps.Domain.Resilience.Compliance.Outputs;
using Gmcps.Domain.Resilience.ComplianceAuditReports.Inputs;
using Gmcps.Domain.Resilience.ComplianceAuditReports.Outputs;
using Gmcps.Domain.Resilience.ComplianceAudits.Inputs;
using Gmcps.Domain.Resilience.ComplianceAudits.Outputs;
using Gmcps.Domain.Resilience.CompliancePolicies.Inputs;
using Gmcps.Domain.Resilience.CompliancePolicies.Outputs;
using Gmcps.Domain.Resilience.RemediationTickets.Inputs;
using Gmcps.Domain.Resilience.RemediationTickets.Outputs;
using Gmcps.Domain.Scans.Notes.Inputs;
using Gmcps.Domain.Scans.Notes.Outputs;
using Gmcps.Domain.Scans.Overrides.Inputs;
using Gmcps.Domain.Scans.Overrides.Outputs;
using Gmcps.Domain.Scans.Reports.Inputs;
using Gmcps.Domain.Scans.Reports.Outputs;
using Gmcps.Domain.Scans.Results.Inputs;
using Gmcps.Domain.Scans.Results.Outputs;
using Gmcps.Domain.Scans.Targets.Inputs;
using Gmcps.Domain.Scans.Targets.Outputs;
using Gmcps.Domain.Scans.Tasks.Inputs;
using Gmcps.Domain.Scans.Tasks.Outputs;
using Gmcps.Domain.Scans.Vulnerabilities.Inputs;
using Gmcps.Domain.Scans.Vulnerabilities.Outputs;
using Gmcps.Domain.SecurityInformation.CertBundAdvisories.Inputs;
using Gmcps.Domain.SecurityInformation.CertBundAdvisories.Outputs;
using Gmcps.Domain.SecurityInformation.Cpes.Inputs;
using Gmcps.Domain.SecurityInformation.Cpes.Outputs;
using Gmcps.Domain.SecurityInformation.Cves.Inputs;
using Gmcps.Domain.SecurityInformation.Cves.Outputs;
using Gmcps.Domain.SecurityInformation.DfnCertAdvisories.Inputs;
using Gmcps.Domain.SecurityInformation.DfnCertAdvisories.Outputs;
using Gmcps.Domain.SecurityInformation.Nvts.Inputs;
using Gmcps.Domain.SecurityInformation.Nvts.Outputs;
using Gmcps.Core.Abstractions;
using Gmcps.Infrastructure.Clients.Gvm;
using Gmcps.Tools.Resilience.Compliance.IsTargetCompliant;
using Gmcps.Tools.Resilience.ComplianceAuditReports.ListComplianceAuditReports;
using Gmcps.Tools.Resilience.ComplianceAudits.ListComplianceAudits;
using Gmcps.Tools.Resilience.CompliancePolicies.ListCompliancePolicies;
using Gmcps.Tools.Resilience.RemediationTickets.CreateRemediationTicket;
using Gmcps.Tools.Resilience.RemediationTickets.DeleteRemediationTicket;
using Gmcps.Tools.Resilience.RemediationTickets.ListRemediationTickets;
using Gmcps.Tools.Resilience.RemediationTickets.ModifyRemediationTicket;
using Gmcps.Tools.Scans.Notes.CreateNote;
using Gmcps.Tools.Scans.Notes.DeleteNote;
using Gmcps.Tools.Scans.Notes.ListNotes;
using Gmcps.Tools.Scans.Notes.ModifyNote;
using Gmcps.Tools.Scans.Overrides.CreateOverride;
using Gmcps.Tools.Scans.Overrides.DeleteOverride;
using Gmcps.Tools.Scans.Overrides.ListOverrides;
using Gmcps.Tools.Scans.Overrides.ModifyOverride;
using Gmcps.Tools.Scans.Reports.DeleteReport;
using Gmcps.Tools.Scans.Reports.GetReportSummary;
using Gmcps.Tools.Scans.Reports.ListReports;
using Gmcps.Tools.Scans.Results.ListResults;
using Gmcps.Tools.Scans.Targets.GetTargetsStatus;
using Gmcps.Tools.Scans.Targets.ListCriticalTargets;
using Gmcps.Tools.Scans.Tasks.CreateTask;
using Gmcps.Tools.Scans.Tasks.GetTaskStatus;
using Gmcps.Tools.Scans.Tasks.ListTasks;
using Gmcps.Tools.Scans.Tasks.ResumeTask;
using Gmcps.Tools.Scans.Tasks.StartTask;
using Gmcps.Tools.Scans.Tasks.StopTask;
using Gmcps.Tools.Scans.Vulnerabilities.ListCriticalVulnerabilities;
using Gmcps.Infrastructure.Security;
using Gmcps.Tools.SecurityInformation.CertBundAdvisories.ListCertBundAdvisories;
using Gmcps.Tools.SecurityInformation.Cpes.ListCpes;
using Gmcps.Tools.SecurityInformation.Cves.ListCves;
using Gmcps.Tools.SecurityInformation.DfnCertAdvisories.ListDfnCertAdvisories;
using Gmcps.Tools.SecurityInformation.Nvts.ListNvts;
using Gmcps.Infrastructure.Stores;
using Gmcps.Tools.Administration.Version.GetVersion;
using Gmcps.Tools.Assets.Hosts.ListHostAssets;
using Gmcps.Tools.Assets.OperatingSystems.ListOperatingSystemAssets;
using Gmcps.Tools.Assets.TlsCertificates.ListTlsCertificates;
using Gmcps.Tools.Configuration.Alerts.ListAlerts;
using Gmcps.Tools.Configuration.Credentials.ListCredentials;
using Gmcps.Tools.Configuration.Filters.ListFilters;
using Gmcps.Tools.Configuration.PortLists.ListPortLists;
using Gmcps.Tools.Configuration.ReportConfigs.ListReportConfigs;
using Gmcps.Tools.Configuration.ReportFormats.ListReportFormats;
using Gmcps.Tools.Configuration.ScanConfigs.ListScanConfigs;
using Gmcps.Tools.Configuration.Scanners.ListScanners;
using Gmcps.Tools.Configuration.Schedules.ListSchedules;
using Gmcps.Tools.Configuration.Tags.ListTags;
using Gmcps.Tools.Configuration.Targets.CreateTarget;
using Gmcps.Tools.Configuration.Targets.ListTargets;
using Gmcps.Tools.Configuration.Targets.Metadata.GetTargetMetadata;
using Gmcps.Tools.Configuration.Targets.Metadata.SetTargetMetadata;
using Gmcps.Tools.Core;
using Gmcps.Tools.Scans.Tasks.DeleteTask;
using Gmcps.Tools.Scans.Vulnerabilities.ListCriticalPackages;
using Gmcps.Toolsets;

namespace Gmcps;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGmcpsInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IClient, UnixSocketClient>();
        services.AddSingleton<ITargetMetadataStore, SqliteTargetMetadataStore>();
        services.AddSingleton<ICompliancePolicyStore, JsonCompliancePolicyStore>();
        services.AddSingleton<IRateLimiter, TokenBucketRateLimiter>();

        return services;
    }

    public static IServiceCollection AddGmcpsToolsets(this IServiceCollection services)
    {
        services.AddTransient<AdministrationToolset>();
        services.AddTransient<ScansToolset>();
        services.AddTransient<ConfigurationToolset>();
        services.AddTransient<AssetsToolset>();
        services.AddTransient<SecurityInformationToolset>();
        services.AddTransient<ResilienceToolset>();

        return services;
    }

    public static IServiceCollection AddGmcpsToolImplementations(this IServiceCollection services)
    {
        services.AddTransient<ITool<GetTaskStatusInput, GetTaskStatusOutput>, GetTaskStatusTool>();

        services.AddTransient<ITool<GetReportSummaryInput, GetReportSummaryOutput>, GetReportSummaryTool>();

        services.AddTransient<ITool<ListReportsInput, ListReportsOutput>, ListReportsTool>();

        services.AddTransient<ITool<DeleteReportInput, OperationOutput>, DeleteReportTool>();

        services.AddTransient<ITool<ListResultsInput, ListResultsOutput>, ListResultsTool>();

        services.AddTransient<ITool<ListNotesInput, ListNotesOutput>, ListNotesTool>();

        services.AddTransient<ITool<CreateNoteInput, CreateNoteOutput>, CreateNoteTool>();

        services.AddTransient<ITool<ModifyNoteInput, OperationOutput>, ModifyNoteTool>();

        services.AddTransient<ITool<DeleteNoteInput, OperationOutput>, DeleteNoteTool>();

        services.AddTransient<ITool<ListOverridesInput, ListOverridesOutput>, ListOverridesTool>();

        services.AddTransient<ITool<CreateOverrideInput, CreateOverrideOutput>, CreateOverrideTool>();

        services.AddTransient<ITool<ModifyOverrideInput, OperationOutput>, ModifyOverrideTool>();

        services.AddTransient<ITool<DeleteOverrideInput, OperationOutput>, DeleteOverrideTool>();

        services.AddTransient<ITool<CreateTaskInput, CreateTaskOutput>, CreateTaskTool>();

        services.AddTransient<ITool<StartTaskInput, StartTaskOutput>, StartTaskTool>();

        services.AddTransient<ITool<ListTasksInput, ListTasksOutput>, ListTasksTool>();

        services.AddTransient<ITool<StopTaskInput, OperationOutput>, StopTaskTool>();

        services.AddTransient<ITool<ResumeTaskInput, ResumeTaskOutput>, ResumeTaskTool>();

        services.AddTransient<ITool<DeleteTaskInput, OperationOutput>, DeleteTaskTool>();

        services.AddTransient<ITool<EmptyInput, ListScanConfigsOutput>, ListScanConfigsTool>();

        services.AddTransient<ITool<ListPortListsInput, ListPortListsOutput>, ListPortListsTool>();

        services.AddTransient<ITool<ListCredentialsInput, ListCredentialsOutput>, ListCredentialsTool>();

        services.AddTransient<ITool<ListAlertsInput, ListAlertsOutput>, ListAlertsTool>();

        services.AddTransient<ITool<ListSchedulesInput, ListSchedulesOutput>, ListSchedulesTool>();

        services.AddTransient<ITool<ListReportConfigsInput, ListReportConfigsOutput>, ListReportConfigsTool>();

        services.AddTransient<ITool<ListReportFormatsInput, ListReportFormatsOutput>, ListReportFormatsTool>();

        services.AddTransient<ITool<ListScannersInput, ListScannersOutput>, ListScannersTool>();

        services.AddTransient<ITool<ListFiltersInput, ListFiltersOutput>, ListFiltersTool>();

        services.AddTransient<ITool<ListTagsInput, ListTagsOutput>, ListTagsTool>();

        services.AddTransient<ITool<EmptyInput, ListTargetsOutput>, ListTargetsTool>();

        services.AddTransient<ITool<CreateTargetInput, CreateTargetOutput>, CreateTargetTool>();

        services.AddTransient<ITool<SetTargetMetadataInput, SetTargetMetadataOutput>, SetTargetMetadataTool>();

        services.AddTransient<ITool<GetTargetMetadataInput, GetTargetMetadataOutput>, GetTargetMetadataTool>();

        services.AddTransient<ITool<EmptyInput, GetVersionOutput>, GetVersionTool>();

        services.AddTransient<ITool<GetTargetsStatusInput, GetTargetsStatusOutput>, GetTargetsStatusTool>();

        services.AddTransient<ITool<ListCriticalTargetsInput, ListCriticalTargetsOutput>, ListCriticalTargetsTool>();

        services.AddTransient<ITool<ListCriticalVulnerabilitiesInput, ListCriticalVulnerabilitiesOutput>, ListCriticalVulnerabilitiesTool>();

        services.AddTransient<ITool<ListCriticalPackagesInput, ListCriticalPackagesOutput>, ListCriticalPackagesTool>();

        services.AddTransient<ITool<IsTargetCompliantInput, IsTargetCompliantOutput>, IsTargetCompliantTool>();

        services.AddTransient<ITool<ListRemediationTicketsInput, ListRemediationTicketsOutput>, ListRemediationTicketsTool>();

        services.AddTransient<ITool<CreateRemediationTicketInput, CreateRemediationTicketOutput>, CreateRemediationTicketTool>();

        services.AddTransient<ITool<ModifyRemediationTicketInput, OperationOutput>, ModifyRemediationTicketTool>();

        services.AddTransient<ITool<DeleteRemediationTicketInput, OperationOutput>, DeleteRemediationTicketTool>();

        services.AddTransient<ITool<ListCompliancePoliciesInput, ListCompliancePoliciesOutput>, ListCompliancePoliciesTool>();

        services.AddTransient<ITool<ListComplianceAuditsInput, ListComplianceAuditsOutput>, ListComplianceAuditsTool>();

        services.AddTransient<ITool<ListComplianceAuditReportsInput, ListComplianceAuditReportsOutput>, ListComplianceAuditReportsTool>();

        services.AddTransient<ITool<ListHostAssetsInput, ListHostAssetsOutput>, ListHostAssetsTool>();

        services.AddTransient<ITool<ListOperatingSystemAssetsInput, ListOperatingSystemAssetsOutput>, ListOperatingSystemAssetsTool>();

        services.AddTransient<ITool<ListTlsCertificatesInput, ListTlsCertificatesOutput>, ListTlsCertificatesTool>();

        services.AddTransient<ITool<ListNvtsInput, ListNvtsOutput>, ListNvtsTool>();

        services.AddTransient<ITool<ListCvesInput, ListCvesOutput>, ListCvesTool>();

        services.AddTransient<ITool<ListCpesInput, ListCpesOutput>, ListCpesTool>();

        services.AddTransient<ITool<ListCertBundAdvisoriesInput, ListCertBundAdvisoriesOutput>, ListCertBundAdvisoriesTool>();

        services.AddTransient<ITool<ListDfnCertAdvisoriesInput, ListDfnCertAdvisoriesOutput>, ListDfnCertAdvisoriesTool>();

        return services;
    }
}
