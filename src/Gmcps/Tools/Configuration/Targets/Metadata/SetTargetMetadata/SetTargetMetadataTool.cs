
namespace Gmcps.Tools.Configuration.Targets.Metadata.SetTargetMetadata;

public sealed class SetTargetMetadataTool(
    ITargetMetadataStore metadataStore)
    : ITool<SetTargetMetadataInput, SetTargetMetadataOutput>
{
    public async Task<Result<SetTargetMetadataOutput>> ExecuteAsync(SetTargetMetadataInput input, CancellationToken ct)
    {

        var metadata = new TargetMetadata(
            TargetId: input.TargetId,
            Os: (OsType)(int)input.Os,
            Criticality: (Criticality)(int)input.Criticality,
            Tags: input.Tags ?? [],
            CompliancePolicies: []);

        var response = await metadataStore.SetAsync(metadata, ct);

        return response.ToOutput<SetTargetMetadataOutput>();
    }
}
