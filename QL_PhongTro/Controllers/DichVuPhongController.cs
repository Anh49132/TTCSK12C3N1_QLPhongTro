using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Authorization;
using QL_PhongTro.Services;

namespace QL_PhongTro.Controllers;

[Authorize(Roles = "CHU_NHA")]
[ModuleAccess("PHONG_TRO")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class DichVuPhongController(DichVuPhongService services) : Controller
{
    private int AccountId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> Index(int phongId)
    {
        try { return View(await services.XemAsync(AccountId, phongId)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPost, ValidateAntiForgeryToken, ModuleAccess("PHONG_TRO", write: true)]
    public async Task<IActionResult> Set(int phongId, int dichVuToaNhaId, bool enabled)
    {
        if (!ModelState.IsValid) return BadRequest();
        try
        {
            await services.DatDichVuAsync(AccountId, phongId, dichVuToaNhaId, enabled);
            TempData["Success"] = "Đã cập nhật dịch vụ của phòng.";
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (DbUpdateException) { TempData["Error"] = "Dịch vụ vừa thay đổi. Vui lòng tải lại và thử lại."; }
        return RedirectToAction(nameof(Index), new { phongId });
    }
}
