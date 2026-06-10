using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WineApp.Api.DTOs;
using WineApp.Api.Services;

namespace WineApp.Api.Controllers;

[ApiController]
[Route("api/wines")]
[Authorize]
public class WinesController(WineService wines) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? rank,
        [FromQuery] int? year,
        [FromQuery] string? color,
        [FromQuery] string? country,
        [FromQuery] string? drinkStatus)
    {
        return Ok(await wines.GetAllAsync(search, rank, year, color, country, drinkStatus));
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
