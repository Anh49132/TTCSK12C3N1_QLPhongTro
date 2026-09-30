using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Authorization;
using QL_PhongTro.Data;
using QL_PhongTro.Models;
using QL_PhongTro.Services;
using QL_PhongTro.ViewModels;

namespace QL_PhongTro.Controllers;

[Authorize]
[ModuleAccess("YEU_CAU_THUE")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class YeuCauThueController(AppDbContext db, YeuCauThueService services) : Controller
{
    private int AccountId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    private bool LaChuNha => User.IsInRole("CHU_NHA");

    private static SelectListItem Item(string value, string text) => new(text, value);

    /// <summary>Time entered by the user is Vietnam time; the database keeps UTC.</summary>
    private static DateTime? ChuyenUtcSangNhap(DateTime? utc) =>
        utc.HasValue ? utc.Value.AddHours(7) : null;

    [HttpGet]
    public async Task<IActionResult> Index(string? trangThai, string? loai, string? toaNhaId)
    {
        var view = new YeuCauThueIndexViewModel
        {
            TrangThai = trangThai,
            Loai = loai,
            LaChuNha = LaChuNha,
            TrangThaiOptions =
            [
                Item("", "Tất cả"), Item(YeuCauThueTrangThai.Moi, "Chờ xác nhận"),
                Item(YeuCauThueTrangThai.DaHenLich, "Đã xác nhận"), Item(YeuCauThueTrangThai.DaDuyet, "Đã duyệt"),
                Item(YeuCauThueTrangThai.TuChoi, "Đã từ chối"), Item(YeuCauThueTrangThai.DaHuy, "Đã huỷ")
            ],
            LoaiOptions =
            [
                Item("", "Tất cả"), Item(YeuCauThueTrangThai.LoaiXemPhong, "Xem phòng"),
                Item(YeuCauThueTrangThai.LoaiThueNgay, "Thuê ngay")
            ]
        };

        if (LaChuNha)
        {
            var toaNhas = await db.ToaNhas.AsNoTracking().Where(x => x.ChuNhaId == AccountId && x.DangHoatDong)
                .OrderBy(x => x.TenToaNha).Select(x => new SelectListItem(x.TenToaNha, x.Id.ToString())).ToListAsync();
            view.ToaNhas = toaNhas;
        }

        // Both roles read from the same shape; the WHERE clause is what separates them,
        // because KHACH_THUE also holds WRITE on this module.
        var query = from request in db.YeuCauThues.AsNoTracking()
                    join profile in db.KhachThues on request.KhachThueId equals profile.Id
                    join room in db.PhongTros on request.PhongId equals room.Id
                    join building in db.ToaNhas on room.ToaNhaId equals building.Id
                    where LaChuNha
                        ? building.ChuNhaId == AccountId && building.DangHoatDong
                        : db.TaiKhoans.Any(a => a.Id == AccountId
                            && a.DangHoatDong && !a.IsDeleted && a.VaiTro == "KHACH_THUE"
                            && profile.TaiKhoanId == AccountId)
                    select new { request, profile, room, building };

        if (!string.IsNullOrWhiteSpace(trangThai)) query = query.Where(x => x.request.TrangThai == trangThai);
        if (!string.IsNullOrWhiteSpace(loai)) query = query.Where(x => x.request.LoaiYeuCau == loai);
        if (!string.IsNullOrWhiteSpace(toaNhaId) && int.TryParse(toaNhaId, out var buildingId))
            query = query.Where(x => x.building.Id == buildingId);

        view.YeuCaus = await query.OrderByDescending(x => x.request.LichHen != null).ThenBy(x => x.request.LichHen)
            .ThenByDescending(x => x.request.NgayTao)
            .Select(x => new YeuCauThueListItem
            {
                Id = x.request.Id, MaYeuCau = x.request.MaYeuCau, TenPhong = x.room.MaPhong,
                TenToaNha = x.building.TenToaNha, TenKhach = x.profile.HoTen,
                // The phone lives on the linked account, not on the tenant profile.
                SoDienThoaiKhach = db.TaiKhoans.Where(a => a.Id == x.profile.TaiKhoanId)
                    .Select(a => a.SoDienThoai).FirstOrDefault() ?? string.Empty,
                LoaiYeuCau = x.request.LoaiYeuCau,
                TrangThai = x.request.TrangThai, LichHen = x.request.LichHen, NgayTao = x.request.NgayTao,
                DaDoiLich = x.request.DaDoiLich
            }).ToListAsync();
        return View(view);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var yeuCau = await db.YeuCauThues.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (yeuCau is null) return NotFound();
        var isOwner = await services.ChuNhaCuaYeuCauAsync(AccountId, id, HttpContext.RequestAborted);
        var isTenant = await services.KhachChuYeuCauAsync(AccountId, id, HttpContext.RequestAborted);
        if (!isOwner && !isTenant) return Forbid();

        var row = await (from r in db.PhongTros.AsNoTracking()
                         join b in db.ToaNhas on r.ToaNhaId equals b.Id
                         where r.Id == yeuCau.PhongId
                         select new { r, b }).FirstOrDefaultAsync();
        var khach = await db.KhachThues.AsNoTracking().FirstOrDefaultAsync(x => x.Id == yeuCau.KhachThueId);
        var chuNhaId = row is null ? 0 : row.b.ChuNhaId;
        var chuNha = await db.TaiKhoans.AsNoTracking().FirstOrDefaultAsync(x => x.Id == chuNhaId);
        var soDienThoai = khach?.TaiKhoanId is int tk
            ? await db.TaiKhoans.AsNoTracking().Where(x => x.Id == tk).Select(x => x.SoDienThoai).FirstOrDefaultAsync()
            : null;

        var view = new YeuCauThueDetailViewModel
        {
            YeuCau = yeuCau,
            TenPhong = row?.r.MaPhong ?? string.Empty,
            TenToaNha = row?.b.TenToaNha ?? string.Empty,
            DiaChiToaNha = row?.b.DiaChi ?? string.Empty,
            TenKhach = khach?.HoTen ?? string.Empty,
            SoDienThoaiKhach = soDienThoai ?? string.Empty,
            TenChuNha = chuNha?.HoTen ?? string.Empty,
            LaChuNha = isOwner,
            LyDoOptions = YeuCauThueService.DanhSachLyDoTuChoi()
                .Select(r => Item(r, YeuCauThueService.LyDoLabel(r))).ToList()
        };
        view.HienThiFormXacNhan = isOwner && yeuCau.TrangThai == YeuCauThueTrangThai.Moi;
        view.LichHenMoi = ChuyenUtcSangNhap(yeuCau.LichHen);
        return View(view);
    }

    [HttpPost, ValidateAntiForgeryToken, ModuleAccess("YEU_CAU_THUE", write: true)]
    public async Task<IActionResult> XacNhan(int id, DateTime? lichHen)
    {
        try
        {
            if (lichHen is not { } nhap) throw new InvalidOperationException("Hãy chọn ngày giờ hẹn.");
            await services.XacNhanLichAsync(id, AccountId, nhap.AddHours(-7), HttpContext.RequestAborted);
            TempData["YeuCauThueOk"] = "Đã xác nhận lịch hẹn.";
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { TempData["YeuCauThueError"] = ex.Message; }
        return RedirectToAction(nameof(Detail), new { id });
    }
}
