namespace ProductionApi.Infrastructure.Persistence.Queries;

/// <summary>
/// Dapper materialises settable properties (where the registered type handlers apply)
/// rather than a positional record's constructor, so the read model is mapped explicitly.
/// </summary>
internal sealed class ProductReadRow
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
}
