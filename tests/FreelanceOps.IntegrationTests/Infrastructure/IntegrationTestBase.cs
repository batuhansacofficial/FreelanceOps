using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace FreelanceOps.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase
    : IClassFixture<CustomWebApplicationFactory>
{
    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    protected CustomWebApplicationFactory Factory { get; }

    protected HttpClient Client { get; }

    protected HttpClient CreateAuthenticatedClient(TestUserContext user)
    {
        var client = Factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", user.AccessToken);

        return client;
    }

    protected static async Task<T> ReadAsAsync<T>(HttpResponseMessage response)
    {
        var result = await response.Content.ReadFromJsonAsync<T>();

        if (result is null)
        {
            throw new InvalidOperationException("Response body could not be deserialized.");
        }

        return result;
    }
}
