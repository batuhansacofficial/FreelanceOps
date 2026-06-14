using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Identity.Logout;

public sealed class LogoutHandler(
    IApplicationDbContext dbContext,
    IJwtTokenGenerator jwtTokenGenerator,
    ICurrentUserService currentUserService,
    IValidator<LogoutCommand> validator)
{
    public async Task Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var userId = currentUserService.UserId;

        if (!currentUserService.IsAuthenticated || userId is null)
        {
            throw new UnauthorizedException("Authentication is required.");
        }

        var tokenHash = jwtTokenGenerator.HashRefreshToken(command.RefreshToken);
        var refreshToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(token =>
                token.TokenHash == tokenHash &&
                token.UserId == userId.Value,
                cancellationToken);

        if (refreshToken is null || !refreshToken.IsActive)
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }

        refreshToken.Revoke();

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
