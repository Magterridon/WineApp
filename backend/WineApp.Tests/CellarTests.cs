using System.Net;
using System.Net.Http.Json;
using WineApp.Tests.Helpers;

namespace WineApp.Tests;

public class CellarTests(TestWebFactory factory) : IClassFixture<TestWebFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<int> CreateWineAsync(string suffix = "")
    {
        var response = await _client.PostAsJsonAsync("/api/wines", new
        {
            name = $"Cellar Wine{suffix}",
            domain = $"Cellar Domain{suffix}",
            year = 2020,
            rank = 3,
            cepages = Array.Empty<object>()
        });
        response.EnsureSuccessStatusCode();
        var wine = await response.Content.ReadFromJsonAsync<WineRef>();
        return wine!.Id;
    }

    [Fact]
    public async Task GetCellar_Unauthenticated_Returns401()
    {
        var response = await _client.GetAsync("/api/cellar");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCellar_NewUser_ReturnsEmptyItems()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);

        var response = await _client.GetAsync("/api/cellar");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CellarResponse>();
        Assert.NotNull(body);
        Assert.Empty(body.Items);
    }

    [Fact]
    public async Task AddItem_ValidWine_Returns200AndIncreasesCount()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);
        var wineId = await CreateWineAsync(" A");

        var response = await _client.PostAsJsonAsync("/api/cellar/items", new { wineId });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var item = await response.Content.ReadFromJsonAsync<CellarItemResponse>();
        Assert.Equal(1, item!.BottleCount);
        Assert.Equal(wineId, item.WineId);
    }

    [Fact]
    public async Task AddItem_Twice_IncrementsToTwo()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);
        var wineId = await CreateWineAsync(" B");

        await _client.PostAsJsonAsync("/api/cellar/items", new { wineId });
        var response = await _client.PostAsJsonAsync("/api/cellar/items", new { wineId });

        var item = await response.Content.ReadFromJsonAsync<CellarItemResponse>();
        Assert.Equal(2, item!.BottleCount);
    }

    [Fact]
    public async Task IncrementItem_Returns200WithIncrementedCount()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);
        var wineId = await CreateWineAsync(" C");

        await _client.PostAsJsonAsync("/api/cellar/items", new { wineId });
        var response = await _client.PatchAsync($"/api/cellar/items/{wineId}/increment", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var item = await response.Content.ReadFromJsonAsync<CellarItemResponse>();
        Assert.Equal(2, item!.BottleCount);
    }

    [Fact]
    public async Task DecrementItem_AboveOne_Returns200WithDecrementedCount()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);
        var wineId = await CreateWineAsync(" D");

        await _client.PostAsJsonAsync("/api/cellar/items", new { wineId });
        await _client.PostAsJsonAsync("/api/cellar/items", new { wineId });

        var response = await _client.PatchAsync($"/api/cellar/items/{wineId}/decrement", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var item = await response.Content.ReadFromJsonAsync<CellarItemResponse>();
        Assert.Equal(1, item!.BottleCount);
    }

    [Fact]
    public async Task DecrementItem_AtOne_Returns204AndRemovesItem()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);
        var wineId = await CreateWineAsync(" E");

        await _client.PostAsJsonAsync("/api/cellar/items", new { wineId });
        var decrementResponse = await _client.PatchAsync($"/api/cellar/items/{wineId}/decrement", null);

        Assert.Equal(HttpStatusCode.NoContent, decrementResponse.StatusCode);

        // Verify item is gone from cellar
        var cellarResponse = await _client.GetAsync("/api/cellar");
        var cellar = await cellarResponse.Content.ReadFromJsonAsync<CellarResponse>();
        Assert.DoesNotContain(cellar!.Items, i => i.WineId == wineId);
    }

    [Fact]
    public async Task RemoveItem_ExistingItem_Returns204()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);
        var wineId = await CreateWineAsync(" F");

        await _client.PostAsJsonAsync("/api/cellar/items", new { wineId });
        var response = await _client.DeleteAsync($"/api/cellar/items/{wineId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Cellar_IsolatedBetweenUsers()
    {
        var token1 = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token1);
        var wineId = await CreateWineAsync(" G");
        await _client.PostAsJsonAsync("/api/cellar/items", new { wineId });

        // Second user should have empty cellar
        var token2 = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token2);
        var response = await _client.GetAsync("/api/cellar");
        var cellar = await response.Content.ReadFromJsonAsync<CellarResponse>();
        Assert.Empty(cellar!.Items);
    }

    private record CellarResponse(int Id, List<CellarItemResponse> Items);
    private record CellarItemResponse(int WineId, int BottleCount);
    private record WineRef(int Id);
}
