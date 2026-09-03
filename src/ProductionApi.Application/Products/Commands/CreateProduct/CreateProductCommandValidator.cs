using FluentValidation;

namespace ProductionApi.Application.Products.Commands.CreateProduct;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.Description)
            .MaximumLength(2000);

        RuleFor(command => command.Price)
            .GreaterThan(0);

        RuleFor(command => command.StockQuantity)
            .GreaterThanOrEqualTo(0);
    }
}
