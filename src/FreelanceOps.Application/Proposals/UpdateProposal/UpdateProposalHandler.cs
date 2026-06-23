using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using FreelanceOps.Domain.Proposals;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Proposals.UpdateProposal;

public sealed class UpdateProposalHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    IValidator<UpdateProposalCommand> validator)
{
    public async Task Handle(
        UpdateProposalCommand command,
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

        var proposal = await dbContext.Proposals
            .Include(proposal => proposal.Items)
            .FirstOrDefaultAsync(
                proposal =>
                    proposal.Id == command.ProposalId &&
                    proposal.WorkspaceId == command.WorkspaceId &&
                    !proposal.IsDeleted,
                cancellationToken);

        if (proposal is null)
        {
            throw new NotFoundException("Proposal was not found.");
        }

        await ProposalGuard.EnsureActiveClientAsync(
            dbContext,
            command.WorkspaceId,
            proposal.ClientId,
            cancellationToken);

        proposal.UpdateDetails(
            command.Title,
            command.Scope,
            command.ValidUntil,
            command.Currency);

        var items = command.Items
            .Select(item => new ProposalItem(
                proposal.Id,
                item.Description,
                item.Quantity,
                item.UnitPrice,
                item.TaxRate))
            .ToArray();

        proposal.ReplaceItems(items);
        dbContext.ProposalItems.AddRange(items);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
