using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WineApp.Api.Data;
using WineApp.Api.Models;

namespace WineApp.Api.Services;

public class WineAppellationImporter(AppDbContext db, ILogger<WineAppellationImporter> logger)
{
    private const string SourceName = "INAO via data.gouv.fr";
    private const string SourceUrl = "https://www.data.gouv.fr/fr/datasets/appellations-viticoles-et-signes-de-qualite-en-france/";

    public record ImportResult(int Created, int Updated, int Skipped, int Total);

    public async Task<ImportResult> ImportAsync(string jsonFilePath)
    {
        if (!File.Exists(jsonFilePath))
            throw new FileNotFoundException($"Appellation seed file not found: {jsonFilePath}");

        var json = await File.ReadAllTextAsync(jsonFilePath);
        var document = JsonDocument.Parse(json);
        var items = document.RootElement.GetProperty("appellations").EnumerateArray().ToList();

        int created = 0, updated = 0, skipped = 0;

        foreach (var item in items)
        {
            var name = item.GetProperty("name").GetString()!;
            var region = item.TryGetProperty("region", out var r) ? r.GetString() : null;
            var giType = item.GetProperty("giType").GetString()!;
            var colors = item.TryGetProperty("colors", out var c) ? c.GetString() : null;

            var existing = await db.WineAppellations
                .FirstOrDefaultAsync(a => a.Country == "France" && a.Name == name && a.GiType == giType);

            if (existing is null)
            {
                db.WineAppellations.Add(new WineAppellation
                {
                    Name = name,
                    Country = "France",
                    Region = region,
                    GiType = giType,
                    Colors = colors,
                    SourceName = SourceName,
                    SourceUrl = SourceUrl,
                    ImportedAt = DateTime.UtcNow
                });
                created++;
                logger.LogDebug("Creating appellation: {Name} ({GiType})", name, giType);
            }
            else
            {
                var changed = false;

                if (existing.Region != region) { existing.Region = region; changed = true; }
                if (existing.Colors != colors) { existing.Colors = colors; changed = true; }

                if (changed)
                {
                    existing.ImportedAt = DateTime.UtcNow;
                    updated++;
                    logger.LogDebug("Updating appellation: {Name} ({GiType})", name, giType);
                }
                else
                {
                    skipped++;
                }
            }
        }

        await db.SaveChangesAsync();

        logger.LogInformation(
            "Import complete: {Total} total — {Created} created, {Updated} updated, {Skipped} skipped",
            items.Count, created, updated, skipped);

        return new ImportResult(created, updated, skipped, items.Count);
    }
}
