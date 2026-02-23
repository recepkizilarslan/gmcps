using System.Text.Json;
using Gmcps.Domain;
using Gmcps.Domain.Interfaces;

namespace Gmcps.Tools;

public abstract class ToolBase
{
    private readonly IRateLimiter _rateLimiter;

    protected static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    protected ToolBase(IRateLimiter rateLimiter)
    {
        _rateLimiter = rateLimiter;
    }

    protected Result<bool> AcquireRateLimit()
    {
        if (!_rateLimiter.TryAcquire("anonymous"))
        {
            return Result<bool>.Failure("Rate limit exceeded. Please wait and try again.");
        }

        return Result<bool>.Success(true);
    }

    protected static string ToJson<T>(T value) =>
        JsonSerializer.Serialize(value, JsonOpts);

    protected static string ErrorJson(string error) =>
        JsonSerializer.Serialize(new { error }, JsonOpts);
}
