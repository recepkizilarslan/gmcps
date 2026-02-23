using Gmcps.Domain;
using Gmcps.Domain.Configuration;
using Gmcps.Domain.Interfaces;
using Gmcps.Domain.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gmcps.Infrastructure.Stores;

public sealed class SqliteTargetMetadataStore : ITargetMetadataStore, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ILogger<SqliteTargetMetadataStore> _logger;
    private bool _initialized;

    public SqliteTargetMetadataStore(IOptions<StoreOptions> options, ILogger<SqliteTargetMetadataStore> logger)
    {
        _logger = logger;
        var path = options.Value.MetadataPath;

        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        _connection = new SqliteConnection($"Data Source={path}");
        _connection.Open();
    }

    private async Task EnsureInitializedAsync(CancellationToken ct)
    {
        if (_initialized)
        {
            return;
        }

        await using var cmd = _connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS target_metadata (
                target_id TEXT PRIMARY KEY,
                os TEXT NOT NULL DEFAULT 'Unknown',
                criticality TEXT NOT NULL DEFAULT 'Normal',
                tags TEXT NOT NULL DEFAULT '',
                compliance_policies TEXT NOT NULL DEFAULT ''
            );
            """;
        await cmd.ExecuteNonQueryAsync(ct);
        _initialized = true;
    }

    public async Task<Result<TargetMetadata>> GetAsync(string targetId, CancellationToken ct)
    {
        await EnsureInitializedAsync(ct);

        await using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT target_id, os, criticality, tags, compliance_policies FROM target_metadata WHERE target_id = @id";
        cmd.Parameters.AddWithValue("@id", targetId);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            return Result<TargetMetadata>.Failure($"Metadata not found for target {targetId}");
        }

        return Result<TargetMetadata>.Success(ReadMetadata(reader));
    }

    public async Task<Result<bool>> SetAsync(TargetMetadata metadata, CancellationToken ct)
    {
        await EnsureInitializedAsync(ct);

        await using var cmd = _connection.CreateCommand();
        cmd.CommandText = """
            INSERT OR REPLACE INTO target_metadata (target_id, os, criticality, tags, compliance_policies)
            VALUES (@id, @os, @criticality, @tags, @policies)
            """;
        cmd.Parameters.AddWithValue("@id", metadata.TargetId);
        cmd.Parameters.AddWithValue("@os", metadata.Os.ToString());
        cmd.Parameters.AddWithValue("@criticality", metadata.Criticality.ToString());
        cmd.Parameters.AddWithValue("@tags", string.Join(",", metadata.Tags));
        cmd.Parameters.AddWithValue("@policies", string.Join(",", metadata.CompliancePolicies));

        await cmd.ExecuteNonQueryAsync(ct);
        _logger.LogInformation("Stored metadata for target {TargetId}", metadata.TargetId);
        return Result<bool>.Success(true);
    }

    public async Task<Result<IReadOnlyList<TargetMetadata>>> GetAllAsync(CancellationToken ct)
    {
        await EnsureInitializedAsync(ct);

        await using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT target_id, os, criticality, tags, compliance_policies FROM target_metadata";

        var results = new List<TargetMetadata>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            results.Add(ReadMetadata(reader));
        }

        return Result<IReadOnlyList<TargetMetadata>>.Success(results);
    }

    private static TargetMetadata ReadMetadata(SqliteDataReader reader)
    {
        var tagsStr = reader.GetString(3);
        var policiesStr = reader.GetString(4);

        return new TargetMetadata(
            TargetId: reader.GetString(0),
            Os: Enum.TryParse<OsType>(reader.GetString(1), true, out var os) ? os : OsType.Unknown,
            Criticality: Enum.TryParse<Criticality>(reader.GetString(2), true, out var crit) ? crit : Criticality.Normal,
            Tags: string.IsNullOrWhiteSpace(tagsStr) ? [] : tagsStr.Split(',', StringSplitOptions.RemoveEmptyEntries),
            CompliancePolicies: string.IsNullOrWhiteSpace(policiesStr) ? [] : policiesStr.Split(',', StringSplitOptions.RemoveEmptyEntries));
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}
