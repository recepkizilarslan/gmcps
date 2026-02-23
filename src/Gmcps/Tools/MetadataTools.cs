using System.ComponentModel;
using Gmcps.Inputs;
using Gmcps.Domain.Interfaces;
using Gmcps.Infrastructure.Security;
using Gmcps.Models;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace Gmcps.Tools;

[McpServerToolType]
public sealed class MetadataTools(
    ITargetMetadataStore metadataStore,
    ILogger<MetadataTools> logger)
    : ToolBase
{
    private readonly ILogger<MetadataTools> _logger = logger;

    [McpServerTool(Name = "gvm_set_target_metadata"), Description("Set target metadata (OS, criticality, tags)")]
    public async Task<string> SetTargetMetadata(SetTargetMetadataInput input, CancellationToken ct)
    {
        const string toolName = "gvm_set_target_metadata";
        _logger.LogInformation("Executing tool {ToolName} for target {TargetId}", toolName, input.TargetId);

        var validation = InputValidator.Validate(input);
        if (validation.IsFailure)
        {
            _logger.LogWarning("Validation failed for tool {ToolName}: {Error}", toolName, validation.Error);
            return ErrorJson(validation.Error);
        }

        var idValidation = InputValidator.ValidateId(input.TargetId, "targetId");
        if (idValidation.IsFailure)
        {
            _logger.LogWarning("TargetId validation failed for tool {ToolName}: {Error}", toolName, idValidation.Error);
            return ErrorJson(idValidation.Error);
        }

        var metadata = new TargetMetadata(
            TargetId: input.TargetId,
            Os: (OsType)(int)input.Os,
            Criticality: (Criticality)(int)input.Criticality,
            Tags: input.Tags ?? [],
            CompliancePolicies: []);

        var result = await metadataStore.SetAsync(metadata, ct);
        if (result.IsFailure)
        {
            _logger.LogWarning("Metadata store write failed for target {TargetId}: {Error}", input.TargetId, result.Error);
            return ErrorJson(result.Error);
        }

        _logger.LogInformation("Tool {ToolName} completed for target {TargetId}", toolName, input.TargetId);
        return ToJson(new { ok = true });
    }

    [McpServerTool(Name = "gvm_get_target_metadata"), Description("Get target metadata (OS, criticality, tags)")]
    public async Task<string> GetTargetMetadata(GetTargetMetadataInput input, CancellationToken ct)
    {
        const string toolName = "gvm_get_target_metadata";
        _logger.LogInformation("Executing tool {ToolName} for target {TargetId}", toolName, input.TargetId);

        var idValidation = InputValidator.ValidateId(input.TargetId, "targetId");
        if (idValidation.IsFailure)
        {
            _logger.LogWarning("TargetId validation failed for tool {ToolName}: {Error}", toolName, idValidation.Error);
            return ErrorJson(idValidation.Error);
        }

        var result = await metadataStore.GetAsync(input.TargetId, ct);
        if (result.IsFailure)
        {
            _logger.LogWarning("Metadata lookup failed for target {TargetId}: {Error}", input.TargetId, result.Error);
            return ErrorJson(result.Error);
        }

        _logger.LogInformation("Tool {ToolName} completed for target {TargetId}", toolName, input.TargetId);
        return ToJson(new
            {
                targetId = result.Value.TargetId,
                os = result.Value.Os.ToString(),
                criticality = result.Value.Criticality.ToString(),
                tags = result.Value.Tags
            });
    }
}
