
namespace Gmcps.Toolsets;

[McpServerToolType]
public sealed class AssetsToolset(
    ITool<ListHostAssetsInput, ListHostAssetsOutput> listHostAssetsTool,
    ITool<ListOperatingSystemAssetsInput, ListOperatingSystemAssetsOutput> listOperatingSystemAssetsTool,
    ITool<ListTlsCertificatesInput, ListTlsCertificatesOutput> listTlsCertificatesTool,
    ILogger<AssetsToolset> logger)
    : ToolsetBase
{
    [McpServerTool(Name = "gvm_list_host_assets"), Description("List host assets")]
    public Task<string> ListHostAssets(ListHostAssetsInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_list_host_assets", input, ct, logger, listHostAssetsTool);

    [McpServerTool(Name = "gvm_list_operating_system_assets"), Description("List operating system assets")]
    public Task<string> ListOperatingSystemAssets(ListOperatingSystemAssetsInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_list_operating_system_assets", input, ct, logger, listOperatingSystemAssetsTool);

    [McpServerTool(Name = "gvm_list_tls_certificates"), Description("List TLS certificates")]
    public Task<string> ListTlsCertificates(ListTlsCertificatesInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_list_tls_certificates", input, ct, logger, listTlsCertificatesTool);
}
