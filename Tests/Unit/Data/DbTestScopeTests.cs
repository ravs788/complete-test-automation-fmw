using Core.Data;
using Microsoft.Data.Sqlite;

namespace Tests.Unit.Data;

/// <summary>Runs the data helpers against a real (file-based, temporary) SQLite database.</summary>
public sealed class DbTestScopeTests : IAsyncLifetime
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"dbscope-{Guid.NewGuid():N}.db");
    private DbConnectionFactory _db = null!;

    public async Task InitializeAsync()
    {
        _db = new DbConnectionFactory(SqliteFactory.Instance, $"Data Source={_path};Pooling=False");
        await using var scope = await _db.BeginScopeAsync();
        await scope.ExecuteAsync("CREATE TABLE bookings (id INTEGER PRIMARY KEY, name TEXT NOT NULL)");
        await scope.CommitAsync();
    }

    public Task DisposeAsync()
    {
        File.Delete(_path);
        return Task.CompletedTask;
    }

    private async Task<long> CountAsync()
    {
        await using var scope = await _db.BeginScopeAsync();
        return await scope.ScalarAsync<long>("SELECT COUNT(*) FROM bookings");
    }

    [Fact]
    public async Task Scope_Should_RollBack_By_Default()
    {
        // Arrange: the fixture supplies an empty bookings table.
        await using (var scope = await _db.BeginScopeAsync())
        {
            // Arrange: insert a row inside an uncommitted scope.
            await scope.ExecuteAsync("INSERT INTO bookings (name) VALUES ($name)", ("$name", "seeded"));

            // Assert: confirm the row exists inside the transaction.
            Assert.Equal(1L, await scope.ScalarAsync<long>("SELECT COUNT(*) FROM bookings"));

            // Act: leaving the scope disposes it without committing.
        }

        // Assert: implicit disposal rolled the transaction back.
        Assert.Equal(0L, await CountAsync());
    }

    [Fact]
    public async Task Scope_Should_KeepChanges_When_Committed()
    {
        // Arrange: the fixture supplies an empty bookings table.
        await using (var scope = await _db.BeginScopeAsync())
        {
            // Arrange: insert a row inside a new scope.
            await scope.ExecuteAsync("INSERT INTO bookings (name) VALUES ($name)", ("$name", "kept"));

            // Act
            await scope.CommitAsync();
        }

        // Assert: the committed row survives scope disposal.
        Assert.Equal(1L, await CountAsync());
    }

    [Fact]
    public async Task ScalarAsync_Should_UseParameters_And_ReturnDefaultForNoRow()
    {
        // Arrange
        await using var scope = await _db.BeginScopeAsync();
        await scope.ExecuteAsync("INSERT INTO bookings (name) VALUES ($name)", ("$name", "O'Brien"));

        // Act & Assert
        Assert.Equal("O'Brien", await scope.ScalarAsync<string>("SELECT name FROM bookings WHERE name = $name", ("$name", "O'Brien")));
        Assert.Null(await scope.ScalarAsync<string>("SELECT name FROM bookings WHERE name = $name", ("$name", "nobody")));
    }
}
