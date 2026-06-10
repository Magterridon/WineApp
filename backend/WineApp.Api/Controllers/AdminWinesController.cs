using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WineApp.Api.DTOs;
using WineApp.Api.Services;

namespace WineApp.Api.Controllers;

/// <summary>
/// Admin-only endpoints for wine catalog curation.
/// All routes require the Admin role.
/// </summary>
[ApiController]
[Route("api/admin/wines")]
[Authorize(Roles = "Admin")]
public class AdminWinesController(AdminWineService adminWines) : ControllerBase
{
    // ── List with filtering / sorting / pagination ────────────────────────────

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] AdminWinesQuery query)
    {
        var result = await adminWines.GetAdminWinesAsync(query);
        return Ok(result);
    }

    // ── Image file upload ─────────────────────────────────────────────────────

    [HttpPost("upload-image")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file provided" });

        try
        {
            var url = await adminWines.UploadImageAsync(file, Request);
            return Ok(new UploadImageResult { ImageUrl = url });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── Bulk image update ─────────────────────────────────────────────────────

    [HttpPost("bulk-image")]
    public async Task<IActionResult> BulkSetImage([FromBody] BulkImageRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(request.ImageUrl))
            return BadRequest(new { message = "ImageUrl is required" });

        var updated = await adminWines.BulkSetImageAsync(request);
        return Ok(new { updated, message = $"Image updated for {updated} wine(s)" });
    }

    // ── Bulk pairing assignment ───────────────────────────────────────────────

    [HttpPost("bulk-pairings")]
    public async Task<IActionResult> BulkAssignPairings([FromBody] BulkPairingRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await adminWines.BulkAssignPairingsAsync(request);
        return Ok(new
        {
            result.Created,
            result.Skipped,
            message = $"Created {result.Created} pairing(s), {result.Skipped} already existed"
        });
    }
}
