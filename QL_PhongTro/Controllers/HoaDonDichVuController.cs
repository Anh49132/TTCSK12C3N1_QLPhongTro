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

[Authorize(Roles = "CHU_NHA,ADMIN"), ModuleAccess("TAI_CHINH")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class HoaDonDichVuController(AppDbContext db, DichVuService services, HoaDonDichVuService invoices) : Controller
{
    private int AccountId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    [HttpGet, Authorize(Roles = "CHU_NHA")]
    public async Task<IActionResult> Monthly(int? toaNhaId, int? nam, int? thang)
    {
        var buildings = await db.ToaNhas.AsNoTracking().Where(x => x.ChuNhaId == AccountId && x.DangHoatDong)
            .OrderBy(x => x.TenToaNha).Select(x => new SelectListItem(x.TenToaNha, x.Id.ToString())).ToListAsync();
        var today = DateOnly.FromDateTime(HttpContext.RequestServices.GetRequiredService<ITimeProvider>().UtcNow.AddHours(7));
        var selected = toaNhaId ?? (buildings.Count > 0 ? int.Parse(buildings[0].Value) : 0);
        var model = new PhatHanhThangViewModel { Nam = nam ?? today.Year, Thang = thang ?? today.Month };
        if (selected != 0)
        {
            try { model = await invoices.XemThangAsync(AccountId, selected, model.Nam, model.Thang); }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (InvalidOperationException ex) { ModelState.AddModelError("", ex.Message); }
        }
        model.ToaNhas = buildings;
        return View(model);
    }

    [HttpPost, Authorize(Roles = "CHU_NHA"), ValidateAntiForgeryToken, ModuleAccess("TAI_CHINH", write: true)]
    public async Task<IActionResult> IssueMonthly(int toaNhaId, int nam, int thang, int? hopDongId)
    {
        if (!ModelState.IsValid) return BadRequest();
        try
        {
            var count = await invoices.PhatHanhThangAsync(AccountId, toaNhaId, nam, thang, hopDongId);
            TempData["MonthlyMessage"] = count > 0 ? $"Đã phát hành {count} hóa đơn. Chỉ số điện nước đã được khóa." : "Không có phòng đủ dữ liệu để phát hành hóa đơn mới.";
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { TempData["MonthlyError"] = ex.Message; }
        catch (OverflowException) { TempData["MonthlyError"] = "Số tiền vượt giới hạn. Hãy kiểm tra chỉ số và đơn giá."; }
        catch (DbUpdateException) { TempData["MonthlyError"] = "Dữ liệu đã thay đổi. Hãy kiểm tra lại kỳ hóa đơn."; }
        catch (Microsoft.Data.Sqlite.SqliteException) { TempData["MonthlyError"] = "Chưa thể phát hành hóa đơn. Hãy kiểm tra thiết lập hoặc thử lại."; }
        return RedirectToAction(nameof(Monthly), new { toaNhaId, nam, thang });
    }

    private async Task<ChiTietHoaDonViewModel> InvoiceViewAsync(QL_PhongTro.Models.HoaDon invoice, bool preview)
    {
        var context = await (from h in db.HopDongs.AsNoTracking()
            join p in db.PhongTros on h.PhongId equals p.Id
            join t in db.ToaNhas on p.ToaNhaId equals t.Id
            where h.Id == invoice.HopDongId
            select new { p.MaPhong, p.ToaNhaId, t.TenToaNha, h.KhachDungTenId }).SingleAsync();
        var tenant = await db.KhachThues.AsNoTracking().Where(x => x.Id == context.KhachDungTenId).Select(x => x.HoTen).SingleOrDefaultAsync();
        return new() { HoaDon = invoice, XemTruoc = preview, ToaNhaId = context.ToaNhaId,
            MaPhong = context.MaPhong, TenToaNha = context.TenToaNha, TenKhach = tenant ?? "Chưa có thông tin" };
    }

    [HttpGet, Authorize(Roles = "CHU_NHA")]
    public async Task<IActionResult> Preview(int toaNhaId, int hopDongId, int nam, int thang)
    {
        if (!ModelState.IsValid) return BadRequest();
        var belongs = await (from h in db.HopDongs join p in db.PhongTros on h.PhongId equals p.Id
            where h.Id == hopDongId && p.ToaNhaId == toaNhaId select h.Id).AnyAsync();
        if (!belongs || !await services.SoHuuToaNhaAsync(AccountId, toaNhaId)) return Forbid();
        try
        {
            var preview = await invoices.XemThangAsync(AccountId, toaNhaId, nam, thang);
            var row = preview.DuKien.SingleOrDefault(x => x.HoaDon.HopDongId == hopDongId);
            if (row is null)
            {
                TempData["MonthlyError"] = "Phòng chưa đủ dữ liệu hoặc đã có hóa đơn trong kỳ. Hãy kiểm tra lại danh sách.";
                return RedirectToAction(nameof(Monthly), new { toaNhaId, nam, thang });
            }
            return View("Details", await InvoiceViewAsync(row.HoaDon, true));
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { TempData["MonthlyError"] = ex.Message; return RedirectToAction(nameof(Monthly), new { toaNhaId, nam, thang }); }
    }

    private async Task FillAsync(LapHoaDonDichVuViewModel model)
    {
        model.SanSang = await invoices.SanSangAsync();
        if (!model.SanSang) return;
        model.HopDongs = await (from h in db.HopDongs
                                join p in db.PhongTros on h.PhongId equals p.Id
                                where p.ToaNhaId == model.ToaNhaId && h.TrangThai == "DANG_HIEU_LUC"
                                orderby p.MaPhong
                                select new SelectListItem(h.MaHopDong + " · " + p.MaPhong, h.Id.ToString())).ToListAsync();
        var date = model.NgayApDung ?? DichVuService.HomNay();
        model.DonGias.Clear();
        model.HopDongId ??= model.HopDongs.Count > 0 ? int.Parse(model.HopDongs[0].Value) : null;
        var roomId = await (from h in db.HopDongs
                            join p in db.PhongTros on h.PhongId equals p.Id
                            where h.Id == model.HopDongId && p.ToaNhaId == model.ToaNhaId && h.TrangThai == "DANG_HIEU_LUC"
                            select (int?)p.Id).SingleOrDefaultAsync();
        if (roomId.HasValue && model.HopDongId.HasValue)
        {
            try
            {
                var count = await invoices.LaySoNguoiAsync(AccountId, model.HopDongId.Value, model.ToaNhaId, date);
                model.SoNguoi = count.SoNguoi; model.NgayChotSoNguoi = count.NgayChot; date = count.NgayChot;
                model.PhienBanPhong ??= count.PhienBanPhong;
            }
            catch (InvalidOperationException ex) { ModelState.AddModelError("", ex.Message); model.SoNguoi = 0; }
        }
        var contractServices = await db.HopDongDichVus.Where(x => x.HopDongId == model.HopDongId).Select(x => x.DichVuId).ToListAsync();
        foreach (var id in (await services.DanhSachAsync(AccountId, model.ToaNhaId)).Select(x => x.DichVuId).Distinct()
            .Where(id => contractServices.Count == 0 || contractServices.Contains(id)))
        {
            var price = roomId.HasValue ? await new DichVuPhongService(db, services).LayGiaHoaDonAsync(AccountId, roomId.Value, id, date) : null;
            if (price is not null) model.DonGias.Add(price);
        }
        model.DaPhatHanh = await (from hd in db.HoaDons
                                  join h in db.HopDongs on hd.HopDongId equals h.Id
                                  join p in db.PhongTros on h.PhongId equals p.Id
                                  join t in db.ToaNhas on p.ToaNhaId equals t.Id
                                  where p.ToaNhaId == model.ToaNhaId && (t.ChuNhaId == AccountId || User.IsInRole("ADMIN"))
                                  orderby hd.Id descending
                                  select new HoaDonGanDayViewModel
                                  {
                                      Id = hd.Id,
                                      MaHoaDon = hd.MaHoaDon,
                                      MaPhong = p.MaPhong,
                                      TenToaNha = t.TenToaNha,
                                      Thang = hd.Thang,
                                      Nam = hd.Nam,
                                      NgayChot = hd.NgayChot,
                                      TongTien = hd.TongTien
                                  }).Take(30).ToListAsync();
    }

    [HttpGet]
    public async Task<IActionResult> Index(int toaNhaId, DateOnly? ngayApDung, int? hopDongId)
    {
        if (!await services.SoHuuToaNhaAsync(AccountId, toaNhaId)) return Forbid();
        var model = new LapHoaDonDichVuViewModel { ToaNhaId = toaNhaId, HopDongId = hopDongId, NgayApDung = ngayApDung ?? DichVuService.HomNay() };
        await FillAsync(model);
        model.Dong = model.DonGias.Select(x => new DongDichVuInput { Chon = true, DichVuId = x.DichVuId, CauHinhId = x.CauHinhId, DonGiaDaXem = x.DonGia }).ToList();
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken, ModuleAccess("TAI_CHINH", write: true)]
    public async Task<IActionResult> Issue(LapHoaDonDichVuViewModel model)
    {
        ModelState.Remove(nameof(model.SoNguoi));
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
        catch (Microsoft.Data.Sqlite.SqliteException) { ModelState.AddModelError("", "Dữ liệu đang được cập nhật. Hãy tải lại bản xem trước và thử phát hành lại."); }
        // Display refreshed prices but require a new GET before a stale price submission can be accepted.
        await FillAsync(model);
        return View("Index", model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var context = await (from hd in db.HoaDons
                             join h in db.HopDongs on hd.HopDongId equals h.Id
                             join p in db.PhongTros on h.PhongId equals p.Id
                             where hd.Id == id
                             select new { p.ToaNhaId }).SingleOrDefaultAsync();
        if (context is null) return NotFound();
        if (!await services.SoHuuToaNhaAsync(AccountId, context.ToaNhaId)) return Forbid();
        ViewData["ToaNhaId"] = context.ToaNhaId;
        return View(await InvoiceViewAsync(await db.HoaDons.AsNoTracking().Include(x => x.ChiTiet).SingleAsync(x => x.Id == id), false));
    }
}
