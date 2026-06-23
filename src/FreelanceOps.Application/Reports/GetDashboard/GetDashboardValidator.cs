using FluentValidation;

namespace FreelanceOps.Application.Reports.GetDashboard;

public sealed class GetDashboardValidator : AbstractValidator<GetDashboardQuery>
{
    public GetDashboardValidator()
    {
        RuleFor(query => query.WorkspaceId)
            .NotEmpty();
    }
}
