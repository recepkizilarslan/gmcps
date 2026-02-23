using System.ComponentModel;
using Gmcps.Inputs;
using Gmcps.Validation;
using Gmcps.Domain.Interfaces;
using Gmcps.Domain.Models;
using ModelContextProtocol.Server;

namespace Gmcps.Tools;

[McpServerToolType]
public sealed class MetadataTools(
    IRateLimiter rateLimiter,
    ITargetMetadataStore metadataStore)
    : ToolBase(rateLimiter)
{
    [McpServerTool(Name = "gvm_set_target_metadata"), Description("Set target metadata (OS, criticality, tags)")]
    public async Task<string> SetTargetMetadata(SetTargetMetadataInput input, CancellationToken ct)
    {
        var rateLimit = AcquireRateLimit();
        if (rateLimit.IsFailure)
        {
            return ErrorJson(rateLimit.Error);
        }

        var validation = InputValidator.Validate(input);
        if (validation.IsFailure)
        {
            return ErrorJson(validation.Error);
        }

        var idValidation = InputValidator.ValidateId(input.TargetId, "targetId");
        if (idValidation.IsFailure)
        {
            return ErrorJson(idValidation.Error);
        }

        var metadata = new TargetMetadata(
            TargetId: input.TargetId,
            Os: (OsType)(int)input.Os,
            Criticality: (Criticality)(int)input.Criticality,
            Tags: input.Tags ?? [],
            CompliancePolicies: []);

        var result = await metadataStore.SetAsync(metadata, ct);
        return result.IsSuccess ? ToJson(new { ok = true }) : ErrorJson(result.Error);
    }

    [McpServerTool(Name = "gvm_get_target_metadata"), Description("Get target metadata (OS, criticality, tags)")]
    public async Task<string> GetTargetMetadata(GetTargetMetadataInput input, CancellationToken ct)
    {
        var rateLimit = AcquireRateLimit();
        if (rateLimit.IsFailure)
        {
            return ErrorJson(rateLimit.Error);
        }

        var idValidation = InputValidator.ValidateId(input.TargetId, "targetId");
        if (idValidation.IsFailure)
        {
            return ErrorJson(idValidation.Error);
        }

        var result = await metadataStore.GetAsync(input.TargetId, ct);
        return result.IsSuccess
            ? ToJson(new
            {
                targetId = result.Value.TargetId,
                os = result.Value.Os.ToString(),
                criticality = result.Value.Criticality.ToString(),
                tags = result.Value.Tags
            })
            : ErrorJson(result.Error);
    }
}
