using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Authorization;
using QL_PhongTro.Data;
using QL_PhongTro.Models;
using QL_PhongTro.ViewModels;
namespace QL_PhongTro.Controllers;

[Authorize(Roles = "KHACH_THUE"), ModuleAccess("TAI_CHINH")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class ThongBaoController(AppDbContext db, QL_PhongTro.Services.HoaDonDichVuService invoices) : Controller
{
    private int Actor => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    public async Task<IActionResult> Index(int page = 1)
    {
        if (!await QL_PhongTro.Services.InvoiceNotificationDispatcher.IsInstalledAsync(db)) return NotFound();
        page = Math.Max(1, page);
        var published = db.HoaDons.Where(x => x.TrangThai == "DA_PHAT_HANH" || x.TrangThai == "DA_HUY").Select(x => x.Id);
        var query = db.ThongBaoHoaDons.AsNoTracking().Where(x => x.NguoiNhanId == Actor && published.Contains(x.HoaDonId));
        var total = await query.CountAsync();
        var pages = Math.Max(1, (total + 19) / 20); page = Math.Min(page, pages);
        ViewData["Page"] = page; ViewData["Pages"] = pages;
        var rows = await query.OrderByDescending(x => x.NgayTao).ThenByDescending(x => x.Id).Skip((page - 1) * 20).Take(20).ToListAsync();
        var ids = rows.Select(x => x.HoaDonId).ToList();
        ViewData["CancelledIds"] = (await db.HoaDons.Where(x => ids.Contains(x.Id) && x.TrangThai == "DA_HUY").Select(x => x.Id).ToListAsync()).ToHashSet();
        return View(rows);
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
        var invoice = await db.HoaDons.AsNoTracking().Include(x => x.ChiTiet).SingleOrDefaultAsync(x => x.Id == id && (x.TrangThai == "DA_PHAT_HANH" || x.TrangThai == "DA_HUY"));
        if (invoice is null) return NotFound();
        var context = await (from h in db.HopDongs join p in db.PhongTros on h.PhongId equals p.Id join t in db.ToaNhas on p.ToaNhaId equals t.Id
            where h.Id == invoice.HopDongId select new { p.MaPhong, p.ToaNhaId, t.TenToaNha, h.KhachDungTenId }).SingleAsync();
        var tenant = await db.KhachThues.Where(x => x.Id == context.KhachDungTenId).Select(x => x.HoTen).SingleOrDefaultAsync();
        var model = new ChiTietHoaDonViewModel { HoaDon = invoice, KhachXem = true,
            MaPhong = context.MaPhong, ToaNhaId = context.ToaNhaId, TenToaNha = context.TenToaNha, TenKhach = tenant ?? "" };
        await invoices.FillRelationsAsync(model, Actor);
        return View("~/Views/HoaDonDichVu/Details.cshtml", model);
    }

    [HttpGet]
    public IActionResult ChiTiet(string maHoaDon)
    {
        if (string.IsNullOrWhiteSpace(maHoaDon)) return NotFound();
        return View(new ChiTietHoaDonKhachPageViewModel(maHoaDon));
    }

    [HttpGet]
    public async Task<IActionResult> ChiTietDuLieu(string maHoaDon, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(maHoaDon)
            || !await QL_PhongTro.Services.InvoiceNotificationDispatcher.IsInstalledAsync(db, cancellationToken))
            return NotFound(new { message = "Không tìm thấy hóa đơn." });

        var invoice = await (from bill in db.HoaDons.AsNoTracking()
            join contract in db.HopDongs.AsNoTracking() on bill.HopDongId equals contract.Id
            join profile in db.KhachThues.AsNoTracking() on contract.KhachDungTenId equals profile.Id
            join room in db.PhongTros.AsNoTracking() on contract.PhongId equals room.Id
            where bill.MaHoaDon == maHoaDon && profile.TaiKhoanId == Actor
                && (bill.TrangThai == "DA_PHAT_HANH" || bill.TrangThai == "DA_HUY")
            select new { Bill = bill, room.MaPhong }).SingleOrDefaultAsync(cancellationToken);
        if (invoice is null) return NotFound(new { message = "Không tìm thấy hóa đơn." });

        var culture = System.Globalization.CultureInfo.GetCultureInfo("vi-VN");
        var lines = await db.ChiTietHoaDons.AsNoTracking().Where(x => x.HoaDonId == invoice.Bill.Id)
            .OrderBy(x => x.SoThuTu).Select(x => new ChiTietHoaDonKhachLineViewModel(
                x.TenKhoan,
                x.ChiSoDau,
                x.ChiSoCuoi,
                x.SoLuong,
                x.DonViTinh,
                x.LoaiKhoan,
                x.DonGia,
                x.ThanhTien))
            .ToListAsync(cancellationToken);

        return Ok(new ChiTietHoaDonKhachViewModel(
            invoice.Bill.MaHoaDon,
            invoice.Bill.Thang,
            invoice.Bill.Nam,
            invoice.MaPhong,
            lines.Select(x => new ChiTietHoaDonKhachLineResponse(
                x.TenKhoan,
                x.ChiSoDau.HasValue && x.ChiSoCuoi.HasValue ? x.ChiSoDau.Value.ToString("0.###", culture) : null,
                x.ChiSoDau.HasValue && x.ChiSoCuoi.HasValue ? x.ChiSoCuoi.Value.ToString("0.###", culture) : null,
                (x.ChiSoDau.HasValue && x.ChiSoCuoi.HasValue
                    ? x.ChiSoCuoi.Value - x.ChiSoDau.Value
                    : x.SoLuong).ToString("0.###", culture),
                x.DonViTinh ?? (x.LoaiKhoan == "TIEN_PHONG" ? "tháng" : ""),
                x.DonGia.ToString("N0", culture),
                x.ThanhTien.ToString("N0", culture))).ToArray()));
    }
}
