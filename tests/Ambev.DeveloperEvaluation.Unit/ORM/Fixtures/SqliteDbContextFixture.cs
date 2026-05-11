using Ambev.DeveloperEvaluation.ORM;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.Unit.ORM.Fixtures;

public sealed class SqliteDbContextFixture : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<DefaultContext> _options;

    public DefaultContext Context { get; }

    public SqliteDbContextFixture()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<DefaultContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new DefaultContext(_options);
        Context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = ON;");
        Context.Database.EnsureCreated();
    }

    public DefaultContext NewContext() => new DefaultContext(_options);

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
