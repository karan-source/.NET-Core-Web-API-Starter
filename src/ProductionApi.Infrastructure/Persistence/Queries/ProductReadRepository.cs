using Dapper;
using ProductionApi.Application.Common.Interfaces;
using ProductionApi.Application.Common.Models;
using ProductionApi.Application.Products;

namespace ProductionApi.Infrastructure.Persistence.Queries;

internal sealed class ProductReadRepository(ISqlConnectionFactory connectionFactory) : IProductReadRepository
{
    private const string Sql =
        """
        SELECT COUNT(*)
        FROM Products
        WHERE (@Search IS NULL OR Name LIKE @Pattern ESCAPE '\');

        SELECT Id, Name, Description, Price, StockQuantity, IsActive, CreatedAtUtc
        FROM Products
        WHERE (@Search IS NULL OR Name LIKE @Pattern ESCAPE '\')
        ORDER BY CreatedAtUtc DESC
        LIMIT @PageSize OFFSET @Offset;
        """;

    public async Task<PaginatedList<ProductDto>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? search,
        CancellationToken cancellationToken)
    {
        var parameters = new
        {
            Search = search,
            Pattern = search is null ? null : $"%{EscapeLikePattern(search)}%",
            PageSize = pageSize,
            Offset = (pageNumber - 1) * pageSize
        };

        using var connection = connectionFactory.Create();
        var command = new CommandDefinition(Sql, parameters, cancellationToken: cancellationToken);

        using var results = await connection.QueryMultipleAsync(command);
        var totalCount = await results.ReadSingleAsync<int>();
        var rows = await results.ReadAsync<ProductReadRow>();

        var items = rows
            .Select(row => new ProductDto(
                row.Id,
                row.Name,
                row.Description,
                row.Price,
                row.StockQuantity,
                row.IsActive,
                row.CreatedAtUtc))
            .ToList();

        return new PaginatedList<ProductDto>(items, pageNumber, pageSize, totalCount);
    }

    // Values are parameterised, but LIKE wildcards inside the value still need neutralising.
    private static string EscapeLikePattern(string value)
        => value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
