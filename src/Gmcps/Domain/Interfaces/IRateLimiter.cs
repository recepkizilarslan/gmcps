namespace Gmcps.Domain.Interfaces;

public interface IRateLimiter
{
    bool TryAcquire(string key);
}
