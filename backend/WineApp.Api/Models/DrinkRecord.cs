namespace WineApp.Api.Models;

public class DrinkRecord
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int WineId { get; set; }
    public DateTime ConsumedAt { get; set; }

    // Tasting
    public int? Rating { get; set; }
    public string? TastingNote { get; set; }

    // Pairing — derived: true if RecipeId set OR MealNote non-empty
    public bool PairedWithMeal { get; set; }
    public int? RecipeId { get; set; }
    public string? MealNote { get; set; }
    public int? PairingRating { get; set; }
    public string? PairingNote { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Wine Wine { get; set; } = null!;
    public Recipe? Recipe { get; set; }
}
