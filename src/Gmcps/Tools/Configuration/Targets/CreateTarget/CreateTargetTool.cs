
namespace Gmcps.Tools.Configuration.Targets.CreateTarget;

public sealed class CreateTargetTool(
    IClient client)
    : ITool<CreateTargetInput, CreateTargetOutput>
{
    public async Task<Result<CreateTargetOutput>> ExecuteAsync(CreateTargetInput input, CancellationToken ct)
    {

        var response = await client.CreateTargetAsync(input.Name, input.Hosts, input.Comment, ct);

        return response.ToOutput<CreateTargetOutput>();
    }
}
