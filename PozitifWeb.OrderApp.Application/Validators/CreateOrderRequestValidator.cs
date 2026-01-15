using FluentValidation;
using PozitifWeb.OrderApp.Application.DTOs;

namespace PozitifWeb.OrderApp.Application.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0);

        RuleFor(x => x.Items)
            .NotNull()
            .Must(items => items.Count >= 1)
            .WithMessage("Her sipariş en az 1 adet eşya içermelidir.");

        RuleForEach(x => x.Items).ChildRules(i =>
        {
            i.RuleFor(x => x.ProductName).NotEmpty().MaximumLength(200);
            i.RuleFor(x => x.Quantity).GreaterThan(0);
            i.RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}