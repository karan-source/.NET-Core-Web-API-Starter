using FluentValidation;

namespace ProductionApi.Application.Products.Queries.GetProducts;

public sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(query => query.PageNumber)
            .GreaterThanOrEqualTo(1);

        // Upper bound stops a caller from requesting the entire table in one page.
        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(query => query.Search)
            .MaximumLength(200);
    }
}
