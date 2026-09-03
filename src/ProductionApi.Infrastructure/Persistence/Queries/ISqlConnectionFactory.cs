using System.Data;

namespace ProductionApi.Infrastructure.Persistence.Queries;

public interface ISqlConnectionFactory
{
    IDbConnection Create();
}
