using FluentValidation;

namespace FreelanceOps.Application.Reports.GetRevenueReport;

public sealed class GetRevenueReportValidator : AbstractValidator<GetRevenueReportQuery>
{
    public GetRevenueReportValidator()
    {
        RuleFor(query => query.WorkspaceId)
            .NotEmpty();

        RuleFor(query => query.GroupBy)
            .Must(groupBy =>
                string.Equals(groupBy, "day", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(groupBy, "month", StringComparison.OrdinalIgnoreCase))
            .WithMessage("GroupBy must be either day or month.");

        RuleFor(query => query)
            .Must(query => ReportDateRange.IsOrdered(query.From, query.To))
            .WithMessage("To date cannot be before from date.");

        RuleFor(query => query)
            .Must(query => ReportDateRange.IsWithinMaximumRange(query.From, query.To))
            .WithMessage("Report date range cannot exceed 366 days.");
    }
}
