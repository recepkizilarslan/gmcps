namespace Gmcps.Infrastructure.Security.Core;

public interface IRateLimiter
{
    bool TryAcquire(string key);
}
