using Microsoft.EntityFrameworkCore;
using WineApp.Api.Data;
using WineApp.Api.DTOs;
using WineApp.Api.Models;

namespace WineApp.Api.Services;

public class AdminWineService(AppDbContext db, IWebHostEnvironment env)
{
    // ── List with filter / sort / page ────────────────────────────────────────

    public async Task<AdminWinesResponse> GetAdminWinesAsync(AdminWinesQuery q)
    {
        var query = db.Wines
            .Include(w => w.Cepages)
            .Include(w => w.RecipePairings)
            .AsQueryable();

        // ── Filters ────────────────────────────────────────────────────────────

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.ToLower();
            query = query.Where(w =>
                w.Name.ToLower().Contains(s) ||
                w.Domain.ToLower().Contains(s) ||
                (w.Appellation != null && w.Appellation.ToLower().Contains(s)) ||
                (w.Region != null && w.Region.ToLower().Contains(s)) ||
                w.Cepages.Any(c => c.CepageName.ToLower().Contains(s)));
        }

        if (q.Rank.HasValue)       query = query.Where(w => w.Rank == q.Rank.Value);
        if (q.Year.HasValue)       query = query.Where(w => w.Year == q.Year.Value);

        if (!string.IsNullOrWhiteSpace(q.Color))
            query = query.Where(w => w.Color != null && w.Color.ToLower() == q.Color.ToLower());

        if (!string.IsNullOrWhiteSpace(q.Region))
            query = query.Where(w => w.Region != null && w.Region.ToLower() == q.Region.ToLower());

        if (!string.IsNullOrWhiteSpace(q.Appellation))
        {
            var a = q.Appellation.ToLower();
            query = query.Where(w => w.Appellation != null && w.Appellation.ToLower().Contains(a));
        }

        if (!string.IsNullOrWhiteSpace(q.Cepage))
        {
            var c = q.Cepage.ToLower();
            query = query.Where(w => w.Cepages.Any(cp => cp.CepageName.ToLower().Contains(c)));
        }

        if (q.HasImage.HasValue)
        {
            query = q.HasImage.Value
                ? query.Where(w => w.ImageUrl != null && w.ImageUrl != string.Empty)
                : query.Where(w => w.ImageUrl == null || w.ImageUrl == string.Empty);
        }

        if (q.HasPairing.HasValue)
        {
            query = q.HasPairing.Value
                ? query.Where(w => w.RecipePairings.Any())
                : query.Where(w => !w.RecipePairings.Any());
        }

        // ── Total count (before pagination) ───────────────────────────────────

        var total = await query.CountAsync();

        // ── Sort ──────────────────────────────────────────────────────────────

        bool desc = string.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

        query = q.SortBy?.ToLower() switch
        {
            "domain"      => desc ? query.OrderByDescending(w => w.Domain)      : query.OrderBy(w => w.Domain),
            "year"        => desc ? query.OrderByDescending(w => w.Year)         : query.OrderBy(w => w.Year),
            "rank"        => desc ? query.OrderByDescending(w => w.Rank)         : query.OrderBy(w => w.Rank),
            "appellation" => desc ? query.OrderByDescending(w => w.Appellation)  : query.OrderBy(w => w.Appellation),
            "region"      => desc ? query.OrderByDescending(w => w.Region)       : query.OrderBy(w => w.Region),
            "createdat"   => desc ? query.OrderByDescending(w => w.CreatedAt)    : query.OrderBy(w => w.CreatedAt),
            "hasimage"    => desc ? query.OrderByDescending(w => w.ImageUrl != null && w.ImageUrl != "") : query.OrderBy(w => w.ImageUrl != null && w.ImageUrl != ""),
            "haspairing"  => desc ? query.OrderByDescending(w => w.RecipePairings.Any()) : query.OrderBy(w => w.RecipePairings.Any()),
            _             => desc ? query.OrderByDescending(w => w.Name)         : query.OrderBy(w => w.Name),
        };

        // ── Pagination ────────────────────────────────────────────────────────

