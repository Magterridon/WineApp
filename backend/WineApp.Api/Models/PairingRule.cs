namespace WineApp.Api.Models;

public class PairingRule
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 10;
    public string ConditionsJson { get; set; } = "[]";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<PairingRuleRecipe> RecipeTargets { get; set; } = new();
}
