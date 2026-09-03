using MediatR;

namespace ProductionApi.Application.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity) : IRequest<Guid>;
