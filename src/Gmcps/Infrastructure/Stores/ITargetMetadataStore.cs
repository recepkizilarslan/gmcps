
namespace Gmcps.Infrastructure.Stores;

public interface ITargetMetadataStore
{
    Task<Result<TargetMetadata>> GetAsync(string targetId, CancellationToken ct);

    Task<Result<bool>> SetAsync(TargetMetadata metadata, CancellationToken ct);

    Task<Result<IReadOnlyList<TargetMetadata>>> GetAllAsync(CancellationToken ct);
}
