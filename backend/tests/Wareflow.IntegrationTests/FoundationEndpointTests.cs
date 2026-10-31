using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

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

    [Fact]
    public async Task Current_user_requires_authentication()
    {
        using var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Csrf_endpoint_issues_a_token_cookie()
    {
        using var response = await _client.GetAsync("/api/auth/csrf");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Contains(
            response.Headers.GetValues("Set-Cookie"),
            cookie => cookie.StartsWith("XSRF-TOKEN=", StringComparison.Ordinal));
    }

    [Fact]
    public async Task State_changing_auth_requests_require_csrf()
    {
        using var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = "unknown@example.com", password = "not-a-password", rememberMe = false });

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest,
            $"Expected 400 but received {(int)response.StatusCode}: {body}");
    }

    [Fact]
    public async Task Tenant_listing_requires_authentication()
    {
        using var response = await _client.GetAsync("/api/tenants");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Tenant_selection_does_not_accept_an_unauthenticated_identifier()
    {
        using var response = await _client.PostAsync(
            "/api/tenants/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/select",
            content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
