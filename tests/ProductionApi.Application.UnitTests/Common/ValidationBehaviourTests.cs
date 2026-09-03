using FluentValidation;
using ProductionApi.Application.Common.Behaviours;
using ProductionApi.Application.Products.Commands.CreateProduct;
using ValidationException = ProductionApi.Application.Common.Exceptions.ValidationException;

namespace ProductionApi.Application.UnitTests.Common;

public sealed class ValidationBehaviourTests
{
    [Fact]
    public async Task Handle_WithInvalidRequest_ThrowsBeforeReachingTheHandler()
    {
        var handlerWasCalled = false;
        var behaviour = new ValidationBehaviour<CreateProductCommand, Guid>([new CreateProductCommandValidator()]);
        var invalid = new CreateProductCommand(string.Empty, null, -1m, -1);

        var exception = await Assert.ThrowsAsync<ValidationException>(() => behaviour.Handle(
            invalid,
            () =>
            {
                handlerWasCalled = true;
                return Task.FromResult(Guid.NewGuid());
            },
            CancellationToken.None));

        Assert.False(handlerWasCalled);
        Assert.Contains(nameof(CreateProductCommand.Name), exception.Errors.Keys);
        Assert.Contains(nameof(CreateProductCommand.Price), exception.Errors.Keys);
    }

    [Fact]
    public async Task Handle_WithValidRequest_InvokesTheHandler()
    {
        var expected = Guid.NewGuid();
        var behaviour = new ValidationBehaviour<CreateProductCommand, Guid>([new CreateProductCommandValidator()]);
        var valid = new CreateProductCommand("Desk Lamp", null, 25.00m, 4);

        var result = await behaviour.Handle(valid, () => Task.FromResult(expected), CancellationToken.None);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task Handle_WithNoValidators_InvokesTheHandler()
    {
        var expected = Guid.NewGuid();
        var behaviour = new ValidationBehaviour<CreateProductCommand, Guid>([]);

        var result = await behaviour.Handle(
            new CreateProductCommand(string.Empty, null, -1m, -1),
            () => Task.FromResult(expected),
            CancellationToken.None);

        Assert.Equal(expected, result);
    }
}
