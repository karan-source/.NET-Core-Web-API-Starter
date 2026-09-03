using System.Data;
using Microsoft.Data.Sqlite;

namespace ProductionApi.Infrastructure.Persistence.Queries;

internal sealed class SqliteConnectionFactory(string connectionString) : ISqlConnectionFactory
{
    public IDbConnection Create() => new SqliteConnection(connectionString);
}
