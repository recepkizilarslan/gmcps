
namespace Gmcps.Tools.Configuration.Targets.CreateTarget;

public sealed class CreateTargetTool(
    IClient client,
    IOptions<GvmOptions> options)
    : ITool<CreateTargetInput, CreateTargetOutput>
{
    private readonly string? _defaultPortListId =
        string.IsNullOrWhiteSpace(options.Value.DefaultPortListId) ? null : options.Value.DefaultPortListId.Trim();

    public async Task<Result<CreateTargetOutput>> ExecuteAsync(CreateTargetInput input, CancellationToken ct)
    {
        var portListId = string.IsNullOrWhiteSpace(input.PortListId) ? _defaultPortListId : input.PortListId;

        var response = await client.CreateTargetAsync(input.Name, input.Hosts, input.Comment, portListId, ct);

        return response.ToOutput<CreateTargetOutput>();
    }
}
