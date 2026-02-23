using Gmcps.Models;

namespace Gmcps.Domain.Interfaces;

public interface ITargetMetadataStore
{
    Task<Result<TargetMetadata>> GetAsync(string targetId, CancellationToken ct);
    
    Task<Result<bool>> SetAsync(TargetMetadata metadata, CancellationToken ct);
    
    Task<Result<IReadOnlyList<TargetMetadata>>> GetAllAsync(CancellationToken ct);
}
