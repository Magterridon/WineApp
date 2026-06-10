using System.ComponentModel.DataAnnotations;

namespace WineApp.Api.DTOs;

public class CepageDto
{
    public string CepageName { get; set; } = string.Empty;
    public int? Percentage { get; set; }
}

public class WineDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Rank { get; set; }
    public string? Color { get; set; }
    public string? Country { get; set; }
    public string? Region { get; set; }
    public string? Appellation { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int? DrinkFromYear { get; set; }
    public int? DrinkToYear { get; set; }
    public List<CepageDto> Cepages { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

// ── Admin backoffice DTOs ─────────────────────────────────────────────────────

public class AdminWineDto : WineDto
{
    public int PairingCount { get; set; }
    public bool HasImage { get; set; }
    public bool HasPairing { get; set; }
}

public class AdminWinesQuery
{
    public string? Search       { get; set; }
    public int?    Rank         { get; set; }
    public int?    Year         { get; set; }
    public string? Color        { get; set; }
    public string? Region       { get; set; }
    public string? Appellation  { get; set; }
    public string? Cepage       { get; set; }
    /// <summary>null = all, true = only wines with image, false = only without image</summary>
    public bool?   HasImage     { get; set; }
    /// <summary>null = all, true = only wines with ≥1 pairing, false = only without</summary>
    public bool?   HasPairing   { get; set; }
    public string  SortBy       { get; set; } = "name";
    public string  SortDir      { get; set; } = "asc";
    public int     Page         { get; set; } = 1;
    public int     PageSize     { get; set; } = 50;
}

public class AdminWinesResponse
{
    public List<AdminWineDto> Items    { get; set; } = new();
    public int                Total    { get; set; }
    public int                Page     { get; set; }
    public int                PageSize { get; set; }
}

public class BulkImageRequest
{
    [Required, MinLength(1)]
    public List<int> WineIds  { get; set; } = new();

    [Required]
    public string ImageUrl { get; set; } = string.Empty;
}

public class BulkPairingRequest
{
    [Required, MinLength(1)]
    public List<int> WineIds   { get; set; } = new();

    [Required, MinLength(1)]
    public List<int> RecipeIds { get; set; } = new();
}

public class BulkPairingResult
{
    public int Created { get; set; }
    public int Skipped { get; set; }
}

public class UploadImageResult
{
    public string ImageUrl { get; set; } = string.Empty;
}

public class CreateWineRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Domain { get; set; } = string.Empty;

    [Required, Range(1800, 2100)]
    public int Year { get; set; }

    [Range(1, 5)]
    public int Rank { get; set; } = 3;

    public string? Color { get; set; }
    public string? Country { get; set; }
    public string? Region { get; set; }
    public string? Appellation { get; set; }
    public string? Description { get; set; }

    [Url]
    public string? ImageUrl { get; set; }

    public int? DrinkFromYear { get; set; }
    public int? DrinkToYear { get; set; }

    public List<CreateCepageRequest> Cepages { get; set; } = new();
}

public class CreateCepageRequest
{
    [Required]
    public string CepageName { get; set; } = string.Empty;

    [Range(0, 100)]
    public int? Percentage { get; set; }
}
