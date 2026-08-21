using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Npgsql;

namespace PackControl.Infrastructure.Persistence;

public sealed class PostgresAppStatePersistence(
    IOptions<StatePersistenceOptions> options) : IAppStatePersistence
{
    private readonly SemaphoreSlim initializationGate = new(1, 1);
    private readonly JsonSerializerSettings jsonSettings = new()
    {
        ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
        NullValueHandling = NullValueHandling.Ignore
    };

    private volatile bool initialized;

    public bool Enabled =>
        options.Value.Provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase) &&
        !string.IsNullOrWhiteSpace(options.Value.ConnectionString);

    public async Task LoadAsync(AppStateStore stateStore, CancellationToken cancellationToken)
    {
        if (!Enabled)
        {
            return;
        }

        await EnsureInitializedAsync(cancellationToken);

        await using var connection = new NpgsqlConnection(options.Value.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = $"select payload from {GetQualifiedTableName()} where snapshot_key = @snapshotKey limit 1;";
        command.Parameters.AddWithValue("snapshotKey", options.Value.SnapshotKey);

        var payload = await command.ExecuteScalarAsync(cancellationToken) as string;
        if (string.IsNullOrWhiteSpace(payload))
        {
            return;
        }

        var snapshot = JsonConvert.DeserializeObject<AppStateSnapshot>(payload, jsonSettings) ?? new AppStateSnapshot();
        lock (stateStore.SyncRoot)
        {
            stateStore.LoadSnapshot(snapshot);
        }
    }

    public async Task SaveAsync(AppStateStore stateStore, CancellationToken cancellationToken)
    {
        if (!Enabled)
        {
            return;
        }

        await EnsureInitializedAsync(cancellationToken);

        string payload;
        lock (stateStore.SyncRoot)
        {
            payload = JsonConvert.SerializeObject(stateStore.ToSnapshot(), jsonSettings);
        }

        await using var connection = new NpgsqlConnection(options.Value.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText =
            $"""
             insert into {GetQualifiedTableName()} (snapshot_key, payload, updated_at_utc)
             values (@snapshotKey, @payload::jsonb, now())
             on conflict (snapshot_key)
             do update set payload = excluded.payload, updated_at_utc = now();
             """;
        command.Parameters.AddWithValue("snapshotKey", options.Value.SnapshotKey);
        command.Parameters.AddWithValue("payload", payload);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (initialized || !Enabled)
        {
            return;
        }

        await initializationGate.WaitAsync(cancellationToken);
        try
        {
            if (initialized)
            {
                return;
            }

            await using var connection = new NpgsqlConnection(options.Value.ConnectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText =
                $"""
                 create schema if not exists {QuoteIdentifier(options.Value.Schema)};
                 create table if not exists {GetQualifiedTableName()} (
                     snapshot_key text primary key,
                     payload jsonb not null,
                     updated_at_utc timestamptz not null default now()
                 );
                 """;
            await command.ExecuteNonQueryAsync(cancellationToken);

            initialized = true;
        }
        finally
        {
            initializationGate.Release();
        }
    }

    private string GetQualifiedTableName() =>
        $"{QuoteIdentifier(options.Value.Schema)}.{QuoteIdentifier("app_state_snapshots")}";

    private static string QuoteIdentifier(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("Schema de persistencia invalido.");
        }

        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
