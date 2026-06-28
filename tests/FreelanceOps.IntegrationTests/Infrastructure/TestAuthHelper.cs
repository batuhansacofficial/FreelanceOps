using System.Net.Http.Json;

namespace FreelanceOps.IntegrationTests.Infrastructure;

public sealed record TestUserContext(
    Guid UserId,
    string Email,
    string AccessToken,
    string RefreshToken);

public static class TestAuthHelper
{
    public static async Task<TestUserContext> RegisterAndLoginAsync(HttpClient client)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var email = $"user-{Guid.NewGuid():N}@example.com";
        const string password = "Password123!";

        var registerResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                Email = email,
                Password = password,
                FullName = "Test User"
            },
            cancellationToken);

        registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                Email = email,
                Password = password
            },
            cancellationToken);

        loginResponse.EnsureSuccessStatusCode();

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginTestResponse>(cancellationToken);

        if (loginResult is null)
        {
            throw new InvalidOperationException("Login response could not be deserialized.");
        }

        return new TestUserContext(
            loginResult.User.Id,
            email,
            loginResult.AccessToken,
            loginResult.RefreshToken);
    }

    public sealed record LoginTestResponse(
        string AccessToken,
        string RefreshToken,
        DateTime ExpiresAtUtc,
        LoginUserResponse User);

    public sealed record LoginUserResponse(
        Guid Id,
        string Email,
        string FullName);
}
