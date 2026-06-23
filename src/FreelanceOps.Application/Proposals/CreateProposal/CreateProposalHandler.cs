using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Proposals;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Workspaces;
using FreelanceOps.Domain.Proposals;

namespace FreelanceOps.Application.Proposals.CreateProposal;

public sealed class CreateProposalHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    IProposalNumberGenerator proposalNumberGenerator,
    IValidator<CreateProposalCommand> validator)
{
    public async Task<CreateProposalResponse> Handle(
        CreateProposalCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var userId = currentUserService.RequireUserId();

        await ProposalGuard.EnsureManagerAsync(
            dbContext,
            workspaceAuthorizationService,
            userId,
            command.WorkspaceId,
            cancellationToken);

        await ProposalGuard.EnsureActiveClientAsync(
            dbContext,
            command.WorkspaceId,
            command.ClientId,
            cancellationToken);

        var createdAtUtc = DateTime.UtcNow;
        var proposalNumber = await proposalNumberGenerator.GenerateAsync(
            command.WorkspaceId,
            createdAtUtc,
            cancellationToken);
        var proposal = new Proposal(
            command.WorkspaceId,
            command.ClientId,
            proposalNumber,
            command.Title,
            command.Scope,
            command.ValidUntil,
            command.Currency);

        foreach (var item in command.Items)
        {
            proposal.AddItem(
                item.Description,
                item.Quantity,
                item.UnitPrice,
                item.TaxRate);
        }

        dbContext.Proposals.Add(proposal);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateProposalResponse(
            proposal.Id,
            proposal.WorkspaceId,
            proposal.ClientId,
            proposal.ConvertedProjectId,
            proposal.ProposalNumber,
            proposal.Title,
            proposal.Scope,
            proposal.Status,
            proposal.ValidUntil,
            proposal.Currency,
            proposal.SubtotalAmount,
            proposal.TaxAmount,
            proposal.TotalAmount,
            proposal.CreatedAtUtc,
            proposal.UpdatedAtUtc);
    }
}
