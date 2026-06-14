namespace FreelanceOps.Application.Identity.Login;

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAtUtc,
    LoginUserResponse User);

public sealed record LoginUserResponse(Guid Id, string Email, string FullName);
