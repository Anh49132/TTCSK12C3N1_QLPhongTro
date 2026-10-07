using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QL_PhongTro.Authorization;
using QL_PhongTro.Services;

namespace QL_PhongTro.Controllers;

[Authorize(Roles = "QUAN_LY"), ModuleAccess("DIEN_NUOC")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class ChiSoDienNuocController(ChiSoDienNuocService readings) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(int? toaNhaId, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest();
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var accountId)) return Forbid();
        try { return View(await readings.DanhSachAsync(accountId, toaNhaId, ct)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }
}
