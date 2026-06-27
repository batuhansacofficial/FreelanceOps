using FluentValidation;

namespace FreelanceOps.Application.Clients.GetClients;

public sealed class GetClientsValidator : AbstractValidator<GetClientsQuery>
{
    public GetClientsValidator()
    {
        RuleFor(query => query.WorkspaceId)
            .NotEmpty();

        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(query => query.Search)
            .MaximumLength(100);
    }
}
