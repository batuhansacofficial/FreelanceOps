using FluentValidation;

namespace FreelanceOps.Application.TimeTracking.StartTimer;

public sealed class StartTimerValidator : AbstractValidator<StartTimerCommand>
{
    public StartTimerValidator()
    {
        RuleFor(command => command.WorkspaceId)
            .NotEmpty();

        RuleFor(command => command.TaskId)
            .NotEmpty();

        RuleFor(command => command.Description)
            .MaximumLength(2000);
    }
}
