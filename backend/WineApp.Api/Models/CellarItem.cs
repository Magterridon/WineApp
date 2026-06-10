namespace WineApp.Api.Models;

public class CellarItem
{
    public int Id { get; set; }
    public int CellarId { get; set; }
    public int WineId { get; set; }
    public int BottleCount { get; set; }

    public Cellar Cellar { get; set; } = null!;
    public Wine Wine { get; set; } = null!;
}
