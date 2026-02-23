using System.Text.Json;
using Gmcps.Core;

namespace Gmcps.Toolsets;

public abstract class ToolsetBase
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    protected static string ToJson<T>(T value) =>
        JsonSerializer.Serialize(value, JsonOpts);

    protected static string ErrorJson(string error) =>
        JsonSerializer.Serialize(new { error }, JsonOpts);

    protected static async Task<string> ExecuteToolAsync<TInput, TOutput>(
        string toolName,
        TInput input,
        CancellationToken ct,
        ILogger logger,
        ITool<TInput, TOutput> tool)
    {
        logger.LogInformation("Executing tool {ToolName}", toolName);

        var result = await tool.ExecuteAsync(input, ct);
        if (result.IsFailure)
        {
            logger.LogWarning("Tool {ToolName} failed: {Error}", toolName, result.Error);
            return ErrorJson(result.Error);
        }

        logger.LogInformation("Tool {ToolName} completed successfully", toolName);
        return ToJson(result.Value);
    }
}
