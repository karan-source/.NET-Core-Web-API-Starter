using ProductionApi.Domain.Common;

namespace ProductionApi.Domain.Entities;

public class Product : BaseAuditableEntity
{
    public required string Name { get; set; }

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public bool IsActive { get; set; } = true;
}
