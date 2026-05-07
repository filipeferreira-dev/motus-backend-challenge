using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public class ListSalesValidator : AbstractValidator<ListSalesCommand>
{
    public ListSalesValidator()
    {
        RuleFor(c => c.Page).GreaterThan(0);
        RuleFor(c => c.Size).InclusiveBetween(1, 100);
    }
}
