using EvCharging.Application.DTOs;
using FluentValidation;

namespace EvCharging.Application.Validators;

public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(x => x.Nic).NotEmpty();
        RuleFor(x => x.StationId).NotEmpty();
        RuleFor(x => x.End)
            .Must((req, end) => end > req.Start)
            .WithMessage("End time must be after start");
    }
}

public class UpdateBookingRequestValidator : AbstractValidator<UpdateBookingRequest>
{
    public UpdateBookingRequestValidator()
    {
        RuleFor(x => x.End)
            .Must((req, end) => end > req.Start)
            .WithMessage("End time must be after start");
    }
}
