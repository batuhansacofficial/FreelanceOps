using FluentValidation;

namespace FreelanceOps.Application.Reports.GetProjectPerformance;

public sealed class GetProjectPerformanceValidator : AbstractValidator<GetProjectPerformanceQuery>
{
    public GetProjectPerformanceValidator()
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
