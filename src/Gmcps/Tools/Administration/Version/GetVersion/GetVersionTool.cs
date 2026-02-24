
namespace Gmcps.Tools.Administration.Version.GetVersion;

public sealed class GetVersionTool(
    IClient client)
    : ITool<EmptyInput, GetVersionOutput>
{
    public async Task<Result<GetVersionOutput>> ExecuteAsync(EmptyInput input, CancellationToken ct)
    {
        var response = await client.GetVersionAsync(ct);

        return response.ToOutput<GetVersionOutput>();
    }
}
