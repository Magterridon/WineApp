using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WineApp.Api.DTOs;
using WineApp.Api.Services;

namespace WineApp.Api.Controllers;

[ApiController]
[Route("api/cellar")]
[Authorize]
public class CellarController(CellarService cellar) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetCellar()
    {
        return Ok(await cellar.GetCellarAsync(UserId));
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddCellarItemRequest request)
    {
        try
        {
            var item = await cellar.AddItemAsync(UserId, request.WineId);
            return Ok(item);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("items/{wineId}/increment")]
    public async Task<IActionResult> Increment(int wineId)
    {
        var item = await cellar.IncrementAsync(UserId, wineId);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPatch("items/{wineId}/decrement")]
    public async Task<IActionResult> Decrement(int wineId)
    {
        // Returns 200 with updated item, or 204 if item was removed (count reached 0)
        var item = await cellar.DecrementAsync(UserId, wineId);
        return item is null ? NoContent() : Ok(item);
    }

    [HttpDelete("items/{wineId}")]
    public async Task<IActionResult> RemoveItem(int wineId)
    {
        await cellar.RemoveItemAsync(UserId, wineId);
        return NoContent();
    }
}
