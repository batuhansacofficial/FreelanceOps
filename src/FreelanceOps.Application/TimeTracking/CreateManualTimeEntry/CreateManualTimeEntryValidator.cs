using FluentValidation;

namespace FreelanceOps.Application.TimeTracking.CreateManualTimeEntry;

public sealed class CreateManualTimeEntryValidator : AbstractValidator<CreateManualTimeEntryCommand>
{
    public CreateManualTimeEntryValidator()
    {
        RuleFor(command => command.WorkspaceId)
            .NotEmpty();

        RuleFor(command => command.TaskId)
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
