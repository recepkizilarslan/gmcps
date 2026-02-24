namespace Gmcps.Core.Abstractions;

public interface ITool<in TInput, TOutput>
{
    Task<Result<TOutput>> ExecuteAsync(TInput input, CancellationToken ct);
}
