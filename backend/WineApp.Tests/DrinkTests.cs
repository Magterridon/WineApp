using System.Net;
using System.Net.Http.Json;
using WineApp.Tests.Helpers;

namespace WineApp.Tests;

public class DrinkTests(TestWebFactory factory) : IClassFixture<TestWebFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private string? _adminToken;

    private static int _counter;

    private async Task<int> CreateWineAsync()
    {
        var n = System.Threading.Interlocked.Increment(ref _counter);
        var response = await _client.PostAsJsonAsync("/api/wines", new
        {
            name = $"Drink Wine {n}",
            domain = $"Drink Domain {n}",
            year = 2015 + (n % 5),
            rank = 3,
            cepages = Array.Empty<object>()
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<WineRef>())!.Id;
    }

    private async Task<int> CreateRecipeAsync()
    {
        // Recipe creation requires Admin role
        _adminToken ??= await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        var savedAuth = _client.DefaultRequestHeaders.Authorization;
        TestClient.Authenticate(_client, _adminToken);

        var n = System.Threading.Interlocked.Increment(ref _counter);
        var response = await _client.PostAsJsonAsync("/api/recipes", new
        {
            name = $"Test Recipe {n}",
            description = "A test recipe",
            recipeType = "Main",
            ingredients = new[] { "ingredient" },
            instructions = "Do it",
            pairings = Array.Empty<object>()
        });
        response.EnsureSuccessStatusCode();
        var id = (await response.Content.ReadFromJsonAsync<RecipeRef>())!.Id;

        // Restore the caller's auth header
        _client.DefaultRequestHeaders.Authorization = savedAuth;
        return id;
    }

    private async Task AddToCellarAsync(int wineId)
    {
        var r = await _client.PostAsJsonAsync("/api/cellar/items", new { wineId });
        r.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task DrinkBottle_NotInCellar_Returns400()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);
        var wineId = await CreateWineAsync();

        var response = await _client.PostAsJsonAsync("/api/cellar/drink", new { wineId });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DrinkBottle_WineDoesNotExist_Returns404()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);

        var response = await _client.PostAsJsonAsync("/api/cellar/drink", new { wineId = 99999 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DrinkBottle_WithTastingFields_ReturnsRecord()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);
        var wineId = await CreateWineAsync();

        await AddToCellarAsync(wineId);
        await AddToCellarAsync(wineId); // count = 2

        var response = await _client.PostAsJsonAsync("/api/cellar/drink", new
        {
            wineId,
            rating = 4,
            tastingNote = "Very smooth"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<DrinkResponse>();
        Assert.NotNull(body?.DrinkRecord);
        Assert.Equal(wineId, body.DrinkRecord.WineId);
        Assert.Equal(4, body.DrinkRecord.Rating);
        Assert.Equal("Very smooth", body.DrinkRecord.TastingNote);
        Assert.False(body.DrinkRecord.PairedWithMeal);
        Assert.NotNull(body.CellarItem);
        Assert.Equal(1, body.CellarItem.BottleCount);
    }

    [Fact]
    public async Task DrinkBottle_WithCustomMeal_SetsPairedWithMeal()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);
        var wineId = await CreateWineAsync();
        await AddToCellarAsync(wineId);

        var response = await _client.PostAsJsonAsync("/api/cellar/drink", new
        {
            wineId,
            mealNote = "Beef stew",
            pairingRating = 5,
            pairingNote = "Perfect match"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<DrinkResponse>();
        Assert.True(body!.DrinkRecord.PairedWithMeal);
        Assert.Equal("Beef stew", body.DrinkRecord.MealNote);
        Assert.Equal(5, body.DrinkRecord.PairingRating);
        Assert.Equal("Perfect match", body.DrinkRecord.PairingNote);
        Assert.Null(body.DrinkRecord.RecipeId);
    }

    [Fact]
    public async Task DrinkBottle_WithRecipe_SetsRecipeAndPairedWithMeal()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);
        var wineId = await CreateWineAsync();
        var recipeId = await CreateRecipeAsync();
        await AddToCellarAsync(wineId);

        var response = await _client.PostAsJsonAsync("/api/cellar/drink", new
        {
            wineId,
            recipeId,
            pairingRating = 4,
            pairingNote = "Great combo"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<DrinkResponse>();
        Assert.True(body!.DrinkRecord.PairedWithMeal);
        Assert.Equal(recipeId, body.DrinkRecord.RecipeId);
        Assert.NotNull(body.DrinkRecord.RecipeName);
        Assert.Equal(4, body.DrinkRecord.PairingRating);
    }

    [Fact]
    public async Task DrinkBottle_InvalidRecipe_Returns404()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);
        var wineId = await CreateWineAsync();
        await AddToCellarAsync(wineId);

        var response = await _client.PostAsJsonAsync("/api/cellar/drink", new { wineId, recipeId = 99999 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DrinkBottle_LastBottle_RemovesFromCellar()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);
        var wineId = await CreateWineAsync();

        await AddToCellarAsync(wineId);

        var response = await _client.PostAsJsonAsync("/api/cellar/drink", new { wineId });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<DrinkResponse>();
        Assert.Null(body!.CellarItem);

        var cellarResponse = await _client.GetAsync("/api/cellar");
        var cellar = await cellarResponse.Content.ReadFromJsonAsync<CellarResult>();
        Assert.DoesNotContain(cellar!.Items, i => i.WineId == wineId);
    }

    [Fact]
    public async Task DrinkBottle_CreatesHistoryRecord()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);
        var wineId = await CreateWineAsync();

        await AddToCellarAsync(wineId);
        await _client.PostAsJsonAsync("/api/cellar/drink", new { wineId, rating = 3, tastingNote = "Good" });

        var historyResponse = await _client.GetAsync("/api/cellar/history");
        Assert.Equal(HttpStatusCode.OK, historyResponse.StatusCode);

        var history = await historyResponse.Content.ReadFromJsonAsync<List<DrinkRecordResult>>();
        Assert.Contains(history!, r => r.WineId == wineId && r.Rating == 3);
    }

    [Fact]
    public async Task GetWineHistory_ReturnsOnlyRecordsForThatWine()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);
        var wineId = await CreateWineAsync();
        var otherWineId = await CreateWineAsync();

        await AddToCellarAsync(wineId);
        await AddToCellarAsync(otherWineId);
        await _client.PostAsJsonAsync("/api/cellar/drink", new { wineId, rating = 4 });
        await _client.PostAsJsonAsync("/api/cellar/drink", new { wineId = otherWineId, rating = 2 });

        var response = await _client.GetAsync($"/api/cellar/history/wine/{wineId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var history = await response.Content.ReadFromJsonAsync<List<DrinkRecordResult>>();
        Assert.All(history!, r => Assert.Equal(wineId, r.WineId));
        Assert.Contains(history!, r => r.Rating == 4);
        Assert.DoesNotContain(history!, r => r.WineId == otherWineId);
    }

    [Fact]
    public async Task GetHistory_ReturnsOnlyCurrentUserRecords()
    {
        var token1 = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token1);
        var wineId = await CreateWineAsync();
        await AddToCellarAsync(wineId);
        await _client.PostAsJsonAsync("/api/cellar/drink", new { wineId });

        var token2 = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token2);

        var response = await _client.GetAsync("/api/cellar/history");
        var history = await response.Content.ReadFromJsonAsync<List<DrinkRecordResult>>();
        Assert.DoesNotContain(history!, r => r.WineId == wineId);
    }

    [Fact]
    public async Task DrinkBottle_DefaultsConsumedDateToToday()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);
        var wineId = await CreateWineAsync();
        await AddToCellarAsync(wineId);

        var before = DateTime.UtcNow.AddSeconds(-5);
        var response = await _client.PostAsJsonAsync("/api/cellar/drink", new { wineId });
        var after = DateTime.UtcNow.AddSeconds(5);

        var body = await response.Content.ReadFromJsonAsync<DrinkResponse>();
        Assert.InRange(body!.DrinkRecord.ConsumedAt, before, after);
    }

    [Fact]
    public async Task GetHistory_Unauthenticated_Returns401()
    {
        var response = await _client.GetAsync("/api/cellar/history");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private record WineRef(int Id);
    private record RecipeRef(int Id);
    private record DrinkResponse(DrinkRecordResult DrinkRecord, CellarItemResult? CellarItem);
    private record DrinkRecordResult(int Id, int WineId, int? Rating, string? TastingNote, bool PairedWithMeal, string? MealNote, int? RecipeId, string? RecipeName, int? PairingRating, string? PairingNote, DateTime ConsumedAt);
    private record CellarItemResult(int WineId, int BottleCount);
    private record CellarResult(int Id, List<CellarItemResult> Items);
}
