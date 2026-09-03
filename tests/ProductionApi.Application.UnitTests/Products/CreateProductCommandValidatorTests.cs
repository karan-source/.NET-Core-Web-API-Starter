using ProductionApi.Application.Products.Commands.CreateProduct;

namespace ProductionApi.Application.UnitTests.Products;

public sealed class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidCommand_Passes()
    {
        var command = new CreateProductCommand("Laptop Stand", "Aluminium.", 79.99m, 10);

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Validate_WithMissingName_Fails(string name)
    {
        var command = new CreateProductCommand(name, null, 79.99m, 10);

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateProductCommand.Name));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_WithNonPositivePrice_Fails(decimal price)
    {
        var command = new CreateProductCommand("Laptop Stand", null, price, 10);

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateProductCommand.Price));
    }

    [Fact]
    public async Task Validate_WithNegativeStock_Fails()
    {
        var command = new CreateProductCommand("Laptop Stand", null, 79.99m, -5);

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateProductCommand.StockQuantity));
    }

    [Fact]
    public async Task Validate_WithNameOverMaxLength_Fails()
    {
        var command = new CreateProductCommand(new string('x', 201), null, 79.99m, 10);

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
    }
}
