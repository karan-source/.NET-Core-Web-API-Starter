using Microsoft.EntityFrameworkCore;
using Moq;
using ProductionApi.Application.Common.Interfaces;
using ProductionApi.Application.Products.Commands.CreateProduct;
using ProductionApi.Application.UnitTests.Common;

namespace ProductionApi.Application.UnitTests.Products;

public sealed class CreateProductCommandHandlerTests : IDisposable
{
    private static readonly DateTimeOffset FixedNow = new(2026, 1, 15, 9, 30, 0, TimeSpan.Zero);

    private readonly TestDbContextFactory _factory = new();

    [Fact]
    public async Task Handle_PersistsProductAndReturnsId()
    {
        var handler = new CreateProductCommandHandler(_factory.Context, CreateClock());
        var command = new CreateProductCommand("Laptop Stand", "Aluminium.", 79.99m, 10);

        var id = await handler.Handle(command, CancellationToken.None);

        var saved = await _factory.Context.Products.AsNoTracking().SingleAsync(product => product.Id == id);
        Assert.Equal("Laptop Stand", saved.Name);
        Assert.Equal(79.99m, saved.Price);
        Assert.Equal(10, saved.StockQuantity);
        Assert.True(saved.IsActive);
    }

    [Fact]
    public async Task Handle_StampsCreatedAtFromTheClock()
    {
        var handler = new CreateProductCommandHandler(_factory.Context, CreateClock());

        var id = await handler.Handle(new CreateProductCommand("Monitor Arm", null, 59.00m, 3), CancellationToken.None);

        var saved = await _factory.Context.Products.AsNoTracking().SingleAsync(product => product.Id == id);
        Assert.Equal(FixedNow, saved.CreatedAtUtc);
        Assert.Null(saved.LastModifiedAtUtc);
    }

    [Fact]
    public async Task Handle_AssignsDistinctIdentifiers()
    {
        var handler = new CreateProductCommandHandler(_factory.Context, CreateClock());

        var first = await handler.Handle(new CreateProductCommand("Item A", null, 1.00m, 1), CancellationToken.None);
        var second = await handler.Handle(new CreateProductCommand("Item B", null, 2.00m, 1), CancellationToken.None);

        Assert.NotEqual(first, second);
    }

    private static IDateTimeProvider CreateClock()
    {
        var clock = new Mock<IDateTimeProvider>();
        clock.SetupGet(provider => provider.UtcNow).Returns(FixedNow);
        return clock.Object;
    }

    public void Dispose() => _factory.Dispose();
}
