using FluentValidation;

namespace FreelanceOps.Application.Proposals.GetProposals;

public sealed class GetProposalsValidator : AbstractValidator<GetProposalsQuery>
{
    public GetProposalsValidator()
    {
        RuleFor(query => query.WorkspaceId)
            .NotEmpty();

        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(query => query.Search)
            .MaximumLength(200);
    }
}
