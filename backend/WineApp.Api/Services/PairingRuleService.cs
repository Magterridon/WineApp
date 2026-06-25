using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WineApp.Api.Data;
using WineApp.Api.DTOs;
using WineApp.Api.Models;

namespace WineApp.Api.Services;

public class PairingRuleService(AppDbContext db)
{
    public async Task<List<PairingRuleDto>> GetAllAsync()
    {
        var rules = await db.PairingRules
            .Include(r => r.RecipeTargets).ThenInclude(rt => rt.Recipe)
            .OrderByDescending(r => r.Priority)
            .ThenBy(r => r.Name)
            .ToListAsync();
        return rules.Select(ToDto).ToList();
    }

    public async Task<PairingRuleDto?> GetByIdAsync(int id)
    {
        var rule = await db.PairingRules
            .Include(r => r.RecipeTargets).ThenInclude(rt => rt.Recipe)
            .FirstOrDefaultAsync(r => r.Id == id);
        return rule is null ? null : ToDto(rule);
    }

    public async Task<PairingRuleDto> CreateAsync(SavePairingRuleRequest req)
    {
        var rule = new PairingRule
        {
            Name = req.Name,
            Description = req.Description,
            IsActive = req.IsActive,
            Priority = req.Priority,
            ConditionsJson = JsonSerializer.Serialize(req.Conditions)
        };

        var recipeIds = req.RecipeIds.Distinct().ToList();
        rule.RecipeTargets = recipeIds.Select(id => new PairingRuleRecipe { RecipeId = id }).ToList();

        db.PairingRules.Add(rule);
        await db.SaveChangesAsync();

        await db.Entry(rule).Collection(r => r.RecipeTargets).Query()
            .Include(rt => rt.Recipe).LoadAsync();

        return ToDto(rule);
    }

    public async Task<PairingRuleDto?> UpdateAsync(int id, SavePairingRuleRequest req)
    {
        var rule = await db.PairingRules
            .Include(r => r.RecipeTargets)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (rule is null) return null;

        rule.Name = req.Name;
        rule.Description = req.Description;
        rule.IsActive = req.IsActive;
        rule.Priority = req.Priority;
        rule.ConditionsJson = JsonSerializer.Serialize(req.Conditions);

        db.PairingRuleRecipes.RemoveRange(rule.RecipeTargets);
        var recipeIds = req.RecipeIds.Distinct().ToList();
        rule.RecipeTargets = recipeIds.Select(rid => new PairingRuleRecipe { RecipeId = rid }).ToList();

        await db.SaveChangesAsync();

        await db.Entry(rule).Collection(r => r.RecipeTargets).Query()
            .Include(rt => rt.Recipe).LoadAsync();

        return ToDto(rule);
    }