        var pageSize = Math.Clamp(q.PageSize, 10, 200);
        var page     = Math.Max(1, q.Page);
        var skip     = (page - 1) * pageSize;

        var wines = await query.Skip(skip).Take(pageSize).ToListAsync();

        return new AdminWinesResponse
        {
            Total    = total,
            Page     = page,
            PageSize = pageSize,
            Items    = wines.Select(ToAdminDto).ToList()
        };
    }

    // ── Bulk image update ─────────────────────────────────────────────────────

    public async Task<int> BulkSetImageAsync(BulkImageRequest req)
    {
        var wines = await db.Wines
            .Where(w => req.WineIds.Contains(w.Id))
            .ToListAsync();

        foreach (var wine in wines)
            wine.ImageUrl = req.ImageUrl.Trim();

        await db.SaveChangesAsync();
        return wines.Count;
    }

    // ── Image file upload ────────────────────────────────────────────────────

    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public async Task<string> UploadImageAsync(IFormFile file, HttpRequest request)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            throw new ArgumentException($"File type not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}");

        if (file.Length > MaxFileSizeBytes)
            throw new ArgumentException("File size exceeds the 10 MB limit");

        var uploadsPath = Path.Combine(env.WebRootPath, "uploads", "wines");
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsPath, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        // Return absolute URL so the frontend can display the image immediately
        var baseUrl = $"{request.Scheme}://{request.Host}";
        return $"{baseUrl}/uploads/wines/{fileName}";
    }

    // ── Bulk pairing assignment ───────────────────────────────────────────────

    public async Task<BulkPairingResult> BulkAssignPairingsAsync(BulkPairingRequest req)
    {
        // Validate that all wine IDs and recipe IDs exist
        var validWineIds = await db.Wines
            .Where(w => req.WineIds.Contains(w.Id))
            .Select(w => w.Id)
            .ToListAsync();

        var validRecipeIds = await db.Recipes
            .Where(r => req.RecipeIds.Contains(r.Id))
            .Select(r => r.Id)
            .ToListAsync();

        // Load existing pairings for these wines/recipes to skip duplicates
        var existingPairings = await db.RecipeWinePairings
            .Where(p => validWineIds.Contains(p.WineId) && validRecipeIds.Contains(p.RecipeId))
            .Select(p => new { p.WineId, p.RecipeId })
            .ToListAsync();

        var existingSet = existingPairings
            .Select(p => $"{p.WineId}:{p.RecipeId}")
            .ToHashSet();

        int created = 0;
        int skipped = 0;

        foreach (var wineId in validWineIds)
        {
            foreach (var recipeId in validRecipeIds)
            {
                var key = $"{wineId}:{recipeId}";
                if (existingSet.Contains(key))
                {
                    skipped++;
                    continue;
                }

                db.RecipeWinePairings.Add(new RecipeWinePairing
                {
                    WineId   = wineId,
                    RecipeId = recipeId
                });
                existingSet.Add(key);
                created++;
            }
        }

        await db.SaveChangesAsync();
        return new BulkPairingResult { Created = created, Skipped = skipped };
    }

    // ── DTO mapping ──────────────────────────────────────────────────────────

    private static AdminWineDto ToAdminDto(Wine w) => new()
    {
        Id            = w.Id,
        Name          = w.Name,
        Domain        = w.Domain,
        Year          = w.Year,
        Rank          = w.Rank,
        Color         = w.Color,
        Country       = w.Country,
        Region        = w.Region,
        Appellation   = w.Appellation,
        Description   = w.Description,
        ImageUrl      = w.ImageUrl,
        DrinkFromYear = w.DrinkFromYear,
        DrinkToYear   = w.DrinkToYear,
        CreatedAt     = w.CreatedAt,
        Cepages       = w.Cepages.Select(c => new CepageDto
        {
            CepageName = c.CepageName, Percentage = c.Percentage
        }).ToList(),
        PairingCount  = w.RecipePairings.Count,
        HasImage      = !string.IsNullOrEmpty(w.ImageUrl),
        HasPairing    = w.RecipePairings.Count > 0
    };
}
