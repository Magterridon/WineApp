using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WineApp.Api.Data;
using WineApp.Api.Models;

namespace WineApp.Tests.Helpers;

public static class TestClient
{
    private static int _counter;

    public static string UniqueEmail() =>
        $"user{System.Threading.Interlocked.Increment(ref _counter)}@test.com";

    public static async Task<string> RegisterAndGetTokenAsync(
        HttpClient client,
        string? email = null,
        string password = "Password123!")
    {
        email ??= UniqueEmail();
        var response = await client.PostAsJsonAsync("/api/auth/register", new { email, password });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthResult>();
        return result!.Token;
    }

    /// <summary>
    /// Registers a user then promotes them to Admin via the DB, and re-logs in for a token that contains the Admin role claim.
    /// </summary>
    public static async Task<string> RegisterAdminAndGetTokenAsync(
        HttpClient client,
        TestWebFactory factory,
        string? email = null,
        string password = "Password123!")
    {
        email ??= UniqueEmail();

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new { email, password });
        registerResponse.EnsureSuccessStatusCode();

        // Promote the user to Admin directly in the DB
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = db.Users.First(u => u.Email == email);
        user.Role = UserRole.Admin;
        await db.SaveChangesAsync();

        // Re-login so the returned JWT contains the Admin role claim
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        loginResponse.EnsureSuccessStatusCode();
        var result = await loginResponse.Content.ReadFromJsonAsync<AuthResult>();
        return result!.Token;
    }

    public static void Authenticate(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private record AuthResult(string Token, int UserId, string Email, string Role);
}
