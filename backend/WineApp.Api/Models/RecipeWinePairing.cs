namespace WineApp.Api.Models;

public class RecipeWinePairing
{
    public int Id { get; set; }
    public int RecipeId { get; set; }
    public int WineId { get; set; }
    public string? Notes { get; set; }

    public Recipe Recipe { get; set; } = null!;
    public Wine Wine { get; set; } = null!;
}
