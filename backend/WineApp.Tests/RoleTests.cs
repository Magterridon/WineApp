using System.Net;
using System.Net.Http.Json;
using WineApp.Tests.Helpers;

namespace WineApp.Tests;

public class RoleTests(TestWebFactory factory) : IClassFixture<TestWebFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    // ---------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------

    private static int _counter;

    private async Task<int> CreateWineAsAdmin()
    {
        var n = System.Threading.Interlocked.Increment(ref _counter);
        var response = await _client.PostAsJsonAsync("/api/wines", new
        {
            name = $"Role Wine {n}",
            domain = $"Role Domain {n}",
            year = 2018,
            rank = 3,
            cepages = Array.Empty<object>()
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IdResult>())!.Id;
    }

    private async Task<int> CreateRecipeAsAdmin()
    {
        var n = System.Threading.Interlocked.Increment(ref _counter);
        var response = await _client.PostAsJsonAsync("/api/recipes", new
        {
            name = $"Role Recipe {n}",
            description = "Test",
            recipeType = "Main",
            ingredients = new[] { "item" },
            instructions = "Do it",
            pairings = Array.Empty<object>()
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IdResult>())!.Id;
    }

    // ---------------------------------------------------------------
    // Auth model — role in token and response
    // ---------------------------------------------------------------

    [Fact]
    public async Task Register_DefaultsToUserRole()
    {
        var email = TestClient.UniqueEmail();
        var response = await _client.PostAsJsonAsync("/api/auth/register", new { email, password = "Password123!" });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<AuthBody>();
        Assert.Equal("User", body!.Role);
    }

    [Fact]
    public async Task Login_ReturnsCorrectRole()
    {
        var adminToken = await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        TestClient.Authenticate(_client, adminToken);

        // Re-login as admin to verify role in token
        var email = TestClient.UniqueEmail();
        await TestClient.RegisterAdminAndGetTokenAsync(_client, factory, email);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new { email, password = "Password123!" });
        loginResponse.EnsureSuccessStatusCode();
        var body = await loginResponse.Content.ReadFromJsonAsync<AuthBody>();
        Assert.Equal("Admin", body!.Role);
    }

    // ---------------------------------------------------------------
    // Wine endpoints
    // ---------------------------------------------------------------

    [Fact]
    public async Task User_CanCreateWine()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);

        var response = await _client.PostAsJsonAsync("/api/wines", new
        {
            name = $"User Created Wine {_counter++}",
            domain = "SomeDomain",
            year = 2020,
            rank = 2,
            cepages = Array.Empty<object>()
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task User_CannotUpdateWine()
    {
        var adminToken = await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        TestClient.Authenticate(_client, adminToken);
        var wineId = await CreateWineAsAdmin();

        var userToken = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, userToken);

        var response = await _client.PutAsJsonAsync($"/api/wines/{wineId}", new
        {
            name = "Hacked Name",
            domain = "Hacked Domain",
            year = 2020,
            rank = 1,
            cepages = Array.Empty<object>()
        });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task User_CannotDeleteWine()
    {
        var adminToken = await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        TestClient.Authenticate(_client, adminToken);
        var wineId = await CreateWineAsAdmin();

        var userToken = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, userToken);

        var response = await _client.DeleteAsync($"/api/wines/{wineId}");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_CanUpdateWine()
    {
        var adminToken = await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        TestClient.Authenticate(_client, adminToken);
        var wineId = await CreateWineAsAdmin();

        var response = await _client.PutAsJsonAsync($"/api/wines/{wineId}", new
        {
            name = "Updated Name",
            domain = "Updated Domain",
            year = 2021,
            rank = 4,
            cepages = Array.Empty<object>()
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Admin_CanDeleteWine()
    {
        var adminToken = await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        TestClient.Authenticate(_client, adminToken);
        var wineId = await CreateWineAsAdmin();

        var response = await _client.DeleteAsync($"/api/wines/{wineId}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    // ---------------------------------------------------------------
    // Recipe endpoints
    // ---------------------------------------------------------------

    [Fact]
    public async Task User_CannotCreateRecipe()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);

        var response = await _client.PostAsJsonAsync("/api/recipes", new
        {
            name = "Sneaky Recipe",
            description = "Test",
            recipeType = "Main",
            ingredients = new[] { "item" },
            instructions = "Do it",
            pairings = Array.Empty<object>()
        });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task User_CannotUpdateRecipe()
    {
        var adminToken = await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        TestClient.Authenticate(_client, adminToken);
        var recipeId = await CreateRecipeAsAdmin();

        var userToken = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, userToken);

        var response = await _client.PutAsJsonAsync($"/api/recipes/{recipeId}", new
        {
            name = "Hacked Recipe",
            description = "Bad",
            recipeType = "Main",
            ingredients = new[] { "item" },
            instructions = "Do it",
            pairings = Array.Empty<object>()
        });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task User_CannotDeleteRecipe()
    {
        var adminToken = await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        TestClient.Authenticate(_client, adminToken);
        var recipeId = await CreateRecipeAsAdmin();

        var userToken = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, userToken);

        var response = await _client.DeleteAsync($"/api/recipes/{recipeId}");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_CanCreateRecipe()
    {
        var adminToken = await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        TestClient.Authenticate(_client, adminToken);

        var response = await _client.PostAsJsonAsync("/api/recipes", new
        {
            name = $"Admin Recipe {_counter++}",
            description = "Good",
            recipeType = "Starter",
            ingredients = new[] { "item" },
            instructions = "Do it",
            pairings = Array.Empty<object>()
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Admin_CanDeleteRecipe()
    {
        var adminToken = await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        TestClient.Authenticate(_client, adminToken);
        var recipeId = await CreateRecipeAsAdmin();

        var response = await _client.DeleteAsync($"/api/recipes/{recipeId}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task User_CanReadWinesAndRecipes()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);

        var winesResponse = await _client.GetAsync("/api/wines");
        Assert.Equal(HttpStatusCode.OK, winesResponse.StatusCode);

        var recipesResponse = await _client.GetAsync("/api/recipes");
        Assert.Equal(HttpStatusCode.OK, recipesResponse.StatusCode);
    }

    // ---------------------------------------------------------------
    // Pairings — embedded in recipe create/update, same protection
    // ---------------------------------------------------------------

    [Fact]
    public async Task Admin_CanCreateRecipeWithPairings()
    {
        var adminToken = await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        TestClient.Authenticate(_client, adminToken);
        var wineId = await CreateWineAsAdmin();

        var response = await _client.PostAsJsonAsync("/api/recipes", new
        {
            name = $"Paired Recipe {_counter++}",
            description = "With pairing",
            recipeType = "Main",
            ingredients = new[] { "item" },
            instructions = "Do it",
            pairings = new[] { new { wineId, notes = "Great match" } }
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<RecipeBody>();
        Assert.Single(body!.Pairings);
    }

    [Fact]
    public async Task User_CannotCreateRecipeWithPairings()
    {
        var adminToken = await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        TestClient.Authenticate(_client, adminToken);
        var wineId = await CreateWineAsAdmin();

        var userToken = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, userToken);

        var response = await _client.PostAsJsonAsync("/api/recipes", new
        {
            name = "Sneaky Pairing",
            description = "Bad",
            recipeType = "Main",
            ingredients = new[] { "item" },
            instructions = "Do it",
            pairings = new[] { new { wineId, notes = "Should fail" } }
        });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private record IdResult(int Id);
    private record AuthBody(string Token, int UserId, string Email, string Role);
    private record PairingResult(int WineId);
    private record RecipeBody(int Id, List<PairingResult> Pairings);
}
