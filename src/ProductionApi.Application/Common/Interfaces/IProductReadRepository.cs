using ProductionApi.Application.Common.Models;
using ProductionApi.Application.Products;

namespace ProductionApi.Application.Common.Interfaces;

/// <summary>Read-side access backed by Dapper, kept separate from the EF Core write model.</summary>
public interface IProductReadRepository
{
    Task<PaginatedList<ProductDto>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? search,
        CancellationToken cancellationToken);
}
