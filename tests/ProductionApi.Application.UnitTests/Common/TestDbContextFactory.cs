using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProductionApi.Infrastructure.Persistence;

namespace ProductionApi.Application.UnitTests.Common;

/// <summary>
/// Backs the context with a real in-memory SQLite database so tests exercise
/// actual relational behaviour instead of the EF in-memory provider's approximation.
/// </summary>
public sealed class TestDbContextFactory : IDisposable
{
    private readonly SqliteConnection _connection;

    public TestDbContextFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new ApplicationDbContext(options);
        Context.Database.EnsureCreated();
    }

    public ApplicationDbContext Context { get; }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
