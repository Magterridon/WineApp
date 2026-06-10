namespace WineApp.Api.Models;

public class Cellar
{
    public int Id { get; set; }
    public int UserId { get; set; }

    public User User { get; set; } = null!;
    public List<CellarItem> Items { get; set; } = new();
}
