namespace FreelanceOps.Application.Identity.RefreshToken;

public sealed record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAtUtc);
