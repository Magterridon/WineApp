namespace WineApp.Api.Models;

public class WineCepage
{
    public int Id { get; set; }
    public int WineId { get; set; }
    public string CepageName { get; set; } = string.Empty;
    public int? Percentage { get; set; }

    public Wine Wine { get; set; } = null!;
}
