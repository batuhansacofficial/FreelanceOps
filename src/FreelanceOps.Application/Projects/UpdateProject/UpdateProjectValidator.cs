using FluentValidation;

namespace FreelanceOps.Application.Projects.UpdateProject;

public sealed class UpdateProjectValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectValidator()
    {
        RuleFor(command => command.WorkspaceId)
            .NotEmpty();

        RuleFor(command => command.ProjectId)
            .NotEmpty();

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(160);

        RuleFor(command => command.Description)
            .MaximumLength(4000);

        RuleFor(command => command)
            .Must(command =>
                !command.StartDate.HasValue ||
                !command.Deadline.HasValue ||
                command.Deadline.Value >= command.StartDate.Value)
            .WithMessage("Deadline cannot be before start date.");
    }
}
