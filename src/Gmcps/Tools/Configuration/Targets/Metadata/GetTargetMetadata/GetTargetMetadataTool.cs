using Gmcps.Infrastructure.Stores;

namespace Gmcps.Tools.Configuration.Targets.Metadata.GetTargetMetadata;

public sealed class GetTargetMetadataTool(
    ITargetMetadataStore metadataStore)
    : ITool<GetTargetMetadataInput, GetTargetMetadataOutput>
{
    public async Task<Result<GetTargetMetadataOutput>> ExecuteAsync(GetTargetMetadataInput input, CancellationToken ct)
    {

        var response = await metadataStore.GetAsync(input.TargetId, ct);

        return response.ToOutput<GetTargetMetadataOutput>();
    }
}
