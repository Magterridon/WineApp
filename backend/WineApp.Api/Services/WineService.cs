using Microsoft.EntityFrameworkCore;
using WineApp.Api.Data;
using WineApp.Api.DTOs;
using WineApp.Api.Models;

namespace WineApp.Api.Services;

public class WineService(AppDbContext db)
{
    public async Task<List<WineDto>> GetAllAsync(
        string? search,
        int? rank,
        int? year,
        string? color = null,
        string? country = null,
        string? drinkStatus = null)
    {
        var query = db.Wines.Include(w => w.Cepages).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.ToLower();
            query = query.Where(w =>
                w.Name.ToLower().Contains(q) ||
                w.Domain.ToLower().Contains(q) ||
                w.Year.ToString().Contains(q) ||
                (w.Country != null && w.Country.ToLower().Contains(q)) ||
                (w.Region != null && w.Region.ToLower().Contains(q)) ||
                (w.Appellation != null && w.Appellation.ToLower().Contains(q)) ||
                (w.Color != null && w.Color.ToLower().Contains(q)) ||
                w.Cepages.Any(c => c.CepageName.ToLower().Contains(q)));
        }

        if (rank.HasValue) query = query.Where(w => w.Rank == rank.Value);
        if (year.HasValue) query = query.Where(w => w.Year == year.Value);

        if (!string.IsNullOrWhiteSpace(color))
            query = query.Where(w => w.Color == color);

        if (!string.IsNullOrWhiteSpace(country))
            query = query.Where(w => w.Country != null && w.Country.ToLower() == country.ToLower());

        if (!string.IsNullOrWhiteSpace(drinkStatus))
        {
            var currentYear = DateTime.UtcNow.Year;
            query = drinkStatus switch
            {
                "young" => query.Where(w => w.DrinkFromYear.HasValue && currentYear < w.DrinkFromYear),
                "ready" => query.Where(w => w.DrinkFromYear.HasValue && w.DrinkToYear.HasValue
                               && currentYear >= w.DrinkFromYear && currentYear <= w.DrinkToYear
                               && w.DrinkToYear - currentYear > 2),
                "soon"  => query.Where(w => w.DrinkToYear.HasValue
                               && currentYear <= w.DrinkToYear && w.DrinkToYear - currentYear <= 2),
                "past"  => query.Where(w => w.DrinkToYear.HasValue && currentYear > w.DrinkToYear),
                _ => query
            };
        }

        var wines = await query.OrderByDescending(w => w.CreatedAt).ToListAsync();
        return wines.Select(ToDto).ToList();
    }

    public async Task<List<WineDto>> GetSimilarAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return new();
        var q = name.ToLower();
        var wines = await db.Wines.Include(w => w.Cepages)
            .Where(w => w.Name.ToLower().Contains(q))
            .OrderByDescending(w => w.CreatedAt)
            .Take(5)
            .ToListAsync();
        return wines.Select(ToDto).ToList();
    }

    public async Task<WineDto?> GetByIdAsync(int id)
    {
        var wine = await db.Wines.Include(w => w.Cepages).FirstOrDefaultAsync(w => w.Id == id);
        return wine is null ? null : ToDto(wine);
    }

    public async Task<WineDto> CreateAsync(CreateWineRequest req)
    {
        if (await db.Wines.AnyAsync(w => w.Name == req.Name && w.Domain == req.Domain && w.Year == req.Year))
            throw new InvalidOperationException("A wine with this name, domain, and year already exists");

        var wine = new Wine
        {
            Name = req.Name, Domain = req.Domain, Year = req.Year,
            Rank = req.Rank, Color = req.Color?.Trim(), Country = req.Country?.Trim(),
            Region = req.Region?.Trim(), Appellation = req.Appellation?.Trim(),
            Description = req.Description, ImageUrl = req.ImageUrl,
            DrinkFromYear = req.DrinkFromYear, DrinkToYear = req.DrinkToYear,
            Cepages = req.Cepages.Select(c => new WineCepage
            {
                CepageName = c.CepageName, Percentage = c.Percentage
            }).ToList()
        };

        db.Wines.Add(wine);
        await db.SaveChangesAsync();
        return ToDto(wine);
    }

    public async Task<WineDto?> UpdateAsync(int id, CreateWineRequest req)
    {
        var wine = await db.Wines.Include(w => w.Cepages).FirstOrDefaultAsync(w => w.Id == id);
        if (wine is null) return null;

        if (await db.Wines.AnyAsync(w => w.Name == req.Name && w.Domain == req.Domain && w.Year == req.Year && w.Id != id))
            throw new InvalidOperationException("A wine with this name, domain, and year already exists");

        wine.Name = req.Name; wine.Domain = req.Domain; wine.Year = req.Year;
        wine.Rank = req.Rank; wine.Color = req.Color?.Trim(); wine.Country = req.Country?.Trim();
        wine.Region = req.Region?.Trim(); wine.Appellation = req.Appellation?.Trim();
        wine.Description = req.Description;
        wine.ImageUrl = req.ImageUrl; wine.DrinkFromYear = req.DrinkFromYear; wine.DrinkToYear = req.DrinkToYear;

        db.WineCepages.RemoveRange(wine.Cepages);
        wine.Cepages = req.Cepages.Select(c => new WineCepage
        {
            WineId = wine.Id, CepageName = c.CepageName, Percentage = c.Percentage
        }).ToList();

        await db.SaveChangesAsync();
        return ToDto(wine);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var wine = await db.Wines.FindAsync(id);
        if (wine is null) return false;
        db.Wines.Remove(wine);
        await db.SaveChangesAsync();
        return true;
    }

    public static WineDto ToDto(Wine w) => new()
    {
        Id = w.Id, Name = w.Name, Domain = w.Domain, Year = w.Year,
        Rank = w.Rank, Color = w.Color, Country = w.Country,
        Region = w.Region, Appellation = w.Appellation,
        Description = w.Description, ImageUrl = w.ImageUrl,
        DrinkFromYear = w.DrinkFromYear, DrinkToYear = w.DrinkToYear,
        CreatedAt = w.CreatedAt,
        Cepages = w.Cepages.Select(c => new CepageDto
        {
            CepageName = c.CepageName, Percentage = c.Percentage
        }).ToList()
    };
}
