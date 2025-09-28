using EvCharging.Application.DTOs;
using FluentValidation;

namespace EvCharging.Application.Validators;

public class CreateOwnerRequestValidator : AbstractValidator<CreateOwnerRequest>
{
    public CreateOwnerRequestValidator()
    {
        RuleFor(x => x.Nic).NotEmpty().Length(9, 12);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(15);
    }
}

public class UpdateOwnerRequestValidator : AbstractValidator<UpdateOwnerRequest>
{
    public UpdateOwnerRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(15);
    }
}
