using Microsoft.Data.SqlClient;
using Respawn;
using Respawn.Graph;
using System.Data.Common;

namespace BookCatalog.IntegrationTests.Infrastructure;

internal class DatabaseResetter : IAsyncDisposable
{
    private readonly DbConnection _connection;
    private readonly Respawner _respawner;

    private DatabaseResetter(DbConnection connection, Respawner respawner)
    {
        _connection = connection;
        _respawner = respawner;
    }
    public static async Task<DatabaseResetter> CreateAsync(string connectionString)
    {
        var sqlConnection = new SqlConnection(connectionString);

        await sqlConnection.OpenAsync();
        var respawner = await Respawner.CreateAsync(sqlConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            TablesToIgnore = [new Table("__EFMigrationsHistory")]
        });
        await sqlConnection.CloseAsync();
        return new DatabaseResetter(sqlConnection, respawner);
    }
    public async Task ResetAsync()
    {
        await _connection.OpenAsync();
        await _respawner.ResetAsync(_connection);
        await _connection.CloseAsync();
    }
    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
