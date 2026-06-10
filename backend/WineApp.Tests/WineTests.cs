using System.Net;
using System.Net.Http.Json;
using WineApp.Tests.Helpers;

namespace WineApp.Tests;

public class WineTests(TestWebFactory factory) : IClassFixture<TestWebFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static int _wineCounter;

    private static object UniqueWine(string? suffix = null)
    {
        var n = System.Threading.Interlocked.Increment(ref _wineCounter);
        return new
        {
            name = $"Test Wine {n}{suffix}",
            domain = $"Domain {n}",
            year = 2000 + (n % 20),
            rank = 3,
            cepages = Array.Empty<object>()
        };
    }

    private async Task<WineResponse> CreateWineAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/wines", UniqueWine());
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<WineResponse>())!;
    }

    [Fact]
    public async Task GetWines_Unauthenticated_Returns401()
    {
        var response = await _client.GetAsync("/api/wines");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetWines_Authenticated_Returns200()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);

        var response = await _client.GetAsync("/api/wines");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateWine_ValidData_Returns201()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);

        var wine = UniqueWine();
        var response = await _client.PostAsJsonAsync("/api/wines", wine);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<WineResponse>();
        Assert.NotNull(body);
        Assert.True(body.Id > 0);
        Assert.Equal(3, body.Rank);
    }

    [Fact]
    public async Task CreateWine_WithCepages_ReturnsCepages()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);

        var n = System.Threading.Interlocked.Increment(ref _wineCounter);
        var response = await _client.PostAsJsonAsync("/api/wines", new
        {
            name = $"Cepage Wine {n}",
            domain = $"Domain {n}",
            year = 2020,
            rank = 4,
            cepages = new[] { new { cepageName = "Merlot", percentage = 80 }, new { cepageName = "Cabernet", percentage = 20 } }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<WineResponse>();
        Assert.Equal(2, body!.Cepages.Count);
    }

    [Fact]
    public async Task CreateWine_DuplicateNameDomainYear_Returns409()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);

        var wine = UniqueWine();
        await _client.PostAsJsonAsync("/api/wines", wine);
        var response = await _client.PostAsJsonAsync("/api/wines", wine);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateWine_MissingName_Returns400()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);

        var response = await _client.PostAsJsonAsync("/api/wines", new { domain = "Test Domain", year = 2020 });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetWineById_ExistingWine_Returns200()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);

        var wine = await CreateWineAsync();

        var response = await _client.GetAsync($"/api/wines/{wine.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetWineById_NotFound_Returns404()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);

        var response = await _client.GetAsync("/api/wines/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateWine_ValidData_Returns200()
    {
        var token = await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        TestClient.Authenticate(_client, token);

        var wine = await CreateWineAsync();

        var response = await _client.PutAsJsonAsync($"/api/wines/{wine.Id}", new
        {
            name = "Updated Wine",
            domain = "Updated Domain",
            year = 2021,
            rank = 5,
            cepages = Array.Empty<object>()
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<WineResponse>();
        Assert.Equal("Updated Wine", updated!.Name);
        Assert.Equal(2021, updated.Year);
    }

    [Fact]
    public async Task DeleteWine_ExistingWine_Returns204()
    {
        var token = await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        TestClient.Authenticate(_client, token);

        var wine = await CreateWineAsync();

        var response = await _client.DeleteAsync($"/api/wines/{wine.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task SearchWines_ByName_ReturnsMatchingWines()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);

        var n = System.Threading.Interlocked.Increment(ref _wineCounter);
        var uniqueName = $"UniqueSearchableWine{n}";

        await _client.PostAsJsonAsync("/api/wines", new
        {
            name = uniqueName,
            domain = "Search Domain",
            year = 2019,
            rank = 3,
            cepages = Array.Empty<object>()
        });

        var response = await _client.GetAsync($"/api/wines?search={uniqueName}");
        var wines = await response.Content.ReadFromJsonAsync<List<WineResponse>>();
        Assert.Contains(wines!, w => w.Name == uniqueName);
    }

    [Fact]
    public async Task GetWines_FilterByColor_ReturnsMatchingWines()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);

        var n = System.Threading.Interlocked.Increment(ref _wineCounter);
        await _client.PostAsJsonAsync("/api/wines", new
        {
            name = $"Rosé Wine {n}", domain = "Pink Domain", year = 2022, rank = 3,
            color = "Rosé", cepages = Array.Empty<object>()
        });

        var response = await _client.GetAsync("/api/wines?color=Rosé");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var wines = await response.Content.ReadFromJsonAsync<List<WineResponse>>();
        Assert.All(wines!, w => Assert.Equal("Rosé", w.Color));
    }

    [Fact]
    public async Task GetWines_FilterByDrinkStatus_OnlyReturnsMatchingWines()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);

        var n = System.Threading.Interlocked.Increment(ref _wineCounter);
        var currentYear = DateTime.UtcNow.Year;
        // Wine that is past peak
        await _client.PostAsJsonAsync("/api/wines", new
        {
            name = $"OldWine {n}", domain = "Old Domain", year = 2000, rank = 3,
            drinkFromYear = 2005, drinkToYear = 2010,
            cepages = Array.Empty<object>()
        });

        var response = await _client.GetAsync("/api/wines?drinkStatus=past");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var wines = await response.Content.ReadFromJsonAsync<List<WineResponse>>();
        // All returned wines must have drinkToYear < currentYear
        Assert.All(wines!, w => Assert.True(w.DrinkToYear.HasValue && w.DrinkToYear < currentYear));
    }

    [Fact]
    public async Task GetSimilarWines_ByName_ReturnsSuggestions()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);

        var n = System.Threading.Interlocked.Increment(ref _wineCounter);
        var uniquePart = $"UniqueSimilar{n}";
        await _client.PostAsJsonAsync("/api/wines", new
        {
            name = $"{uniquePart} Rouge", domain = "Domain", year = 2020, rank = 3,
            cepages = Array.Empty<object>()
        });
        await _client.PostAsJsonAsync("/api/wines", new
        {
            name = $"{uniquePart} Blanc", domain = "Domain", year = 2021, rank = 3,
            cepages = Array.Empty<object>()
        });

        var response = await _client.GetAsync($"/api/wines/similar?name={uniquePart}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var similar = await response.Content.ReadFromJsonAsync<List<WineResponse>>();
        Assert.True(similar!.Count >= 2);
        Assert.All(similar!, w => Assert.Contains(uniquePart, w.Name));
    }

    [Fact]
    public async Task CreateWine_WithMetadata_ReturnAllFields()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);

        var n = System.Threading.Interlocked.Increment(ref _wineCounter);
        var response = await _client.PostAsJsonAsync("/api/wines", new
        {
            name = $"Meta Wine {n}", domain = "Meta Domain", year = 2020, rank = 4,
            color = "White", country = "France", region = "Bourgogne", appellation = "Meursault",
            cepages = Array.Empty<object>()
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var wine = await response.Content.ReadFromJsonAsync<WineResponse>();
        Assert.Equal("White", wine!.Color);
        Assert.Equal("France", wine.Country);
        Assert.Equal("Bourgogne", wine.Region);
        Assert.Equal("Meursault", wine.Appellation);
    }

    [Fact]
    public async Task GetWines_FilterByName_ReturnsOnlyMatchingWines()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);

        var n = System.Threading.Interlocked.Increment(ref _wineCounter);
        var uniqueName = $"NameOnly{n}Château";
        await _client.PostAsJsonAsync("/api/wines", new
        {
            name = uniqueName,
            domain = "SomeDomainNotMatchingName",
            year = 2020, rank = 3,
            cepages = Array.Empty<object>()
        });

        var response = await _client.GetAsync($"/api/wines?name={uniqueName}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var wines = await response.Content.ReadFromJsonAsync<List<WineResponse>>();
        Assert.NotEmpty(wines!);
        Assert.All(wines!, w => Assert.Contains(uniqueName, w.Name, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetWines_FilterByDomain_ReturnsOnlyMatchingWines()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);

        var n = System.Threading.Interlocked.Increment(ref _wineCounter);
        var uniqueDomain = $"DomainOnly{n}Estates";
        await _client.PostAsJsonAsync("/api/wines", new
        {
            name = $"Wine {n}", domain = uniqueDomain, year = 2020, rank = 3,
            cepages = Array.Empty<object>()
        });

        var response = await _client.GetAsync($"/api/wines?domain={uniqueDomain}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var wines = await response.Content.ReadFromJsonAsync<List<WineResponse>>();
        Assert.NotEmpty(wines!);
        Assert.All(wines!, w => Assert.Contains(uniqueDomain, w.Domain, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetWines_FilterByMultipleColors_ReturnsOnlySelectedColors()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);

        var n = System.Threading.Interlocked.Increment(ref _wineCounter);
        await _client.PostAsJsonAsync("/api/wines", new
        {
            name = $"Red Wine {n}", domain = "Domain", year = 2020, rank = 3,
            color = "Red", cepages = Array.Empty<object>()
        });
        await _client.PostAsJsonAsync("/api/wines", new
        {
            name = $"White Wine {n}", domain = "Domain", year = 2021, rank = 3,
            color = "White", cepages = Array.Empty<object>()
        });

        var response = await _client.GetAsync("/api/wines?colors=Red&colors=White");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var wines = await response.Content.ReadFromJsonAsync<List<WineResponse>>();
        Assert.NotEmpty(wines!);
        Assert.All(wines!, w => Assert.True(w.Color == "Red" || w.Color == "White"));
    }

    [Fact]
    public async Task GetWines_FilterByMultipleCepages_ReturnsOnlyMatchingWines()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);

        var n = System.Threading.Interlocked.Increment(ref _wineCounter);
        var cepage1 = $"Merlot{n}";
        var cepage2 = $"Syrah{n}";
        await _client.PostAsJsonAsync("/api/wines", new
        {
            name = $"Merlot Wine {n}", domain = "Domain", year = 2020, rank = 3,
            cepages = new[] { new { cepageName = cepage1, percentage = 100 } }
        });
        await _client.PostAsJsonAsync("/api/wines", new
        {
            name = $"Syrah Wine {n}", domain = "Domain", year = 2021, rank = 3,
            cepages = new[] { new { cepageName = cepage2, percentage = 100 } }
        });

        var response = await _client.GetAsync($"/api/wines?cepages={cepage1}&cepages={cepage2}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var wines = await response.Content.ReadFromJsonAsync<List<WineResponse>>();
        Assert.NotEmpty(wines!);
        Assert.All(wines!, w =>
        {
            bool hasCepage1 = w.Cepages.Any(c => c.CepageName.Equals(cepage1, StringComparison.OrdinalIgnoreCase));
            bool hasCepage2 = w.Cepages.Any(c => c.CepageName.Equals(cepage2, StringComparison.OrdinalIgnoreCase));
            Assert.True(hasCepage1 || hasCepage2, $"Wine '{w.Name}' does not have cepage {cepage1} or {cepage2}");
        });
    }

    [Fact]
    public async Task GetWines_FilterByRecipeId_ReturnsOnlyPairedWines()
    {
        var token = await TestClient.RegisterAdminAndGetTokenAsync(_client, factory);
        TestClient.Authenticate(_client, token);

        // Create a wine and a recipe, then pair them
        var n = System.Threading.Interlocked.Increment(ref _wineCounter);
        var wineResp = await _client.PostAsJsonAsync("/api/wines", new
        {
            name = $"Paired Wine {n}", domain = "Domain", year = 2020, rank = 3,
            cepages = Array.Empty<object>()
        });
        var wine = await wineResp.Content.ReadFromJsonAsync<WineResponse>();

        var recipeResp = await _client.PostAsJsonAsync("/api/recipes", new
        {
            name = $"Test Recipe {n}", description = "desc",
            ingredients = new[] { "x" }, instructions = "do it.",
            recipeType = "Main",
            pairings = new[] { new { wineId = wine!.Id, notes = "great" } }
        });
        var recipe = await recipeResp.Content.ReadFromJsonAsync<RecipeRef>();

        var response = await _client.GetAsync($"/api/wines?recipeId={recipe!.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var wines = await response.Content.ReadFromJsonAsync<List<WineResponse>>();
        Assert.Contains(wines!, w => w.Id == wine.Id);
    }

    [Fact]
    public async Task GetWines_FilterByAppellation_ReturnsOnlyMatchingWines()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);

        var n = System.Threading.Interlocked.Increment(ref _wineCounter);
        var uniqueApp = $"Appellation{n}Unique";
        await _client.PostAsJsonAsync("/api/wines", new
        {
            name = $"App Wine {n}", domain = "App Domain", year = 2020, rank = 3,
            appellation = uniqueApp, cepages = Array.Empty<object>()
        });

        var response = await _client.GetAsync($"/api/wines?appellation={uniqueApp}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var wines = await response.Content.ReadFromJsonAsync<List<WineResponse>>();
        Assert.NotEmpty(wines!);
        Assert.All(wines!, w => Assert.Contains(uniqueApp, w.Appellation, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetWines_FilterByCepage_ReturnsOnlyMatchingWines()
    {
        var token = await TestClient.RegisterAndGetTokenAsync(_client);
        TestClient.Authenticate(_client, token);

        var n = System.Threading.Interlocked.Increment(ref _wineCounter);
        var uniqueCepage = $"Varietal{n}Unique";
        await _client.PostAsJsonAsync("/api/wines", new
        {
            name = $"Cepage Wine {n}", domain = "Cepage Domain", year = 2021, rank = 3,
            cepages = new[] { new { cepageName = uniqueCepage, percentage = 100 } }
        });

        var response = await _client.GetAsync($"/api/wines?cepage={uniqueCepage}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var wines = await response.Content.ReadFromJsonAsync<List<WineResponse>>();
        Assert.NotEmpty(wines!);
        Assert.All(wines!, w => Assert.Contains(w.Cepages,
            c => c.CepageName.Contains(uniqueCepage, StringComparison.OrdinalIgnoreCase)));
    }

    private record WineResponse(int Id, string Name, string Domain, int Year, int Rank,
        string? Color, string? Country, string? Region, string? Appellation,
        int? DrinkFromYear, int? DrinkToYear,
        List<CepageResponse> Cepages);
    private record CepageResponse(string CepageName, int? Percentage);
    private record RecipeRef(int Id);
}
