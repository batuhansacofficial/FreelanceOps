using FluentValidation;

namespace FreelanceOps.Application.Billing.RecordPayment;

public sealed class RecordPaymentValidator : AbstractValidator<RecordPaymentCommand>
{
    public RecordPaymentValidator()
    {
        RuleFor(command => command.WorkspaceId)
            .NotEmpty();

        RuleFor(command => command.InvoiceId)
            .NotEmpty();

        RuleFor(command => command.Amount)
            .GreaterThan(0);

        RuleFor(command => command.Method)
            .IsInEnum();

        RuleFor(command => command.Reference)
            .MaximumLength(200);

        RuleFor(command => command.PaidAt)
            .Must(paidAt => paidAt <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Payment date cannot be in the future.");
    }
}
