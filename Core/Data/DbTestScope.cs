using System.Data.Common;
using System.Runtime.ExceptionServices;

namespace Core.Data;

/// <summary>
/// A connection plus transaction for one test: seed state or check persistence, and everything is rolled back when
/// the scope is disposed (unless <see cref="CommitAsync"/> was called), so tests leave no rows behind. Works with raw
/// ADO.NET commands or with Dapper on <see cref="Connection"/> / <see cref="Transaction"/>.
/// </summary>
public sealed class DbTestScope(DbConnection connection, DbTransaction transaction) : IAsyncDisposable
{
    private bool _committed;

    public DbConnection Connection { get; } = connection;
    public DbTransaction Transaction { get; } = transaction;

    /// <summary>Runs a statement inside the scope's transaction and returns the number of affected rows.</summary>
    public async Task<int> ExecuteAsync(string sql, params (string Name, object? Value)[] parameters)
    {
        await using var command = CreateCommand(sql, parameters);
        return await command.ExecuteNonQueryAsync();
    }

    /// <summary>Runs a query inside the scope's transaction and returns the first column of the first row.</summary>
    public async Task<T?> ScalarAsync<T>(string sql, params (string Name, object? Value)[] parameters)
    {
        await using var command = CreateCommand(sql, parameters);
        var value = await command.ExecuteScalarAsync();
        return value is null or DBNull ? default : (T)Convert.ChangeType(value, Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T));
    }

    /// <summary>Keeps the changes (by default they are rolled back).</summary>
    public async Task CommitAsync()
    {
        await Transaction.CommitAsync();
        _committed = true;
    }

    public async ValueTask DisposeAsync()
    {
        ExceptionDispatchInfo? failure = null;
        try
        {
            if (!_committed)
            {
                await Transaction.RollbackAsync();
            }
        }
        catch (Exception ex) { failure = ExceptionDispatchInfo.Capture(ex); }
        try { await Transaction.DisposeAsync(); }
        catch (Exception ex) { failure ??= ExceptionDispatchInfo.Capture(ex); }
        try { await Connection.DisposeAsync(); }
        catch (Exception ex) { failure ??= ExceptionDispatchInfo.Capture(ex); }
        failure?.Throw();
    }

    private DbCommand CreateCommand(string sql, (string Name, object? Value)[] parameters)
    {
        var command = Connection.CreateCommand();
        command.Transaction = Transaction;
        command.CommandText = sql;
        foreach (var (name, value) in parameters)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }
        return command;
    }
}
