using System.Net;
using System.Net.Http.Json;
using WineApp.Tests.Helpers;

namespace WineApp.Tests;

public class AuthTests(TestWebFactory factory) : IClassFixture<TestWebFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_ValidCredentials_Returns200WithToken()
    {
        var email = TestClient.UniqueEmail();
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "Password123!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrEmpty(body.Token));
        Assert.Equal(email, body.Email);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var email = TestClient.UniqueEmail();
        await _client.PostAsJsonAsync("/api/auth/register", new { email, password = "Password123!" });
        var response = await _client.PostAsJsonAsync("/api/auth/register", new { email, password = "Password123!" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_InvalidEmail_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "not-an-email",
            password = "Password123!"
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_ShortPassword_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = TestClient.UniqueEmail(),
            password = "abc"
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithToken()
    {
        var email = TestClient.UniqueEmail();
        await _client.PostAsJsonAsync("/api/auth/register", new { email, password = "Password123!" });

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email, password = "Password123!" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrEmpty(body.Token));
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var email = TestClient.UniqueEmail();
        await _client.PostAsJsonAsync("/api/auth/register", new { email, password = "Password123!" });

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email, password = "WrongPassword!" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_UnknownEmail_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "nobody@nowhere.com",
            password = "Password123!"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/cellar");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private record AuthResponse(string Token, int UserId, string Email);
}
