using FluentValidation;

namespace ProductionApi.Application.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();

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
