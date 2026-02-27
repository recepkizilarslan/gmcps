
namespace Gmcps.Toolsets;

public abstract class ToolsetBase
{
    private const string GenericError = "Request failed.";
    private const int MaxErrorLength = 512;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    protected static async Task<string> ExecuteToolAsync<TInput, TOutput>(
        string toolName,
        TInput input,
        CancellationToken ct,
        ILogger logger,
        ITool<TInput, TOutput> tool)
        where TInput : class
    {
        try
        {
            logger.LogInformation("Executing tool {ToolName}", toolName);
            InputValidator.ValidateOrThrow(input);

            var result = await tool.ExecuteAsync(input, ct);

            if (result.IsFailure)
            {
                throw new InvalidOperationException(result.Error);
            }

            logger.LogInformation("Tool {ToolName} completed successfully", toolName);
            return ToJson(result.Value);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Tool {ToolName} canceled", toolName);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Tool {ToolName} failed", toolName);
            return ErrorJson(ToClientError(ex));
        }
    }

    private static string ToJson<T>(T value) =>
        JsonSerializer.Serialize(value, JsonOpts);

    private static string ErrorJson(string error) =>
        JsonSerializer.Serialize(new { error }, JsonOpts);

    private static string ToClientError(Exception ex)
    {
        var message = ex.Message?.ReplaceLineEndings(" ").Trim();
        if (string.IsNullOrWhiteSpace(message))
        {
            return GenericError;
        }

        return message.Length <= MaxErrorLength
            ? message
            : message[..MaxErrorLength];
    }
}
