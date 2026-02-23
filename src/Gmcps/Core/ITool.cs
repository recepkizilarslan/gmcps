using Gmcps.Domain;

namespace Gmcps.Core;

public interface ITool<in TInput, TOutput>
{
    Task<Result<TOutput>> ExecuteAsync(TInput input, CancellationToken ct);
}
