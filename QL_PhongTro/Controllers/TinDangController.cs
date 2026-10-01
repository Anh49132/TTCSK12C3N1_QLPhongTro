using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Data;
using QL_PhongTro.Services;
using QL_PhongTro.ViewModels;

namespace QL_PhongTro.Controllers;

public class TinDangController(AppDbContext db, YeuCauThueService requests) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!await requests.IsInstalled()) return View("ChuaCaiDat");
        return View(await requests.PublicListings().OrderByDescending(t => t.NgayDang).Take(100).ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> ChiTiet(int id)
    {
        if (!await requests.IsInstalled()) return View("ChuaCaiDat");
        var model = await Detail(id, new());
        return model is null ? NotFound() : View(model);
    }

    [Authorize(Roles = "KHACH_THUE"), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> GuiYeuCau(int id, [Bind(Prefix = "Form")] GuiYeuCauViewModel form)
    {
        if (!await requests.IsInstalled()) return View("ChuaCaiDat");
        var model = await Detail(id, form);
        if (model is null) return NotFound();
        if (!ModelState.IsValid) return View("ChiTiet", model);
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var accountId)) return Forbid();
        try
        {
            var request = await requests.Send(id, accountId, form);
            return request is null ? NotFound() : RedirectToAction(nameof(ThanhCong), new { id = request.Id });
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (RequestCodeExhaustedException error)
        {
            ModelState.AddModelError("", error.Message);
            return View("ChiTiet", model);
        }
    }

    [Authorize(Roles = "KHACH_THUE"), HttpGet]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> ThanhCong(int id)
    {
        if (!await requests.IsInstalled()) return NotFound();
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var accountId)) return Forbid();
        var request = await db.YeuCauThues.AsNoTracking().SingleOrDefaultAsync(r => r.Id == id
            && db.KhachThues.Any(k => k.Id == r.KhachThueId && k.TaiKhoanId == accountId));
        return request is null ? NotFound() : View(request);
    }

    private async Task<ChiTietTinDangViewModel?> Detail(int id, GuiYeuCauViewModel form)
    {
        var tin = await requests.PublicListings().SingleOrDefaultAsync(t => t.Id == id);
        if (tin is null) return null;
        var room = await db.PhongTros.AsNoTracking().SingleAsync(p => p.Id == tin.PhongId);
        return new(tin, room, form);
    }
}
