namespace WineApp.Api.Models;

public class WineAppellation
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Region { get; set; }
    public string GiType { get; set; } = string.Empty;   // AOP, IGP, AOC
    public string? Colors { get; set; }                   // e.g. "Rouge, Blanc, Rosé"
    public string SourceName { get; set; } = string.Empty;
    public string? SourceUrl { get; set; }
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
}
