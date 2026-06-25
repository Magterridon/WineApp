using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WineApp.Api.DTOs;
using WineApp.Api.Services;

namespace WineApp.Api.Controllers;

[ApiController]
[Route("api/admin/pairing-rules")]
[Authorize(Roles = "Admin")]
public class AdminPairingRulesController(PairingRuleService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var rule = await service.GetByIdAsync(id);
        return rule is null ? NotFound() : Ok(rule);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SavePairingRuleRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var rule = await service.CreateAsync(req);
        return CreatedAtAction(nameof(GetById), new { id = rule.Id }, rule);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SavePairingRuleRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var rule = await service.UpdateAsync(id, req);
        return rule is null ? NotFound() : Ok(rule);
    }

    [HttpPatch("{id:int}/toggle")]
    public async Task<IActionResult> Toggle(int id)
    {
        var rule = await service.ToggleAsync(id);
        return rule is null ? NotFound() : Ok(rule);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
