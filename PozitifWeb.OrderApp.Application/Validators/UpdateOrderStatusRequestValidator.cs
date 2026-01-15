using FluentValidation;
using PozitifWeb.OrderApp.Application.DTOs;

namespace PozitifWeb.OrderApp.Application.Validators;

public class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}