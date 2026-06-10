namespace WineApp.Api.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Cellar Cellar { get; set; } = null!;
    public List<DrinkRecord> DrinkRecords { get; set; } = new();
}
