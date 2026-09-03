using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductionApi.Application.Common.Exceptions;
using ProductionApi.Application.Common.Interfaces;
using ProductionApi.Domain.Entities;

namespace ProductionApi.Application.Products.Queries.GetProductById;

public sealed class GetProductByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await context.Products
            .AsNoTracking()
            .Where(entity => entity.Id == request.Id)
            .Select(entity => new ProductDto(
                entity.Id,
                entity.Name,
                entity.Description,
                entity.Price,
                entity.StockQuantity,
                entity.IsActive,
                entity.CreatedAtUtc))
            .FirstOrDefaultAsync(cancellationToken);

        return product ?? throw new NotFoundException(nameof(Product), request.Id);
    }
}
