using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Identity.RefreshToken;

public sealed class RefreshTokenHandler(
    IApplicationDbContext dbContext,
    IJwtTokenGenerator jwtTokenGenerator,
    IValidator<RefreshTokenCommand> validator)
{
    public async Task<RefreshTokenResponse> Handle(
        RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var tokenHash = jwtTokenGenerator.HashRefreshToken(command.RefreshToken);
        var storedRefreshToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(refreshToken => refreshToken.TokenHash == tokenHash, cancellationToken);

        if (storedRefreshToken is null || !storedRefreshToken.IsActive)
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }

        var user = await dbContext.Users
            .FirstOrDefaultAsync(user =>
                user.Id == storedRefreshToken.UserId &&
                user.IsActive,
                cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }

        storedRefreshToken.Revoke();

        var refreshToken = jwtTokenGenerator.GenerateRefreshToken();
        var refreshTokenHash = jwtTokenGenerator.HashRefreshToken(refreshToken);
        var refreshTokenEntity = user.AddRefreshToken(
            refreshTokenHash,
            jwtTokenGenerator.GetRefreshTokenExpirationUtc());

        dbContext.RefreshTokens.Add(refreshTokenEntity);

        var accessToken = jwtTokenGenerator.GenerateAccessToken(user);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new RefreshTokenResponse(
            accessToken.AccessToken,
            refreshToken,
            accessToken.ExpiresAtUtc);
    }
}
