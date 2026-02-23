using Gmcps.Domain.Configuration;
using Gmcps.Infrastructure.Security;
using Microsoft.Extensions.Options;

namespace Gmcps.Tests;

public class SecurityTests
{
    [Fact]
    public void RateLimiter_WithinLimit_Allows()
    {
        var options = Options.Create(new ServerOptions { RateLimitPerMinute = 10 });
        var limiter = new TokenBucketRateLimiter(options);

        for (int i = 0; i < 10; i++)
        {
            Assert.True(limiter.TryAcquire("key1"));
        }
    }

    [Fact]
    public void RateLimiter_ExceedsLimit_Denies()
    {
        var options = Options.Create(new ServerOptions { RateLimitPerMinute = 5 });
        var limiter = new TokenBucketRateLimiter(options);

        // Consume all tokens
        for (int i = 0; i < 5; i++)
        {
            limiter.TryAcquire("key1");
        }

        // Next should be denied
        Assert.False(limiter.TryAcquire("key1"));
    }

    [Fact]
    public void RateLimiter_DifferentKeys_IndependentBuckets()
    {
        var options = Options.Create(new ServerOptions { RateLimitPerMinute = 2 });
        var limiter = new TokenBucketRateLimiter(options);

        Assert.True(limiter.TryAcquire("key1"));
        Assert.True(limiter.TryAcquire("key1"));
        Assert.False(limiter.TryAcquire("key1"));

        // key2 should still work
        Assert.True(limiter.TryAcquire("key2"));
    }
}
