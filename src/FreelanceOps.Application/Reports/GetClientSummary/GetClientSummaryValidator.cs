using FluentValidation;

namespace FreelanceOps.Application.Reports.GetClientSummary;

public sealed class GetClientSummaryValidator : AbstractValidator<GetClientSummaryQuery>
{
    public GetClientSummaryValidator()
    {
        RuleFor(query => query.WorkspaceId)
            .NotEmpty();

        RuleFor(query => query)
            .Must(query => ReportDateRange.IsOrdered(query.From, query.To))
            .WithMessage("To date cannot be before from date.");

        RuleFor(query => query)
            .Must(query => ReportDateRange.IsWithinMaximumRange(query.From, query.To))
            .WithMessage("Report date range cannot exceed 366 days.");
    }
}
