
namespace Gmcps.Toolsets;

[McpServerToolType]
public sealed class AdministrationToolset(
    ITool<EmptyInput, GetVersionOutput> getVersionTool,
    ILogger<AdministrationToolset> logger)
    : ToolsetBase
{
    [McpServerTool(Name = "gvm_get_version"), Description("Get GVM protocol version")]
    public Task<string> GetVersion(EmptyInput input, CancellationToken ct) =>
        ExecuteToolAsync("gvm_get_version", input, ct, logger, getVersionTool);
}
