namespace WineApp.Api.DTOs;

public class CellarDto
{
    public int Id { get; set; }
    public List<CellarItemDto> Items { get; set; } = new();
}

public class CellarItemDto
{
    public int WineId { get; set; }
    public int BottleCount { get; set; }
    public WineDto? Wine { get; set; }
}

public class AddCellarItemRequest
{
    public int WineId { get; set; }
}
