using FluentValidation;

namespace FreelanceOps.Application.Projects.GetProjects;

public sealed class GetProjectsValidator : AbstractValidator<GetProjectsQuery>
{
    public GetProjectsValidator()
    {
        RuleFor(query => query.WorkspaceId)
            .NotEmpty();

        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(query => query.Search)
            .MaximumLength(100);

        RuleFor(query => query.Status)
            .IsInEnum()
            .When(query => query.Status.HasValue);
    }
}
