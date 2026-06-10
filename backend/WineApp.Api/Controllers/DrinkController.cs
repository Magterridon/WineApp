using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WineApp.Api.DTOs;
using WineApp.Api.Services;

namespace WineApp.Api.Controllers;

[ApiController]
[Route("api/cellar")]
[Authorize]
public class DrinkController(DrinkService drinkService) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("drink")]
    public async Task<ActionResult<DrinkBottleResponse>> DrinkBottle([FromBody] CreateDrinkRecordRequest request)
    {
        try
        {
            var result = await drinkService.DrinkBottleAsync(UserId, request);
            return Ok(result);
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

    [HttpGet("history")]
    public async Task<ActionResult<List<DrinkRecordDto>>> GetHistory()
    {
        return Ok(await drinkService.GetHistoryAsync(UserId));
    }

    [HttpGet("history/wine/{wineId:int}")]
    public async Task<ActionResult<List<DrinkRecordDto>>> GetWineHistory(int wineId)
    {
        return Ok(await drinkService.GetWineHistoryAsync(UserId, wineId));
    }
}
