using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Common.Exceptions;

namespace FreelanceOps.Application.Workspaces;

internal static class CurrentUserExtensions
{
    public static Guid RequireUserId(this ICurrentUserService currentUserService)
    {
        return currentUserService.UserId
            ?? throw new UnauthorizedException("Authentication is required.");
    }
}
