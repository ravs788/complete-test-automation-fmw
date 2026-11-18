using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Core.Data;

namespace Tests.Unit.Data;

public sealed class DbFailureTests
{
    [Theory]
    [InlineData("ConnectionString")]
    [InlineData("Open")]
    [InlineData("Begin")]
    [InlineData("CancelledOpen")]
    [InlineData("CancelledBegin")]
    public async Task Setup_Should_DisposeConnection_And_PreserveOriginalFailure(string stage)
    {
        // Arrange
        var original = stage.StartsWith("Cancelled", StringComparison.Ordinal)
            ? (Exception)new OperationCanceledException("cancelled") : new IOException(stage);
        var connection = new TrackingConnection(stage, original) { DisposeFailure = new IOException("dispose") };
        var factory = new DbConnectionFactory(new Provider(connection), "test");

        // Act
        var error = await Record.ExceptionAsync(async () => await factory.BeginScopeAsync());

        // Assert
        Assert.Same(original, error);
        Assert.Equal(1, connection.DisposeCalls);
    }

    [Theory]
    [InlineData(true, true, true, "rollback")]
    [InlineData(false, true, true, "transaction")]
    [InlineData(false, false, true, "connection")]
    [InlineData(false, false, false, null)]
    public async Task Disposal_Should_AttemptEveryResource_And_PreserveFirstFailure(
        bool rollbackFails, bool transactionFails, bool connectionFails, string? expected)
    {
        // Arrange
        var connection = new TrackingConnection("", new IOException())
        {
            DisposeFailure = connectionFails ? new IOException("connection") : null
        };
        var transaction = new TrackingTransaction(connection)
        {
            RollbackFailure = rollbackFails ? new IOException("rollback") : null,
            DisposeFailure = transactionFails ? new IOException("transaction") : null
        };
        var scope = new DbTestScope(connection, transaction);

        // Act
        var error = await Record.ExceptionAsync(async () => await scope.DisposeAsync());

        // Assert
        Assert.Equal(expected, error?.Message);
        Assert.Equal(1, transaction.RollbackCalls);
        Assert.Equal(1, transaction.DisposeCalls);
        Assert.Equal(1, connection.DisposeCalls);
    }

    [Fact]
    public async Task CommittedScope_Should_SkipRollback_ButStillDisposeBothResources()
    {
        // Arrange
        var connection = new TrackingConnection("", new IOException());
        var transaction = new TrackingTransaction(connection) { DisposeFailure = new IOException("transaction") };
        var scope = new DbTestScope(connection, transaction);
        await scope.CommitAsync();

        // Act & Assert
        var error = await Assert.ThrowsAsync<IOException>(async () => await scope.DisposeAsync());

        // Assert: committed changes skip rollback, while both resources are disposed.
        Assert.Equal("transaction", error.Message);
        Assert.Equal(0, transaction.RollbackCalls);
        Assert.Equal(1, transaction.DisposeCalls);
        Assert.Equal(1, connection.DisposeCalls);
    }

    private sealed class Provider(TrackingConnection connection) : DbProviderFactory
    {
        public override DbConnection CreateConnection() => connection;
    }

    private sealed class TrackingConnection(string stage, Exception failure) : DbConnection
    {
        public int DisposeCalls { get; private set; }
        public Exception? DisposeFailure { get; init; }
        [AllowNull]
        public override string ConnectionString
        {
            get => "test";
            set { if (stage == "ConnectionString") throw failure; }
        }
        public override string Database => "test";
        public override string DataSource => "test";
        public override string ServerVersion => "1";
        public override ConnectionState State => ConnectionState.Open;
        public override void Open() => throw new NotSupportedException();
        public override Task OpenAsync(CancellationToken cancellationToken) =>
            stage is "Open" or "CancelledOpen" ? Task.FromException(failure) : Task.CompletedTask;
        public override void Close() { }
        public override void ChangeDatabase(string databaseName) => throw new NotSupportedException();
        protected override DbCommand CreateDbCommand() => throw new NotSupportedException();
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => throw new NotSupportedException();
        protected override ValueTask<DbTransaction> BeginDbTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken) =>
            stage is "Begin" or "CancelledBegin"
                ? ValueTask.FromException<DbTransaction>(failure)
                : ValueTask.FromResult<DbTransaction>(new TrackingTransaction(this));
        public override ValueTask DisposeAsync()
        {
            DisposeCalls++;
            return DisposeFailure is null ? ValueTask.CompletedTask : ValueTask.FromException(DisposeFailure);
        }
    }

    private sealed class TrackingTransaction(DbConnection connection) : DbTransaction
    {
        public Exception? RollbackFailure { get; init; }
        public Exception? DisposeFailure { get; init; }
        public int RollbackCalls { get; private set; }
        public int DisposeCalls { get; private set; }
        public override IsolationLevel IsolationLevel => IsolationLevel.Serializable;
        protected override DbConnection DbConnection => connection;
        public override void Commit() { }
        public override void Rollback() => throw new NotSupportedException();
        public override Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            RollbackCalls++;
            return RollbackFailure is null ? Task.CompletedTask : Task.FromException(RollbackFailure);
        }
        public override ValueTask DisposeAsync()
        {
            DisposeCalls++;
            return DisposeFailure is null ? ValueTask.CompletedTask : ValueTask.FromException(DisposeFailure);
        }
    }
}
