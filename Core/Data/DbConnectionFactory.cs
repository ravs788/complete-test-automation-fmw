using System.Data.Common;

namespace Core.Data;

/// <summary>
/// Opens connections for any ADO.NET provider (SQL Server, PostgreSQL, SQLite, …) without Core depending on one:
/// the test project references the provider package and passes its factory, e.g. <c>NpgsqlFactory.Instance</c> or
/// <c>SqlClientFactory.Instance</c>. Keep the connection string in settings, with the password as a
/// <c>secret://NAME</c> placeholder (R36).
/// </summary>
public sealed class DbConnectionFactory(DbProviderFactory provider, string connectionString)
{
    public async Task<DbConnection> OpenAsync(CancellationToken cancellationToken = default)
    {
        var connection = provider.CreateConnection()
                         ?? throw new InvalidOperationException($"{provider.GetType().Name} can't create connections.");
        connection.ConnectionString = connectionString;
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    /// <summary>Opens a connection with a transaction that is rolled back when the scope ends, unless committed.</summary>
    public async Task<DbTestScope> BeginScopeAsync(CancellationToken cancellationToken = default)
    {
        var connection = await OpenAsync(cancellationToken);
        var transaction = await connection.BeginTransactionAsync(cancellationToken);
        return new DbTestScope(connection, transaction);
    }
}
