using System.Collections.Concurrent;
using Gmcps.Domain.Configuration;
using Gmcps.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace Gmcps.Infrastructure.Security;

public sealed class TokenBucketRateLimiter : IRateLimiter
{
    private readonly int _maxTokens;
    private readonly double _refillRatePerSecond;
    private readonly ConcurrentDictionary<string, Bucket> _buckets = new();

    public TokenBucketRateLimiter(IOptions<ServerOptions> options)
    {
        _maxTokens = options.Value.RateLimitPerMinute;
        _refillRatePerSecond = _maxTokens / 60.0;
    }

    public bool TryAcquire(string key)
    {
        var bucket = _buckets.GetOrAdd(key, _ => new Bucket(_maxTokens));
        return bucket.TryConsume(_refillRatePerSecond, _maxTokens);
    }

    private sealed class Bucket
    {
        private double _tokens;
        private long _lastRefillTicks;
        private readonly object _lock = new();

        public Bucket(int maxTokens)
        {
            _tokens = maxTokens;
            _lastRefillTicks = Environment.TickCount64;
        }

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
