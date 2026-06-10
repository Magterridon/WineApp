using System.Net;
using System.Net.Http.Json;
using WineApp.Tests.Helpers;

namespace WineApp.Tests;

/// <summary>
/// Integration tests for the admin wine backoffice endpoints.
/// </summary>
public class AdminTests(TestWebFactory factory) : IClassFixture<TestWebFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static int _counter;

    private async Task<int> CreateWine(string? name = null, string? domain = null)
    {
        var n = System.Threading.Interlocked.Increment(ref _counter);
        var response = await _client.PostAsJsonAsync("/api/wines", new
        {
            name   = name   ?? $"Admin Wine {n}",
            domain = domain ?? $"Admin Domain {n}",
            year   = 2015,
            rank   = 3,
            cepages = Array.Empty<object>()
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IdResult>())!.Id;
    }

    private async Task<int> CreateRecipe()
    {
        var n = System.Threading.Interlocked.Increment(ref _counter);
        var response = await _client.PostAsJsonAsync("/api/recipes", new
        {
            name         = $"Admin Recipe {n}",
            description  = "Test",
            recipeType   = "Main",
            ingredients  = new[] { "item" },
            instructions = "Do it",
            pairings     = Array.Empty<object>()
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IdResult>())!.Id;
    }

    // ── Access control ────────────────────────────────────────────────────────

    [Fact]
    public async Task AdminWines_Unauthenticated_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/admin/wines");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminWines_RegularUser_Returns403()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);
        var response = await _client.GetAsync("/api/admin/wines");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminWines_Admin_Returns200()
    {
        var adminToken = await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        TestClient.Authenticate(_client, adminToken);
        var response = await _client.GetAsync("/api/admin/wines");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── List and filtering ────────────────────────────────────────────────────

    [Fact]
    public async Task AdminWines_ReturnsPaginatedResult()
    {
        var adminToken = await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        TestClient.Authenticate(_client, adminToken);

        // Create some wines first
        await CreateWine("Paginated Wine A");
        await CreateWine("Paginated Wine B");

        var response = await _client.GetAsync("/api/admin/wines?pageSize=10&page=1");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResult>();
        Assert.NotNull(result);
        Assert.True(result!.Total >= 2);
        Assert.True(result.Items.Count > 0);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task AdminWines_FilterBySearch()
    {
        var adminToken = await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        TestClient.Authenticate(_client, adminToken);
        await CreateWine("Searchable Chateau XYZ");

        var response = await _client.GetAsync("/api/admin/wines?search=Searchable+Chateau+XYZ");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResult>();
        Assert.Contains(result!.Items, w => w.Name.Contains("Searchable"));
    }

    [Fact]
    public async Task AdminWines_FilterByHasImage_MissingOnly()
    {
        var adminToken = await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        TestClient.Authenticate(_client, adminToken);

        // Create wine without image
        await CreateWine($"No Image Wine {_counter}");

        var response = await _client.GetAsync("/api/admin/wines?hasImage=false&pageSize=50");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResult>();
        Assert.True(result!.Items.All(w => !w.HasImage));
    }

    [Fact]
    public async Task AdminWines_FilterByHasPairing_MissingOnly()
    {
        var adminToken = await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        TestClient.Authenticate(_client, adminToken);

        await CreateWine($"Unpaired Wine {_counter}");

        var response = await _client.GetAsync("/api/admin/wines?hasPairing=false&pageSize=50");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResult>();
        Assert.True(result!.Items.All(w => !w.HasPairing));
    }

    // ── Bulk image ────────────────────────────────────────────────────────────

    [Fact]
    public async Task BulkImage_User_Returns403()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);

        var response = await _client.PostAsJsonAsync("/api/admin/wines/bulk-image", new
        {
            wineIds  = new[] { 1 },
            imageUrl = "https://example.com/image.jpg"
        });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task BulkImage_Admin_UpdatesSelectedWines()
    {
        var adminToken = await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        TestClient.Authenticate(_client, adminToken);

        var wineId1 = await CreateWine();
        var wineId2 = await CreateWine();
        var otherWineId = await CreateWine();

        var imageUrl = "https://example.com/wine-image.jpg";
        var response = await _client.PostAsJsonAsync("/api/admin/wines/bulk-image", new
        {
            wineIds  = new[] { wineId1, wineId2 },
            imageUrl
        });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<BulkUpdateResult>();
        Assert.Equal(2, body!.Updated);

        // Verify wines were updated
        var w1 = await _client.GetFromJsonAsync<WineResult>($"/api/wines/{wineId1}");
        Assert.Equal(imageUrl, w1!.ImageUrl);

        // Verify non-selected wine was NOT updated
        var w3 = await _client.GetFromJsonAsync<WineResult>($"/api/wines/{otherWineId}");
        Assert.Null(w3!.ImageUrl);
    }

    [Fact]
    public async Task BulkImage_EmptyWineIds_ReturnsBadRequest()
    {
        var adminToken = await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        TestClient.Authenticate(_client, adminToken);

        var response = await _client.PostAsJsonAsync("/api/admin/wines/bulk-image", new
        {
            wineIds  = Array.Empty<int>(),
            imageUrl = "https://example.com/image.jpg"
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── Bulk pairings ────────────────────────────────────────────────────────

    [Fact]
    public async Task BulkPairings_User_Returns403()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);

        var response = await _client.PostAsJsonAsync("/api/admin/wines/bulk-pairings", new
        {
            wineIds   = new[] { 1 },
            recipeIds = new[] { 1 }
        });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task BulkPairings_Admin_CreatesPairings()
    {
        var adminToken = await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        TestClient.Authenticate(_client, adminToken);

        var wineId1   = await CreateWine();
        var wineId2   = await CreateWine();
        var recipeId1 = await CreateRecipe();
        var recipeId2 = await CreateRecipe();

        var response = await _client.PostAsJsonAsync("/api/admin/wines/bulk-pairings", new
        {
            wineIds   = new[] { wineId1, wineId2 },
            recipeIds = new[] { recipeId1, recipeId2 }
        });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<PairingResult>();
        // 2 wines × 2 recipes = 4 pairings created
        Assert.Equal(4, body!.Created);
        Assert.Equal(0, body.Skipped);
    }

    [Fact]
    public async Task BulkPairings_Idempotent_SkipsDuplicates()
    {
        var adminToken = await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        TestClient.Authenticate(_client, adminToken);

        var wineId   = await CreateWine();
        var recipeId = await CreateRecipe();

        // First assignment
        var r1 = await _client.PostAsJsonAsync("/api/admin/wines/bulk-pairings", new
        {
            wineIds   = new[] { wineId },
            recipeIds = new[] { recipeId }
        });
        r1.EnsureSuccessStatusCode();
        var b1 = await r1.Content.ReadFromJsonAsync<PairingResult>();
        Assert.Equal(1, b1!.Created);

        // Second assignment — same pair, should be skipped
        var r2 = await _client.PostAsJsonAsync("/api/admin/wines/bulk-pairings", new
        {
            wineIds   = new[] { wineId },
            recipeIds = new[] { recipeId }
        });
        r2.EnsureSuccessStatusCode();
        var b2 = await r2.Content.ReadFromJsonAsync<PairingResult>();
        Assert.Equal(0, b2!.Created);
        Assert.Equal(1, b2.Skipped);
    }

    [Fact]
    public async Task BulkPairings_IncorrectWineId_SilentlyIgnored()
    {
        var adminToken = await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        TestClient.Authenticate(_client, adminToken);

        var wineId   = await CreateWine();
        var recipeId = await CreateRecipe();

        var response = await _client.PostAsJsonAsync("/api/admin/wines/bulk-pairings", new
        {
            wineIds   = new[] { wineId, 999999 },  // 999999 doesn't exist
            recipeIds = new[] { recipeId }
        });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<PairingResult>();
        Assert.Equal(1, body!.Created); // Only valid wine got a pairing
    }

    // ── Upload image endpoint ─────────────────────────────────────────────────

    [Fact]
    public async Task UploadImage_User_Returns403()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);

        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent([0xFF, 0xD8, 0xFF]), "file", "test.jpg");
        var response = await _client.PostAsync("/api/admin/wines/upload-image", content);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UploadImage_Admin_InvalidExtension_Returns400()
    {
        var adminToken = await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        TestClient.Authenticate(_client, adminToken);

        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent([0x00]), "file", "malware.exe");
        var response = await _client.PostAsync("/api/admin/wines/upload-image", content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── Private DTOs for test deserialization ─────────────────────────────────

    private record IdResult(int Id);

    private record PagedResult(
        List<AdminWineItem> Items,
        int Total,
        int Page,
        int PageSize);

    private record AdminWineItem(
        int Id,
        string Name,
        string? ImageUrl,
        bool HasImage,
        bool HasPairing,
        int PairingCount);

    private record WineResult(int Id, string? ImageUrl);
    private record BulkUpdateResult(int Updated, string Message);
    private record PairingResult(int Created, int Skipped);
}
