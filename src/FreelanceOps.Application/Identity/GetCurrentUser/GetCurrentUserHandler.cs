using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Identity.GetCurrentUser;

public sealed class GetCurrentUserHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService)
{
    public async Task<CurrentUserResponse> Handle(CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        if (!currentUserService.IsAuthenticated || userId is null)
        {
            throw new UnauthorizedException("Authentication is required.");
        }

        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user =>
                user.Id == userId.Value &&
                user.IsActive,
                cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedException("Authentication is required.");
        }

        return new CurrentUserResponse(user.Id, user.Email, user.FullName);
    }
}