    public async Task<PairingRuleDto?> ToggleAsync(int id)
    {
        var rule = await db.PairingRules
            .Include(r => r.RecipeTargets).ThenInclude(rt => rt.Recipe)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (rule is null) return null;

        rule.IsActive = !rule.IsActive;
        await db.SaveChangesAsync();

        return ToDto(rule);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var rule = await db.PairingRules.FindAsync(id);
        if (rule is null) return false;
        db.PairingRules.Remove(rule);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<List<PairingCandidateDto>> GetCandidatesAsync(IEnumerable<int> wineIds)
    {
        var wineIdList = wineIds.Distinct().ToList();
        if (wineIdList.Count == 0) return [];

        var wines = await db.Wines
            .Include(w => w.Cepages)
            .Where(w => wineIdList.Contains(w.Id))
            .ToListAsync();

        var rules = await db.PairingRules
            .Include(r => r.RecipeTargets)
            .Where(r => r.IsActive && r.RecipeTargets.Any())
            .OrderByDescending(r => r.Priority)
            .ToListAsync();

        var candidates = new List<PairingCandidateDto>();
        var seen = new HashSet<(int wineId, int recipeId)>();

        foreach (var rule in rules)
        {
            var conditions = JsonSerializer.Deserialize<List<RuleConditionDto>>(rule.ConditionsJson) ?? [];
            if (conditions.Count == 0) continue;

            foreach (var wine in wines)
            {
                if (!MatchesRule(wine, conditions)) continue;

                foreach (var target in rule.RecipeTargets)
                {
                    var key = (wine.Id, target.RecipeId);
                    if (!seen.Add(key)) continue;

                    candidates.Add(new PairingCandidateDto
                    {
                        WineId = wine.Id,
                        RecipeId = target.RecipeId,
                        Priority = rule.Priority,
                        RuleName = rule.Name
                    });
                }
            }
        }

        return candidates;
    }

    public async Task<List<RuleMatchedWineDto>> GetMatchingWinesForRecipeAsync(int recipeId)
    {
        var rules = await db.PairingRules
            .Include(r => r.RecipeTargets)
            .Where(r => r.IsActive && r.RecipeTargets.Any(rt => rt.RecipeId == recipeId))
            .OrderByDescending(r => r.Priority)
            .ToListAsync();

        if (rules.Count == 0) return [];

        var wines = await db.Wines.Include(w => w.Cepages).ToListAsync();
        var result = new List<RuleMatchedWineDto>();
        var seen = new HashSet<int>();

        foreach (var rule in rules)
        {
            var conditions = JsonSerializer.Deserialize<List<RuleConditionDto>>(rule.ConditionsJson) ?? [];
            if (conditions.Count == 0) continue;

            foreach (var wine in wines)
            {
                if (!MatchesRule(wine, conditions)) continue;
                if (!seen.Add(wine.Id)) continue;

                result.Add(new RuleMatchedWineDto
                {
                    WineId   = wine.Id,
                    WineName = wine.Name,
                    WineYear = wine.Year,
                    RuleName = rule.Name
                });
            }
        }

        return result;
    }

    // ── Rule evaluation ───────────────────────────────────────────────────────

    private static bool MatchesRule(Wine wine, List<RuleConditionDto> conditions) =>
        conditions.All(c => EvaluateCondition(c, wine));

    private static bool EvaluateCondition(RuleConditionDto cond, Wine wine)
    {
        if (cond.Field == "cepage")
        {
            var names = wine.Cepages.Select(c => c.CepageName).ToList();
            return cond.Operator switch
            {
                "equals"   => names.Any(n => n.Equals(cond.Value, StringComparison.OrdinalIgnoreCase)),
                "contains" => names.Any(n => n.Contains(cond.Value, StringComparison.OrdinalIgnoreCase)),
                "in"       => ParseList(cond.Value).Any(v => names.Any(n => n.Equals(v, StringComparison.OrdinalIgnoreCase))),
                _          => false
            };
        }

        var fieldValue = GetFieldValue(wine, cond.Field);
        if (fieldValue is null) return false;

        return cond.Operator switch
        {
            "equals"   => fieldValue.Equals(cond.Value, StringComparison.OrdinalIgnoreCase),
            "contains" => fieldValue.Contains(cond.Value, StringComparison.OrdinalIgnoreCase),
            "in"       => ParseList(cond.Value).Any(v => v.Equals(fieldValue, StringComparison.OrdinalIgnoreCase)),
            _          => false
        };
    }

    private static string? GetFieldValue(Wine wine, string field) => field switch
    {
        "color"       => wine.Color,
        "region"      => wine.Region,
        "appellation" => wine.Appellation,
        "country"     => wine.Country,
        "domain"      => wine.Domain,
        "name"        => wine.Name,
        "rank"        => wine.Rank.ToString(),
        _             => null
    };

    private static IEnumerable<string> ParseList(string value) =>
        value.Split(',').Select(v => v.Trim()).Where(v => v.Length > 0);

    // ── Mapping ───────────────────────────────────────────────────────────────

    private static PairingRuleDto ToDto(PairingRule r)
    {
        var conditions = JsonSerializer.Deserialize<List<RuleConditionDto>>(r.ConditionsJson) ?? [];
        return new PairingRuleDto
        {
            Id          = r.Id,
            Name        = r.Name,
            Description = r.Description,
            IsActive    = r.IsActive,
            Priority    = r.Priority,
            Conditions  = conditions,
            Recipes     = r.RecipeTargets
                           .Select(t => new RecipeRefDto { Id = t.RecipeId, Name = t.Recipe?.Name ?? "" })
                           .ToList(),
            CreatedAt   = r.CreatedAt
        };
    }
}
