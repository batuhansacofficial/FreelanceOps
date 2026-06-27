using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Identity.Login;

public sealed class LoginHandler(
    IApplicationDbContext dbContext,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    IValidator<LoginCommand> validator)
{
    public async Task<LoginResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var email = User.NormalizeEmail(command.Email);
        var user = await dbContext.Users
            .FirstOrDefaultAsync(user => user.Email == email && user.IsActive, cancellationToken);

        if (user is null || !passwordHasher.Verify(user.PasswordHash, command.Password))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        user.MarkLoggedIn();

        var refreshToken = jwtTokenGenerator.GenerateRefreshToken();
        var refreshTokenHash = jwtTokenGenerator.HashRefreshToken(refreshToken);
        var refreshTokenEntity = user.AddRefreshToken(
            refreshTokenHash,
            jwtTokenGenerator.GetRefreshTokenExpirationUtc());

        dbContext.RefreshTokens.Add(refreshTokenEntity);

        var accessToken = jwtTokenGenerator.GenerateAccessToken(user);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new LoginResponse(
            accessToken.AccessToken,
            refreshToken,
            accessToken.ExpiresAtUtc,
            new LoginUserResponse(user.Id, user.Email, user.FullName));
    }
}
