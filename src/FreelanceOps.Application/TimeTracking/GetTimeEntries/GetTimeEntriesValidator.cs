using FluentValidation;

namespace FreelanceOps.Application.TimeTracking.GetTimeEntries;

public sealed class GetTimeEntriesValidator : AbstractValidator<GetTimeEntriesQuery>
{
    public GetTimeEntriesValidator()
    {
        RuleFor(query => query.WorkspaceId)
            .NotEmpty();

        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(query => query)
            .Must(query =>
                !query.From.HasValue ||
                !query.To.HasValue ||
                query.To.Value >= query.From.Value)
            .WithMessage("To date cannot be before from date.");
    }
}
