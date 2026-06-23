using FluentValidation;

namespace FreelanceOps.Application.Proposals.ConvertProposalToProject;

public sealed class ConvertProposalToProjectValidator : AbstractValidator<ConvertProposalToProjectCommand>
{
    public ConvertProposalToProjectValidator()
    {
        RuleFor(command => command.WorkspaceId)
            .NotEmpty();

        RuleFor(command => command.ProposalId)
            .NotEmpty();

        RuleFor(command => command)
            .Must(command =>
                !command.StartDate.HasValue ||
                !command.Deadline.HasValue ||
                command.Deadline.Value >= command.StartDate.Value)
            .WithMessage("Deadline cannot be before start date.");
    }
}
