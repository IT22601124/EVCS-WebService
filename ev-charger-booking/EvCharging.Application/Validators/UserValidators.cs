using EvCharging.Application.DTOs;
using EvCharging.Domain.Enums;
using FluentValidation;

namespace EvCharging.Application.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.Role).Must(r => r == Roles.Backoffice || r == Roles.Operator || r == Roles.Owner);
    }
}

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Role).Must(r => r == Roles.Backoffice || r == Roles.Operator || r == Roles.Owner);
        When(x => !string.IsNullOrWhiteSpace(x.Password), () =>
        {
            RuleFor(x => x.Password!).MinimumLength(6);
        });
    }
}
