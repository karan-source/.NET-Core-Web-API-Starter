using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductionApi.Application.Common.Exceptions;
using ProductionApi.Application.Common.Interfaces;
using ProductionApi.Domain.Entities;

namespace ProductionApi.Application.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandler(IApplicationDbContext context, IDateTimeProvider clock)
    : IRequestHandler<UpdateProductCommand>
{
    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await context.Products
            .FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.IsActive = request.IsActive;
        product.LastModifiedAtUtc = clock.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
    }
}
