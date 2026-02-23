using Gmcps.Domain;

namespace Gmcps.Core;

public interface IClient<in TRequest, TResponse>
{
    Task<Result<TResponse>> SendAsync(TRequest request, CancellationToken ct);
}
