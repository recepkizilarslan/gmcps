
namespace Gmcps.Toolsets;

[McpServerToolType]
public sealed class SecurityInformationToolset(
    ITool<ListNvtsInput, ListNvtsOutput> listNvtsTool,
    ITool<ListCvesInput, ListCvesOutput> listCvesTool,
    ITool<ListCpesInput, ListCpesOutput> listCpesTool,
    ITool<ListCertBundAdvisoriesInput, ListCertBundAdvisoriesOutput> listCertBundAdvisoriesTool,
    ITool<ListDfnCertAdvisoriesInput, ListDfnCertAdvisoriesOutput> listDfnCertAdvisoriesTool,
    ILogger<SecurityInformationToolset> logger)
    : ToolsetBase
{
    [McpServerTool(Name = "gvm_list_nvts"), Description("List NVT definitions")]
    public Task<string> ListNvts(ListNvtsInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_list_nvts", input, ct, logger, listNvtsTool);

    [McpServerTool(Name = "gvm_list_cves"), Description("List CVE entries")]
    public Task<string> ListCves(ListCvesInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_list_cves", input, ct, logger, listCvesTool);

    [McpServerTool(Name = "gvm_list_cpes"), Description("List CPE entries")]
    public Task<string> ListCpes(ListCpesInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_list_cpes", input, ct, logger, listCpesTool);

    [McpServerTool(Name = "gvm_list_cert_bund_advisories"), Description("List CERT-Bund advisories")]
    public Task<string> ListCertBundAdvisories(ListCertBundAdvisoriesInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_list_cert_bund_advisories", input, ct, logger, listCertBundAdvisoriesTool);

    [McpServerTool(Name = "gvm_list_dfn_cert_advisories"), Description("List DFN-CERT advisories")]
    public Task<string> ListDfnCertAdvisories(ListDfnCertAdvisoriesInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_list_dfn_cert_advisories", input, ct, logger, listDfnCertAdvisoriesTool);
}
