using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WineApp.Api.Services;

namespace WineApp.Api.Controllers;

[ApiController]
[Route("api/pairing-rules")]
[Authorize]
public class PairingRulesController(PairingRuleService service) : ControllerBase
{
    [HttpGet("candidates")]
    public async Task<IActionResult> GetCandidates([FromQuery] int[] wineIds)
    {
        var candidates = await service.GetCandidatesAsync(wineIds);
        return Ok(candidates);
    }

    [HttpGet("wines-for-recipe/{recipeId:int}")]
    public async Task<IActionResult> GetWinesForRecipe(int recipeId)
    {
        var wines = await service.GetMatchingWinesForRecipeAsync(recipeId);
        return Ok(wines);
    }
}
