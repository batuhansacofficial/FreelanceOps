using FluentValidation;

namespace FreelanceOps.Application.Billing.GetInvoices;

public sealed class GetInvoicesValidator : AbstractValidator<GetInvoicesQuery>
{
    public GetInvoicesValidator()
    {
        RuleFor(query => query.WorkspaceId)
            .NotEmpty();

        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(query => query.Status)
            .IsInEnum()
            .When(query => query.Status.HasValue);

        RuleFor(query => query.Search)
            .MaximumLength(100);

        RuleFor(query => query)
            .Must(query =>
                !query.FromIssueDate.HasValue ||
                !query.ToIssueDate.HasValue ||
                query.ToIssueDate.Value >= query.FromIssueDate.Value)
            .WithMessage("To issue date cannot be before from issue date.");
    }
}
