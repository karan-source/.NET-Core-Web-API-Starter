using MediatR;

namespace ProductionApi.Application.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    bool IsActive) : IRequest;
