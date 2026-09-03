using System.Data;
using System.Globalization;
using Dapper;

namespace ProductionApi.Infrastructure.Persistence.Queries;

/// <summary>
/// SQLite has no native GUID, decimal or DateTimeOffset type, so EF Core persists them as TEXT.
/// These handlers let Dapper read the same columns EF Core writes.
/// </summary>
internal static class SqliteTypeHandlers
{
    private static bool _registered;

    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        SqlMapper.AddTypeHandler(new GuidHandler());
        SqlMapper.AddTypeHandler(new DecimalHandler());
        SqlMapper.AddTypeHandler(new DateTimeOffsetHandler());
        _registered = true;
    }

    private sealed class GuidHandler : SqlMapper.TypeHandler<Guid>
    {
        public override Guid Parse(object value) => value switch
        {
            Guid guid => guid,
            string text => Guid.Parse(text),
            byte[] bytes => new Guid(bytes),
            _ => throw new DataException($"Cannot convert {value.GetType()} to Guid.")
        };

        public override void SetValue(IDbDataParameter parameter, Guid value)
        {
            parameter.DbType = DbType.String;
            parameter.Value = value.ToString();
        }
    }

    private sealed class DecimalHandler : SqlMapper.TypeHandler<decimal>
    {
        public override decimal Parse(object value)
            => Convert.ToDecimal(value, CultureInfo.InvariantCulture);

        public override void SetValue(IDbDataParameter parameter, decimal value)
        {
            parameter.DbType = DbType.String;
            parameter.Value = value.ToString(CultureInfo.InvariantCulture);
        }
    }

    private sealed class DateTimeOffsetHandler : SqlMapper.TypeHandler<DateTimeOffset>
    {
        public override DateTimeOffset Parse(object value) => value switch
        {
            DateTimeOffset offset => offset,
            DateTime dateTime => new DateTimeOffset(dateTime, TimeSpan.Zero),
            string text => DateTimeOffset.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal),
            _ => throw new DataException($"Cannot convert {value.GetType()} to DateTimeOffset.")
        };

        public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
        {
            parameter.DbType = DbType.String;
            parameter.Value = value.ToString("O", CultureInfo.InvariantCulture);
        }
    }
}
