using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Authorization;
using QL_PhongTro.Data;
using QL_PhongTro.ViewModels;
namespace QL_PhongTro.Controllers;

[Authorize(Roles = "KHACH_THUE"), ModuleAccess("TAI_CHINH")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class ThongBaoController(AppDbContext db) : Controller
{
    private int Actor => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    public async Task<IActionResult> Index(int page = 1)
    {
        if (!await QL_PhongTro.Services.InvoiceNotificationDispatcher.IsInstalledAsync(db)) return NotFound();
        page = Math.Max(1, page);
        var published = db.HoaDons.Where(x => x.TrangThai == "DA_PHAT_HANH").Select(x => x.Id);
        var query = db.ThongBaoHoaDons.AsNoTracking().Where(x => x.NguoiNhanId == Actor && published.Contains(x.HoaDonId));
        var total = await query.CountAsync();
        var pages = Math.Max(1, (total + 19) / 20); page = Math.Min(page, pages);
        ViewData["Page"] = page; ViewData["Pages"] = pages;
        return View(await query.OrderByDescending(x => x.NgayTao).ThenByDescending(x => x.Id).Skip((page - 1) * 20).Take(20).ToListAsync());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Read(int id)
    {
        if (!await QL_PhongTro.Services.InvoiceNotificationDispatcher.IsInstalledAsync(db)) return NotFound();
        if (!await db.ThongBaoHoaDons.AnyAsync(x => x.Id == id && x.NguoiNhanId == Actor)) return Forbid();
        var now = HttpContext.RequestServices.GetRequiredService<QL_PhongTro.Services.ITimeProvider>().UtcNow;
        await db.ThongBaoHoaDons.Where(x => x.Id == id && x.NguoiNhanId == Actor && x.NgayDoc == null)
            .ExecuteUpdateAsync(set => set.SetProperty(x => x.NgayDoc, now));
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> HoaDon(int id)
    {
        if (!await QL_PhongTro.Services.InvoiceNotificationDispatcher.IsInstalledAsync(db)) return NotFound();
        if (!await db.ThongBaoHoaDons.AnyAsync(x => x.HoaDonId == id && x.NguoiNhanId == Actor)) return Forbid();
        var invoice = await db.HoaDons.AsNoTracking().Include(x => x.ChiTiet).SingleOrDefaultAsync(x => x.Id == id && x.TrangThai == "DA_PHAT_HANH");
        if (invoice is null) return NotFound();
        var context = await (from h in db.HopDongs join p in db.PhongTros on h.PhongId equals p.Id join t in db.ToaNhas on p.ToaNhaId equals t.Id
            where h.Id == invoice.HopDongId select new { p.MaPhong, p.ToaNhaId, t.TenToaNha, h.KhachDungTenId }).SingleAsync();
        var tenant = await db.KhachThues.Where(x => x.Id == context.KhachDungTenId).Select(x => x.HoTen).SingleOrDefaultAsync();
        return View("~/Views/HoaDonDichVu/Details.cshtml", new ChiTietHoaDonViewModel { HoaDon = invoice, KhachXem = true,
            MaPhong = context.MaPhong, ToaNhaId = context.ToaNhaId, TenToaNha = context.TenToaNha, TenKhach = tenant ?? "" });
    }
}
