namespace FreelanceOps.Application.Abstractions.Authentication;

public sealed record AccessTokenResult(string AccessToken, DateTime ExpiresAtUtc);
