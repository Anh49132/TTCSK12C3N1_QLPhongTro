using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QL_PhongTro.Authorization;
using QL_PhongTro.Services;

namespace QL_PhongTro.Controllers;

[Authorize(Roles = "CHU_NHA"), ModuleAccess("DIEN_NUOC")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class TienDoChiSoController(TienDoChiSoService progress, ITimeProvider clock) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(int? nam, int? thang, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest();
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var ownerId)) return Forbid();

        var today = DateOnly.FromDateTime(clock.UtcNow.AddHours(7));
        var year = nam ?? today.Year;
        var month = thang ?? today.Month;
        try { return View(await progress.XemAsync(ownerId, year, month, ct)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ArgumentOutOfRangeException ex) { return BadRequest(ex.Message); }
    }
}
