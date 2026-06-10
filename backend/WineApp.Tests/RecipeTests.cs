using System.Net;
using System.Net.Http.Json;
using WineApp.Tests.Helpers;

namespace WineApp.Tests;

public class RecipeTests(TestWebFactory factory) : IClassFixture<TestWebFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static readonly object ValidRecipe = new
    {
        name = "Test Recipe",
        description = "A delicious test recipe",
        ingredients = new[] { "ingredient 1", "ingredient 2", "ingredient 3" },
        instructions = "Mix everything together.",
        recipeType = "Main",
        pairings = Array.Empty<object>()
    };

    [Fact]
    public async Task GetRecipes_Unauthenticated_Returns401()
    {
        var response = await _client.GetAsync("/api/recipes");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetRecipes_Authenticated_Returns200()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);

        var response = await _client.GetAsync("/api/recipes");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateRecipe_ValidData_Returns201()
    {
        var token = await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        TestClient.Authenticate(_client, token);

        var response = await _client.PostAsJsonAsync("/api/recipes", ValidRecipe);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(body);
        Assert.Equal("Test Recipe", body.Name);
        Assert.Equal(3, body.Ingredients.Length);
        Assert.Equal("Main", body.RecipeType);
    }

    [Fact]
    public async Task CreateRecipe_MissingName_Returns400()
    {
        var token = await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        TestClient.Authenticate(_client, token);

        var response = await _client.PostAsJsonAsync("/api/recipes", new
        {
            description = "A recipe without a name",
            ingredients = new[] { "ingredient 1" },
            instructions = "Do stuff.",
            recipeType = "Main",
            pairings = Array.Empty<object>()
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetRecipeById_ExistingRecipe_Returns200WithIngredients()
    {
        var token = await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        TestClient.Authenticate(_client, token);

        var created = await _client.PostAsJsonAsync("/api/recipes", ValidRecipe);
        var recipe = await created.Content.ReadFromJsonAsync<RecipeResponse>();

        var response = await _client.GetAsync($"/api/recipes/{recipe!.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var fetched = await response.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.Equal(3, fetched!.Ingredients.Length);
    }

    [Fact]
    public async Task GetRecipeById_NotFound_Returns404()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);

        var response = await _client.GetAsync("/api/recipes/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateRecipe_WithWinePairing_Returns201WithPairing()
    {
        var token = await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        TestClient.Authenticate(_client, token);

        // Create a wine first (users can create wines, admin can too)
        var wineResponse = await _client.PostAsJsonAsync("/api/wines", new
        {
            name = "Pairing Wine",
            domain = "Pairing Domain",
            year = 2020,
            rank = 3,
            cepages = Array.Empty<object>()
        });
        var wine = await wineResponse.Content.ReadFromJsonAsync<WineRef>();

        var response = await _client.PostAsJsonAsync("/api/recipes", new
        {
            name = "Paired Recipe",
            description = "Goes well with wine",
            ingredients = new[] { "pasta", "sauce" },
            instructions = "Cook it.",
            recipeType = "Main",
            pairings = new[] { new { wineId = wine!.Id, notes = "Perfect match" } }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.Single(body!.Pairings);
        Assert.Equal("Perfect match", body.Pairings[0].Notes);
    }

    [Fact]
    public async Task UpdateRecipe_ValidData_Returns200()
    {
        var token = await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        TestClient.Authenticate(_client, token);

        var created = await _client.PostAsJsonAsync("/api/recipes", ValidRecipe);
        var recipe = await created.Content.ReadFromJsonAsync<RecipeResponse>();

        var response = await _client.PutAsJsonAsync($"/api/recipes/{recipe!.Id}", new
        {
            name = "Updated Recipe",
            description = "Updated description",
            ingredients = new[] { "new ingredient" },
            instructions = "New instructions.",
            recipeType = "Starter",
            pairings = Array.Empty<object>()
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.Equal("Updated Recipe", updated!.Name);
        Assert.Equal("Starter", updated.RecipeType);
        Assert.Single(updated.Ingredients);
    }

    [Fact]
    public async Task DeleteRecipe_ExistingRecipe_Returns204()
    {
        var token = await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        TestClient.Authenticate(_client, token);

        var created = await _client.PostAsJsonAsync("/api/recipes", ValidRecipe);
        var recipe = await created.Content.ReadFromJsonAsync<RecipeResponse>();

        var response = await _client.DeleteAsync($"/api/recipes/{recipe!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task SearchRecipes_ByName_ReturnsMatchingRecipes()
    {
        var token = await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        TestClient.Authenticate(_client, token);

        await _client.PostAsJsonAsync("/api/recipes", new
        {
            name = "Unique Searchable Recipe",
            description = "Distinctive description",
            ingredients = new[] { "ingredient" },
            instructions = "Do it.",
            recipeType = "Dessert",
            pairings = Array.Empty<object>()
        });

        var response = await _client.GetAsync("/api/recipes?search=Unique+Searchable");
        var recipes = await response.Content.ReadFromJsonAsync<List<RecipeResponse>>();
        Assert.Contains(recipes!, r => r.Name == "Unique Searchable Recipe");
    }

    private record RecipeResponse(int Id, string Name, string Description, string[] Ingredients, string RecipeType, PairingResponse[] Pairings);
    private record PairingResponse(int WineId, string? Notes);
    private record WineRef(int Id);
}
