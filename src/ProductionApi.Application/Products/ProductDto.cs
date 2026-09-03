namespace ProductionApi.Application.Products;

public sealed record ProductDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    DateTimeOffset CreatedAtUtc);
