using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WineApp.Api.DTOs;
using WineApp.Api.Services;

namespace WineApp.Api.Controllers;

[ApiController]
[Route("api/wines")]
[Authorize]
public class WinesController(WineService wines, IWebHostEnvironment env) : ControllerBase
{
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];

    [HttpPost("upload-image")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file provided" });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(ext))
            return BadRequest(new { message = $"File type not allowed. Allowed: {string.Join(", ", AllowedImageExtensions)}" });

        if (file.Length > 10 * 1024 * 1024)
            return BadRequest(new { message = "File size exceeds the 10 MB limit" });

        var uploadsPath = Path.Combine(env.WebRootPath, "uploads", "images");
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsPath, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        var imageUrl = $"{Request.Scheme}://{Request.Host}/uploads/images/{fileName}";
        return Ok(new { imageUrl });
    }


    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? name,
        [FromQuery] string? domain,
        [FromQuery] int? rank,
        [FromQuery] int? year,
        [FromQuery] string? color,
        [FromQuery] string[]? colors,
        [FromQuery] string? country,
        [FromQuery] string? drinkStatus,
        [FromQuery] string? appellation,
        [FromQuery] string? cepage,
        [FromQuery] string[]? cepages,
        [FromQuery] int? recipeId)
    {
        return Ok(await wines.GetAllAsync(
            search, rank, year, color, country, drinkStatus, appellation, cepage,
            name, domain, colors, cepages, recipeId));
    }

    [HttpGet("similar")]
    public async Task<IActionResult> GetSimilar([FromQuery] string? name)
    {
        return Ok(await wines.GetSimilarAsync(name ?? string.Empty));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var wine = await wines.GetByIdAsync(id);
        return wine is null ? NotFound() : Ok(wine);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateWineRequest request)
    {
        try
        {
            var wine = await wines.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = wine.Id }, wine);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, CreateWineRequest request)
    {
        try
        {
            var wine = await wines.UpdateAsync(id, request);
            return wine is null ? NotFound() : Ok(wine);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        return await wines.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
