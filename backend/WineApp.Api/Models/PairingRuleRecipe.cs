namespace WineApp.Api.Models;

public class PairingRuleRecipe
{
    public int PairingRuleId { get; set; }
    public int RecipeId { get; set; }

    public PairingRule Rule { get; set; } = null!;
    public Recipe Recipe { get; set; } = null!;
}
