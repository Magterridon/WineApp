using System.ComponentModel.DataAnnotations;

namespace WineApp.Api.DTOs;

public class RuleConditionDto
{
    [Required]
    public string Field { get; set; } = string.Empty;
    [Required]
    public string Operator { get; set; } = string.Empty;
    [Required]
    public string Value { get; set; } = string.Empty;
}

public class RecipeRefDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class PairingRuleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public List<RuleConditionDto> Conditions { get; set; } = new();
    public List<RecipeRefDto> Recipes { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class SavePairingRuleRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    [Range(1, 100)]
    public int Priority { get; set; } = 10;
    [Required]
    public List<RuleConditionDto> Conditions { get; set; } = new();
    [Required, MinLength(1)]
    public List<int> RecipeIds { get; set; } = new();
}

public class PairingCandidateDto
{
    public int WineId { get; set; }
    public int RecipeId { get; set; }
    public int Priority { get; set; }
    public string RuleName { get; set; } = string.Empty;
}

public class RuleMatchedWineDto
{
    public int WineId { get; set; }
    public string WineName { get; set; } = string.Empty;
    public int WineYear { get; set; }
    public string RuleName { get; set; } = string.Empty;
}
