using Microsoft.EntityFrameworkCore;
using WineApp.Api.Data;
using WineApp.Api.DTOs;
using WineApp.Api.Models;

namespace WineApp.Api.Services;

public class DrinkService(AppDbContext db)
{
    public async Task<DrinkBottleResponse> DrinkBottleAsync(int userId, CreateDrinkRecordRequest request)
    {
        var wine = await db.Wines.Include(w => w.Cepages).FirstOrDefaultAsync(w => w.Id == request.WineId)
            ?? throw new ArgumentException("Wine not found");

        var cellar = await db.Cellars.Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new InvalidOperationException("Cellar not found");

        var item = cellar.Items.FirstOrDefault(i => i.WineId == request.WineId)
            ?? throw new InvalidOperationException("Wine is not in your cellar");

        Recipe? recipe = null;
        if (request.RecipeId.HasValue)
        {
            recipe = await db.Recipes.FindAsync(request.RecipeId.Value);
            if (recipe is null)
                throw new ArgumentException("Recipe not found");
        }

        var hasMeal = request.RecipeId.HasValue || !string.IsNullOrWhiteSpace(request.MealNote);

        var record = new DrinkRecord
        {
            UserId = userId,
            WineId = request.WineId,
            ConsumedAt = request.ConsumedAt?.ToUniversalTime() ?? DateTime.UtcNow,
            Rating = request.Rating,
            TastingNote = request.TastingNote?.Trim(),
            PairedWithMeal = hasMeal,
            RecipeId = request.RecipeId,
            MealNote = request.MealNote?.Trim(),
            PairingRating = hasMeal ? request.PairingRating : null,
            PairingNote = hasMeal ? request.PairingNote?.Trim() : null,
            CreatedAt = DateTime.UtcNow
        };

        db.DrinkRecords.Add(record);

        item.BottleCount--;
        CellarItemDto? cellarItemDto = null;

        if (item.BottleCount <= 0)
        {
            db.CellarItems.Remove(item);
        }
        else
        {
            cellarItemDto = new CellarItemDto
            {
                WineId = item.WineId,
                BottleCount = item.BottleCount,
                Wine = WineService.ToDto(wine)
            };
        }

        await db.SaveChangesAsync();

        return new DrinkBottleResponse
        {
            DrinkRecord = ToDto(record, wine, recipe),
            CellarItem = cellarItemDto
        };
    }

    public async Task<List<DrinkRecordDto>> GetHistoryAsync(int userId)
    {
        return await db.DrinkRecords
            .Include(d => d.Wine)
            .Include(d => d.Recipe)
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.ConsumedAt)
            .Select(d => ToDto(d, d.Wine, d.Recipe))
            .ToListAsync();
    }

    public async Task<List<DrinkRecordDto>> GetWineHistoryAsync(int userId, int wineId)
    {
        return await db.DrinkRecords
            .Include(d => d.Wine)
            .Include(d => d.Recipe)
            .Where(d => d.UserId == userId && d.WineId == wineId)
            .OrderByDescending(d => d.ConsumedAt)
            .Select(d => ToDto(d, d.Wine, d.Recipe))
            .ToListAsync();
    }

    private static DrinkRecordDto ToDto(DrinkRecord record, Wine wine, Recipe? recipe) => new()
    {
        Id = record.Id,
        WineId = wine.Id,
        WineName = wine.Name,
        WineDomain = wine.Domain,
        WineYear = wine.Year,
        WineDrinkFromYear = wine.DrinkFromYear,
        WineDrinkToYear = wine.DrinkToYear,
        ConsumedAt = record.ConsumedAt,
        Rating = record.Rating,
        TastingNote = record.TastingNote,
        PairedWithMeal = record.PairedWithMeal,
        RecipeId = record.RecipeId,
        RecipeName = recipe?.Name,
        MealNote = record.MealNote,
        PairingRating = record.PairingRating,
        PairingNote = record.PairingNote,
        CreatedAt = record.CreatedAt
    };
}
