namespace WineApp.Api.Models;

public class Wine
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Rank { get; set; }
    public string? Color { get; set; }       // "Red", "White", "Rosé", "Sparkling", "Fortified", "Orange"
    public string? Country { get; set; }
    public string? Region { get; set; }
    public string? Appellation { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int? DrinkFromYear { get; set; }
    public int? DrinkToYear { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<WineCepage> Cepages { get; set; } = new();
    public List<RecipeWinePairing> RecipePairings { get; set; } = new();
    public List<CellarItem> CellarItems { get; set; } = new();
    public List<DrinkRecord> DrinkRecords { get; set; } = new();
}
