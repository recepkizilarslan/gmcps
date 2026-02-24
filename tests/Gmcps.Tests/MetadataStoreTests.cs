
namespace Gmcps.Tests;

public class MetadataStoreTests : IDisposable
{
    private readonly string _dbPath;
    private readonly SqliteTargetMetadataStore _store;

    public MetadataStoreTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"test-metadata-{Guid.NewGuid()}.db");
        var options = Options.Create(new StoreOptions { MetadataPath = _dbPath });
        _store = new SqliteTargetMetadataStore(options, NullLogger<SqliteTargetMetadataStore>.Instance);
    }

    [Fact]
    public async Task SetAndGet_RoundTrips()
    {
        var metadata = new TargetMetadata(
            TargetId: "target-123",
            Os: OsType.Windows,
            Criticality: Criticality.Critical,
            Tags: ["production", "web"],
            CompliancePolicies: ["policy-1"]);

        var setResult = await _store.SetAsync(metadata, CancellationToken.None);
        Assert.True(setResult.IsSuccess);

        var getResult = await _store.GetAsync("target-123", CancellationToken.None);
        Assert.True(getResult.IsSuccess);
        Assert.Equal("target-123", getResult.Value.TargetId);
        Assert.Equal(OsType.Windows, getResult.Value.Os);
        Assert.Equal(Criticality.Critical, getResult.Value.Criticality);
        Assert.Contains("production", getResult.Value.Tags);
        Assert.Contains("web", getResult.Value.Tags);
    }

    [Fact]
    public async Task Get_NonExistent_ReturnsFailure()
    {
        var result = await _store.GetAsync("nonexistent", CancellationToken.None);
        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task Set_Upserts_ExistingRecord()
    {
        var original = new TargetMetadata("t1", OsType.Windows, Criticality.Normal, [], []);
        await _store.SetAsync(original, CancellationToken.None);

        var updated = new TargetMetadata("t1", OsType.Linux, Criticality.Critical, ["updated"], []);
        await _store.SetAsync(updated, CancellationToken.None);

        var result = await _store.GetAsync("t1", CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Equal(OsType.Linux, result.Value.Os);
        Assert.Equal(Criticality.Critical, result.Value.Criticality);
    }

    [Fact]
    public async Task GetAll_ReturnsAllRecords()
    {
        await _store.SetAsync(new TargetMetadata("t1", OsType.Windows, Criticality.High, [], []), CancellationToken.None);
        await _store.SetAsync(new TargetMetadata("t2", OsType.Linux, Criticality.Normal, [], []), CancellationToken.None);

        var result = await _store.GetAllAsync(CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
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
