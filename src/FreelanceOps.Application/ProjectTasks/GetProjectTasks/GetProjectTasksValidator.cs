using FluentValidation;

namespace FreelanceOps.Application.ProjectTasks.GetProjectTasks;

public sealed class GetProjectTasksValidator : AbstractValidator<GetProjectTasksQuery>
{
    public GetProjectTasksValidator()
    {
        RuleFor(query => query.WorkspaceId)
            .NotEmpty();

        RuleFor(query => query.ProjectId)
            .NotEmpty();

        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(query => query.Status)
            .IsInEnum()
            .When(query => query.Status.HasValue);
    }
}
