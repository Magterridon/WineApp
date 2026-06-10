using Microsoft.EntityFrameworkCore;
using WineApp.Api.Models;

namespace WineApp.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Cellar> Cellars => Set<Cellar>();
    public DbSet<CellarItem> CellarItems => Set<CellarItem>();
    public DbSet<Wine> Wines => Set<Wine>();
    public DbSet<WineCepage> WineCepages => Set<WineCepage>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RecipeWinePairing> RecipeWinePairings => Set<RecipeWinePairing>();
    public DbSet<WineAppellation> WineAppellations => Set<WineAppellation>();
    public DbSet<DrinkRecord> DrinkRecords => Set<DrinkRecord>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>()
            .HasIndex(u => u.Email).IsUnique();

        builder.Entity<Wine>()
            .HasIndex(w => new { w.Name, w.Domain, w.Year }).IsUnique();

        builder.Entity<CellarItem>()
            .HasIndex(i => new { i.CellarId, i.WineId }).IsUnique();

        builder.Entity<Cellar>()
            .HasOne(c => c.User).WithOne(u => u.Cellar)
            .HasForeignKey<Cellar>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CellarItem>()
            .HasOne(i => i.Cellar).WithMany(c => c.Items)
            .HasForeignKey(i => i.CellarId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CellarItem>()
            .HasOne(i => i.Wine).WithMany(w => w.CellarItems)
            .HasForeignKey(i => i.WineId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<WineCepage>()
            .HasOne(c => c.Wine).WithMany(w => w.Cepages)
            .HasForeignKey(c => c.WineId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RecipeWinePairing>()
            .HasOne(p => p.Recipe).WithMany(r => r.WinePairings)
            .HasForeignKey(p => p.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RecipeWinePairing>()
            .HasOne(p => p.Wine).WithMany(w => w.RecipePairings)
            .HasForeignKey(p => p.WineId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<WineAppellation>()
            .HasIndex(a => new { a.Country, a.Name, a.GiType }).IsUnique();

        builder.Entity<DrinkRecord>()
            .HasOne(d => d.User).WithMany(u => u.DrinkRecords)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<DrinkRecord>()
            .HasOne(d => d.Wine).WithMany(w => w.DrinkRecords)
            .HasForeignKey(d => d.WineId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<DrinkRecord>()
            .HasOne(d => d.Recipe).WithMany()
            .HasForeignKey(d => d.RecipeId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}
