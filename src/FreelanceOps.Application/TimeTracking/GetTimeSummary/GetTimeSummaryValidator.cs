using FluentValidation;

namespace FreelanceOps.Application.TimeTracking.GetTimeSummary;

public sealed class GetTimeSummaryValidator : AbstractValidator<GetTimeSummaryQuery>
{
    public GetTimeSummaryValidator()
    {
        RuleFor(query => query.WorkspaceId)
            .NotEmpty();

        RuleFor(query => query)
            .Must(query =>
                !query.From.HasValue ||
                !query.To.HasValue ||
                query.To.Value >= query.From.Value)
            .WithMessage("To date cannot be before from date.");
    }
}
