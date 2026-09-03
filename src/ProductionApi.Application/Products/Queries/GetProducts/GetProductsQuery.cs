using MediatR;
using ProductionApi.Application.Common.Models;

namespace ProductionApi.Application.Products.Queries.GetProducts;

public sealed record GetProductsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? Search = null) : IRequest<PaginatedList<ProductDto>>;
