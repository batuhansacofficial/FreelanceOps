using FluentValidation;

namespace FreelanceOps.Application.Billing.CreateInvoice;

public sealed class CreateInvoiceValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceValidator()
    {
        RuleFor(command => command.WorkspaceId)
            .NotEmpty();

        RuleFor(command => command.ClientId)
            .NotEmpty();

        RuleFor(command => command.DueDate)
            .GreaterThanOrEqualTo(command => command.IssueDate);

        RuleFor(command => command.Currency)
            .NotEmpty()
            .Length(3);

        RuleFor(command => command.Notes)
            .MaximumLength(2000);

        RuleFor(command => command.Items)
            .NotNull()
            .NotEmpty();

        RuleForEach(command => command.Items)
            .ChildRules(item =>
            {
                item.RuleFor(value => value.Description)
                    .NotEmpty()
                    .MaximumLength(500);

                item.RuleFor(value => value.Quantity)
                    .GreaterThan(0);

                item.RuleFor(value => value.UnitPrice)
                    .GreaterThanOrEqualTo(0);

                item.RuleFor(value => value.TaxRate)
                    .InclusiveBetween(0, 100);
            });
    }
}
