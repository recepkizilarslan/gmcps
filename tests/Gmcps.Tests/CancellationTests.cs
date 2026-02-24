
namespace Gmcps.Tests;

public class CancellationTests : IDisposable
{
    private readonly string _dbPath;
    private readonly SqliteTargetMetadataStore _store;

    public CancellationTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"test-cancel-{Guid.NewGuid()}.db");
        var options = Options.Create(new StoreOptions { MetadataPath = _dbPath });
        _store = new SqliteTargetMetadataStore(options, NullLogger<SqliteTargetMetadataStore>.Instance);
    }

    [Fact]
    public async Task MetadataStore_CancelledToken_ThrowsOperationCancelled()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _store.SetAsync(
                new TargetMetadata("t1", OsType.Windows, Criticality.Normal, [], []),
                cts.Token));
    }

    [Fact]
    public async Task PolicyStore_CancelledToken_ThrowsOperationCancelled()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var options = Options.Create(new StoreOptions { PoliciesPath = "nonexistent.json" });
        var store = new JsonCompliancePolicyStore(options, NullLogger<JsonCompliancePolicyStore>.Instance);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            store.GetPolicyAsync("p1", cts.Token));
    }

    public void Dispose()
    {
        _store.Dispose();
        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }
}
