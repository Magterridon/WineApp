using Microsoft.EntityFrameworkCore;
using WineApp.Api.Data;
using WineApp.Api.DTOs;
using WineApp.Api.Models;

namespace WineApp.Api.Services;

public class CellarService(AppDbContext db)
{
    public async Task<CellarDto> GetCellarAsync(int userId)
    {
        var cellar = await db.Cellars
            .Include(c => c.Items).ThenInclude(i => i.Wine).ThenInclude(w => w.Cepages)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cellar is null) return new CellarDto();

        return new CellarDto
        {
            Id = cellar.Id,
            Items = cellar.Items.Select(i => ToItemDto(i)).ToList()
        };
    }

    public async Task<CellarItemDto> AddItemAsync(int userId, int wineId)
    {
        var wine = await db.Wines.Include(w => w.Cepages).FirstOrDefaultAsync(w => w.Id == wineId)
            ?? throw new ArgumentException("Wine not found");

        var cellar = await db.Cellars.Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new InvalidOperationException("Cellar not found");

        var existing = cellar.Items.FirstOrDefault(i => i.WineId == wineId);
        if (existing is not null)
        {
            existing.BottleCount++;
            await db.SaveChangesAsync();
            return ToItemDto(existing, wine);
        }

        var item = new CellarItem { CellarId = cellar.Id, WineId = wineId, BottleCount = 1 };
        db.CellarItems.Add(item);
        await db.SaveChangesAsync();
        return ToItemDto(item, wine);
    }

    public async Task<CellarItemDto?> IncrementAsync(int userId, int wineId)
    {
        var item = await GetItemWithWineAsync(userId, wineId);
        if (item is null) return null;

        item.BottleCount++;
        await db.SaveChangesAsync();
        return ToItemDto(item);
    }

    public async Task<CellarItemDto?> DecrementAsync(int userId, int wineId)
    {
        var item = await GetItemWithWineAsync(userId, wineId);
        if (item is null) return null;

        item.BottleCount--;
        if (item.BottleCount <= 0)
        {
            db.CellarItems.Remove(item);
            await db.SaveChangesAsync();
            return null;
        }

        await db.SaveChangesAsync();
        return ToItemDto(item);
    }

    public async Task RemoveItemAsync(int userId, int wineId)
    {
        var item = await GetItemWithWineAsync(userId, wineId);
        if (item is null) return;

        db.CellarItems.Remove(item);
        await db.SaveChangesAsync();
    }

    private async Task<CellarItem?> GetItemWithWineAsync(int userId, int wineId) =>
        await db.CellarItems
            .Include(i => i.Wine).ThenInclude(w => w.Cepages)
            .Include(i => i.Cellar)
            .FirstOrDefaultAsync(i => i.Cellar.UserId == userId && i.WineId == wineId);

    private static CellarItemDto ToItemDto(CellarItem item, Wine? wine = null) => new()
    {
        WineId = item.WineId,
        BottleCount = item.BottleCount,
        Wine = (wine ?? item.Wine) is { } w ? WineService.ToDto(w) : null
    };
}
