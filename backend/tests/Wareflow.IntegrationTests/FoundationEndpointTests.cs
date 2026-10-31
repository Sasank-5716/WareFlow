using Microsoft.AspNetCore.Mvc.Testing;

namespace Wareflow.IntegrationTests;

public sealed class FoundationEndpointTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public FoundationEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Status_endpoint_proves_api_starts()
    {
        using var response = await _client.GetAsync("/api/foundation/status");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();

        Assert.Contains("\"status\":\"ok\"", body, StringComparison.OrdinalIgnoreCase);
    }
}
