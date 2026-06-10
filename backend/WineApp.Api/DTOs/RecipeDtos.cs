using System.ComponentModel.DataAnnotations;

namespace WineApp.Api.DTOs;

public class RecipePairingDto
{
    public int WineId { get; set; }
    public string? WineName { get; set; }
    public int? WineYear { get; set; }
    public string? Notes { get; set; }
}

public class RecipeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] Ingredients { get; set; } = [];
    public string Instructions { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string RecipeType { get; set; } = string.Empty;
    public List<RecipePairingDto> Pairings { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class CreateRecipeRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required, MinLength(1)]
    public string[] Ingredients { get; set; } = [];

    [Required]
    public string Instructions { get; set; } = string.Empty;

    [Url]
    public string? ImageUrl { get; set; }

    [Required]
    public string RecipeType { get; set; } = string.Empty;

    public List<CreatePairingRequest> Pairings { get; set; } = new();
}

public class CreatePairingRequest
{
    public int WineId { get; set; }
    public string? Notes { get; set; }
}
