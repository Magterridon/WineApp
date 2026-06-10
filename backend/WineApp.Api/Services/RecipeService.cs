using Microsoft.EntityFrameworkCore;
using WineApp.Api.Data;
using WineApp.Api.DTOs;
using WineApp.Api.Models;

namespace WineApp.Api.Services;

public class RecipeService(AppDbContext db)
{
    public async Task<List<RecipeDto>> GetAllAsync(string? search, string? recipeType)
    {
        var query = db.Recipes
            .Include(r => r.WinePairings).ThenInclude(p => p.Wine)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.ToLower();
            query = query.Where(r =>
                r.Name.ToLower().Contains(q) ||
                r.Description.ToLower().Contains(q) ||
                r.Ingredients.ToLower().Contains(q) ||
                r.RecipeType.ToLower().Contains(q) ||
                r.WinePairings.Any(p => p.Wine.Name.ToLower().Contains(q)));
        }

        if (!string.IsNullOrWhiteSpace(recipeType))
            query = query.Where(r => r.RecipeType.ToLower() == recipeType.ToLower());

        var recipes = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        return recipes.Select(ToDto).ToList();
    }

    public async Task<RecipeDto?> GetByIdAsync(int id)
    {
        var recipe = await db.Recipes
            .Include(r => r.WinePairings).ThenInclude(p => p.Wine)
            .FirstOrDefaultAsync(r => r.Id == id);
        return recipe is null ? null : ToDto(recipe);
    }

    public async Task<RecipeDto> CreateAsync(CreateRecipeRequest req)
    {
        var recipe = new Recipe
        {
            Name = req.Name, Description = req.Description,
            Ingredients = string.Join('\n', req.Ingredients),
            Instructions = req.Instructions,
            ImageUrl = req.ImageUrl, RecipeType = req.RecipeType,
            WinePairings = req.Pairings.Select(p => new RecipeWinePairing
            {
                WineId = p.WineId, Notes = p.Notes
            }).ToList()
        };

        db.Recipes.Add(recipe);
        await db.SaveChangesAsync();
        await db.Entry(recipe).Collection(r => r.WinePairings).Query()
            .Include(p => p.Wine).LoadAsync();
        return ToDto(recipe);
    }

    public async Task<RecipeDto?> UpdateAsync(int id, CreateRecipeRequest req)
    {
        var recipe = await db.Recipes
            .Include(r => r.WinePairings)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (recipe is null) return null;

        recipe.Name = req.Name; recipe.Description = req.Description;
        recipe.Ingredients = string.Join('\n', req.Ingredients);
        recipe.Instructions = req.Instructions;
        recipe.ImageUrl = req.ImageUrl; recipe.RecipeType = req.RecipeType;

        db.RecipeWinePairings.RemoveRange(recipe.WinePairings);
        recipe.WinePairings = req.Pairings.Select(p => new RecipeWinePairing
        {
            RecipeId = recipe.Id, WineId = p.WineId, Notes = p.Notes
        }).ToList();

        await db.SaveChangesAsync();
        await db.Entry(recipe).Collection(r => r.WinePairings).Query()
            .Include(p => p.Wine).LoadAsync();
        return ToDto(recipe);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var recipe = await db.Recipes.FindAsync(id);
        if (recipe is null) return false;
        db.Recipes.Remove(recipe);
        await db.SaveChangesAsync();
        return true;
    }

    private static RecipeDto ToDto(Recipe r) => new()
    {
        Id = r.Id, Name = r.Name, Description = r.Description,
        Ingredients = r.Ingredients.Split('\n', StringSplitOptions.RemoveEmptyEntries),
        Instructions = r.Instructions, ImageUrl = r.ImageUrl,
        RecipeType = r.RecipeType, CreatedAt = r.CreatedAt,
        Pairings = r.WinePairings.Select(p => new RecipePairingDto
        {
            WineId = p.WineId,
            WineName = p.Wine?.Name,
            WineYear = p.Wine?.Year,
            Notes = p.Notes
        }).ToList()
    };
}
