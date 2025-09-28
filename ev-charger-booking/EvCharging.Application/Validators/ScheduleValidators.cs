using EvCharging.Application.DTOs;
using EvCharging.Domain.Entities;
using FluentValidation;

namespace EvCharging.Application.Validators;

public class UpsertScheduleRequestValidator : AbstractValidator<UpsertScheduleRequest>
{
    public UpsertScheduleRequestValidator()
    {
        RuleFor(x => x.StationId).NotEmpty();
        RuleFor(x => x.Slots).NotNull().Must(s => s.Count > 0)
            .WithMessage("At least one slot is required.");

        RuleForEach(x => x.Slots).SetValidator(new TimeSlotValidator());
    }
}

public class TimeSlotValidator : AbstractValidator<TimeSlot>
{
    public TimeSlotValidator()
    {
        RuleFor(s => s.Capacity).GreaterThan(0).LessThanOrEqualTo(10);
        RuleFor(s => s.End)
            .Must((slot, end) => end > slot.Start)
            .WithMessage("Slot end must be after start");
    }
}
