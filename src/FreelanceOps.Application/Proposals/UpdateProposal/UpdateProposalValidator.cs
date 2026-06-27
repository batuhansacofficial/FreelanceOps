using FluentValidation;

namespace FreelanceOps.Application.Proposals.UpdateProposal;

public sealed class UpdateProposalValidator : AbstractValidator<UpdateProposalCommand>
{
    public UpdateProposalValidator()
    {
        RuleFor(command => command.WorkspaceId)
            .NotEmpty();

        RuleFor(command => command.ProposalId)
            .NotEmpty();

        RuleFor(command => command.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.Scope)
            .NotEmpty()
            .MaximumLength(4000);

        RuleFor(command => command.Currency)
            .NotEmpty()
            .Length(3);

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
