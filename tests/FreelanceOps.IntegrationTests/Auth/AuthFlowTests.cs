using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FreelanceOps.IntegrationTests.Infrastructure;

namespace FreelanceOps.IntegrationTests.Auth;

public sealed class AuthFlowTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Me_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        var response = await Client.GetAsync("/api/auth/me", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_ShouldReturnCreated_WhenRequestIsValid()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                Email = $"user-{Guid.NewGuid():N}@example.com",
                Password = "Password123!",
                FullName = "Test User"
            }, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Register_ShouldReturnConflict_WhenEmailAlreadyExists()
    {
        var email = $"user-{Guid.NewGuid():N}@example.com";
        var request = new
        {
            Email = email,
            Password = "Password123!",
            FullName = "Test User"
        };

        var firstResponse = await Client.PostAsJsonAsync("/api/auth/register", request, TestContext.Current.CancellationToken);
        var secondResponse = await Client.PostAsJsonAsync("/api/auth/register", request, TestContext.Current.CancellationToken);

        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_ShouldReturnTokens_WhenCredentialsAreValid()
    {
        var user = await TestAuthHelper.RegisterAndLoginAsync(Client);

        user.UserId.Should().NotBeEmpty();
        user.AccessToken.Should().NotBeNullOrWhiteSpace();
        user.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnUnauthorized_WhenOldRefreshTokenIsReused()
    {
        var user = await TestAuthHelper.RegisterAndLoginAsync(Client);

        var refreshResponse = await Client.PostAsJsonAsync(
            "/api/auth/refresh-token",
            new
            {
                user.RefreshToken
            }, TestContext.Current.CancellationToken);
        var reuseResponse = await Client.PostAsJsonAsync(
            "/api/auth/refresh-token",
            new
            {
                user.RefreshToken
            }, TestContext.Current.CancellationToken);

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        reuseResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
