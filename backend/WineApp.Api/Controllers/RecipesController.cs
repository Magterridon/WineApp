using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WineApp.Api.DTOs;
using WineApp.Api.Services;

namespace WineApp.Api.Controllers;

[ApiController]
[Route("api/recipes")]
[Authorize]
public class RecipesController(RecipeService recipes) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? recipeType)
    {
        return Ok(await recipes.GetAllAsync(search, recipeType));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var recipe = await recipes.GetByIdAsync(id);
        return recipe is null ? NotFound() : Ok(recipe);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateRecipeRequest request)
    {
        var recipe = await recipes.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = recipe.Id }, recipe);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, CreateRecipeRequest request)
    {
        var recipe = await recipes.UpdateAsync(id, request);
        return recipe is null ? NotFound() : Ok(recipe);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        return await recipes.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
