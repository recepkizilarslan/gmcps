using System.Collections.Concurrent;

namespace Gmcps.Infrastructure.Security;

public sealed class TokenBucketRateLimiter : IRateLimiter
{
    private readonly ILogger<TokenBucketRateLimiter> _logger;

    private readonly int _maxTokens;

    private readonly double _refillRatePerSecond;

    private readonly ConcurrentDictionary<string, Bucket> _buckets = new();

    public TokenBucketRateLimiter(
        IOptions<ServerOptions> options,
        ILogger<TokenBucketRateLimiter> logger)
    {
        _logger = logger;
        _maxTokens = options.Value.RateLimitPerMinute;
        _refillRatePerSecond = _maxTokens / 60.0;
        _logger.LogInformation("Rate limiter initialized with {RateLimitPerMinute} requests per minute", _maxTokens);
    }

    public bool TryAcquire(string key)
    {
        var bucket = _buckets.GetOrAdd(key, bucketKey =>
        {
            _logger.LogDebug("Creating rate-limit bucket for key {RateLimitKey}", bucketKey);
            return new Bucket(_maxTokens);
        });

        var acquired = bucket.TryConsume(_refillRatePerSecond, _maxTokens);
        if (!acquired)
        {
            _logger.LogWarning("Rate limit exceeded for key {RateLimitKey}", key);
        }

        return acquired;
    }

    private sealed class Bucket(int maxTokens)
    {
        private double _tokens = maxTokens;

        private long _lastRefillTicks = Environment.TickCount64;

        private readonly Lock _lock = new();

        public bool TryConsume(double refillRatePerSecond, int maxTokens)
        {
            lock (_lock)
            {
                var now = Environment.TickCount64;
                var elapsed = (now - _lastRefillTicks) / 1000.0;

                _tokens = Math.Min(maxTokens, _tokens + elapsed * refillRatePerSecond);
                _lastRefillTicks = now;

                if (_tokens < 1.0)
                {
                    return false;
                }

                _tokens -= 1.0;
                return true;
            }
        }
    }
}
