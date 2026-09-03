using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductionApi.Application.Common.Exceptions;
using ProductionApi.Application.Common.Interfaces;
using ProductionApi.Domain.Entities;

namespace ProductionApi.Application.Products.Commands.DeleteProduct;

public sealed class DeleteProductCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteProductCommand>
{
    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await context.Products
            .FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        context.Products.Remove(product);
        await context.SaveChangesAsync(cancellationToken);
    }
}
