using System.ComponentModel.DataAnnotations;

namespace WineApp.Api.DTOs;

public class CreateDrinkRecordRequest
{
    [Required]
    public int WineId { get; set; }

    public DateTime? ConsumedAt { get; set; }

    [Range(1, 5)]
    public int? Rating { get; set; }

    public string? TastingNote { get; set; }

    // Pairing — at least one of RecipeId / MealNote sets PairedWithMeal = true
    public int? RecipeId { get; set; }
    public string? MealNote { get; set; }

    [Range(1, 5)]
    public int? PairingRating { get; set; }

    public string? PairingNote { get; set; }
}

public class DrinkRecordDto
{
    public int Id { get; set; }
    public int WineId { get; set; }
    public string WineName { get; set; } = string.Empty;
    public string WineDomain { get; set; } = string.Empty;
    public int WineYear { get; set; }
    public int? WineDrinkFromYear { get; set; }
    public int? WineDrinkToYear { get; set; }
    public DateTime ConsumedAt { get; set; }

    // Tasting
    public int? Rating { get; set; }
    public string? TastingNote { get; set; }

    // Pairing
    public bool PairedWithMeal { get; set; }
    public int? RecipeId { get; set; }
    public string? RecipeName { get; set; }
    public string? MealNote { get; set; }
    public int? PairingRating { get; set; }
    public string? PairingNote { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class DrinkBottleResponse
{
    public DrinkRecordDto DrinkRecord { get; set; } = null!;
    public CellarItemDto? CellarItem { get; set; }
}
