using FreelanceOps.Domain.Users;

namespace FreelanceOps.Application.Abstractions.Authentication;

public interface IJwtTokenGenerator
{
    AccessTokenResult GenerateAccessToken(User user);

    string GenerateRefreshToken();

    string HashRefreshToken(string refreshToken);

    DateTime GetRefreshTokenExpirationUtc();
}
