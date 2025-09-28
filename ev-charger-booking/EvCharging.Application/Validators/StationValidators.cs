using EvCharging.Application.DTOs;
using FluentValidation;

namespace EvCharging.Application.Validators;

public class CreateStationRequestValidator : AbstractValidator<CreateStationRequest>
{
    public CreateStationRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
        RuleFor(x => x.Type).NotEmpty().Must(t => t == "AC" || t == "DC")
            .WithMessage("Type must be AC or DC");
        RuleFor(x => x.Slots).GreaterThan(0).LessThanOrEqualTo(20);
    }
}

public class UpdateStationRequestValidator : AbstractValidator<UpdateStationRequest>
{
    public UpdateStationRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
        RuleFor(x => x.Type).NotEmpty().Must(t => t == "AC" || t == "DC");
        RuleFor(x => x.Slots).GreaterThan(0).LessThanOrEqualTo(20);
    }
}
