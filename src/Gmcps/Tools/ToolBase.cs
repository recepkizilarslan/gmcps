using System.Text.Json;

namespace Gmcps.Tools;

public abstract class ToolBase
{
    protected static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    protected static string ToJson<T>(T value) =>
        JsonSerializer.Serialize(value, JsonOpts);

    protected static string ErrorJson(string error) =>
        JsonSerializer.Serialize(new { error }, JsonOpts);
}
