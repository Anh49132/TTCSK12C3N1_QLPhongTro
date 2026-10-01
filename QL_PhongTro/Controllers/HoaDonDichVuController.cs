using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Authorization;
using QL_PhongTro.Data;
using QL_PhongTro.Services;
using QL_PhongTro.ViewModels;

namespace QL_PhongTro.Controllers;

[Authorize(Roles = "CHU_NHA"), ModuleAccess("TAI_CHINH")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class HoaDonDichVuController(AppDbContext db, DichVuService services, HoaDonDichVuService invoices) : Controller
{
    private int AccountId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    private async Task FillAsync(LapHoaDonDichVuViewModel model)
    {
        model.SanSang = await invoices.SanSangAsync();
        if (!model.SanSang) return;
        model.HopDongs = await (from h in db.HopDongs join p in db.PhongTros on h.PhongId equals p.Id
            where p.ToaNhaId == model.ToaNhaId && h.TrangThai == "DANG_HIEU_LUC"
            orderby p.MaPhong select new SelectListItem(h.MaHopDong + " · " + p.MaPhong, h.Id.ToString())).ToListAsync();
        var date = model.NgayApDung ?? DichVuService.HomNay();
        foreach (var id in (await services.DanhSachAsync(AccountId, model.ToaNhaId)).Select(x => x.DichVuId).Distinct())
        {
            var price = await services.LayDonGiaAsync(AccountId, model.ToaNhaId, id, date);
            if (price is not null) model.DonGias.Add(price);
        }
        model.DaPhatHanh = await (from hd in db.HoaDons join h in db.HopDongs on hd.HopDongId equals h.Id
            join p in db.PhongTros on h.PhongId equals p.Id
            join t in db.ToaNhas on p.ToaNhaId equals t.Id
            where p.ToaNhaId == model.ToaNhaId && t.ChuNhaId == AccountId
            orderby hd.Id descending
            select new HoaDonGanDayViewModel
            {
                Id = hd.Id, MaHoaDon = hd.MaHoaDon, MaPhong = p.MaPhong, TenToaNha = t.TenToaNha,
                Thang = hd.Thang, Nam = hd.Nam, NgayChot = hd.NgayChot, TongTien = hd.TongTien
            }).Take(30).ToListAsync();
    }

    [HttpGet]
    public async Task<IActionResult> Index(int toaNhaId, DateOnly? ngayApDung)
    {
        if (!await services.SoHuuToaNhaAsync(AccountId, toaNhaId)) return Forbid();
        var model = new LapHoaDonDichVuViewModel { ToaNhaId = toaNhaId, NgayApDung = ngayApDung ?? DichVuService.HomNay() };
        await FillAsync(model);
        model.Dong = model.DonGias.Select(x => new DongDichVuInput { DichVuId = x.DichVuId, CauHinhId = x.CauHinhId, DonGiaDaXem = x.DonGia }).ToList();
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken, ModuleAccess("TAI_CHINH", write: true)]
    public async Task<IActionResult> Issue(LapHoaDonDichVuViewModel model)
    {
        if (!await services.SoHuuToaNhaAsync(AccountId, model.ToaNhaId)) return Forbid();
        try
        {
            if (ModelState.IsValid)
            {
                var id = await invoices.PhatHanhAsync(AccountId, model);
                return RedirectToAction(nameof(Details), new { id });
            }
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { ModelState.AddModelError("", ex.Message); }
        catch (ValidationException ex) { ModelState.AddModelError("", ex.Message); }
        catch (OverflowException) { ModelState.AddModelError("", "Số tiền vượt giới hạn cho phép. Hãy kiểm tra đơn giá và chỉ số."); }
        catch (DbUpdateException) { ModelState.AddModelError("", "Chưa phát hành được hóa đơn: dữ liệu vừa thay đổi hoặc đã có hóa đơn cho kỳ này. Hãy tải lại."); }
        // Display refreshed prices but require a new GET before a stale price submission can be accepted.
        await FillAsync(model);
        return View("Index", model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var context = await (from hd in db.HoaDons join h in db.HopDongs on hd.HopDongId equals h.Id
            join p in db.PhongTros on h.PhongId equals p.Id where hd.Id == id select new { p.ToaNhaId }).SingleOrDefaultAsync();
        if (context is null) return NotFound();
        if (!await services.SoHuuToaNhaAsync(AccountId, context.ToaNhaId)) return Forbid();
        ViewData["ToaNhaId"] = context.ToaNhaId;
        return View(await db.HoaDons.AsNoTracking().Include(x => x.ChiTiet).SingleAsync(x => x.Id == id));
    }
}
