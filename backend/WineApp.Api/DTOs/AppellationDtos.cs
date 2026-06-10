namespace WineApp.Api.DTOs;

public class WineAppellationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Region { get; set; }
    public string GiType { get; set; } = string.Empty;
    public string? Colors { get; set; }
}
