namespace Gmcps.Infrastructure.Security;

public interface IRateLimiter
{
    bool TryAcquire(string key);
}
