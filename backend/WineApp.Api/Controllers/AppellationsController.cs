using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WineApp.Api.Data;
using WineApp.Api.DTOs;

namespace WineApp.Api.Controllers;

[ApiController]
[Route("api/appellations")]
[Authorize]
public class AppellationsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<WineAppellationDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? region,
        [FromQuery] string? giType,
        [FromQuery] string? color)
    {
        var query = db.WineAppellations.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(a => a.Name.Contains(search) || (a.Region != null && a.Region.Contains(search)));

        if (!string.IsNullOrWhiteSpace(region))
            query = query.Where(a => a.Region == region);

        if (!string.IsNullOrWhiteSpace(giType))
            query = query.Where(a => a.GiType == giType);

        if (!string.IsNullOrWhiteSpace(color))
            query = query.Where(a => a.Colors != null && a.Colors.Contains(color));

        var results = await query
            .OrderBy(a => a.Region)
            .ThenBy(a => a.Name)
            .Select(a => new WineAppellationDto
            {
                Id = a.Id,
                Name = a.Name,
                Country = a.Country,
                Region = a.Region,
                GiType = a.GiType,
                Colors = a.Colors
            })
            .ToListAsync();

        return Ok(results);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WineAppellationDto>> GetById(int id)
    {
        var a = await db.WineAppellations.FindAsync(id);
        if (a is null) return NotFound();

        return Ok(new WineAppellationDto
        {
            Id = a.Id,
            Name = a.Name,
            Country = a.Country,
            Region = a.Region,
            GiType = a.GiType,
            Colors = a.Colors
        });
    }

    [HttpGet("regions")]
    public async Task<ActionResult<List<string>>> GetRegions()
    {
        var regions = await db.WineAppellations
            .Where(a => a.Region != null)
            .Select(a => a.Region!)
            .Distinct()
            .OrderBy(r => r)
            .ToListAsync();

        return Ok(regions);
    }
}
