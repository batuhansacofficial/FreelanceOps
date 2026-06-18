using FluentValidation;

namespace FreelanceOps.Application.TimeTracking.UpdateTimeEntry;

public sealed class UpdateTimeEntryValidator : AbstractValidator<UpdateTimeEntryCommand>
{
    public UpdateTimeEntryValidator()
    {
        RuleFor(command => command.WorkspaceId)
            .NotEmpty();

        RuleFor(command => command.TimeEntryId)
            .NotEmpty();

        RuleFor(command => command.StartedAtUtc)
            .Must(startedAtUtc => startedAtUtc <= DateTime.UtcNow)
            .WithMessage("Start time cannot be in the future.");

        RuleFor(command => command.DurationMinutes)
            .InclusiveBetween(1, 1440);

        RuleFor(command => command.Description)
            .MaximumLength(2000);
    }
}
