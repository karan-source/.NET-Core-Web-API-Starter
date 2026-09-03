using MediatR;
using ProductionApi.Application.Common.Interfaces;
using ProductionApi.Application.Common.Models;

namespace ProductionApi.Application.Products.Queries.GetProducts;

public sealed class GetProductsQueryHandler(IProductReadRepository repository)
    : IRequestHandler<GetProductsQuery, PaginatedList<ProductDto>>
{
    public Task<PaginatedList<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        => repository.GetPagedAsync(request.PageNumber, request.PageSize, request.Search, cancellationToken);
}
