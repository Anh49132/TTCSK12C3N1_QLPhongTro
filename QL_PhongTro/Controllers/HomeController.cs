using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Data;
using QL_PhongTro.Models;
using QL_PhongTro.Services;
using QL_PhongTro.ViewModels;

namespace QL_PhongTro.Controllers;

public class HomeController(
    RegistrationSettings settings,
    AppDbContext? db = null,
    YeuCauThueService? requests = null,
    TinDangExpirationService? expiration = null,
    TienDoChiSoService? meterProgress = null,
    ITimeProvider? clock = null) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["RegistrationEnabled"] = settings.EnableDuplicateCheck;
        var model = new HomeViewModel
        {
            RegistrationEnabled = settings.EnableDuplicateCheck
        };

        if (db != null && requests != null)
        {
            try
            {
                bool hasTinDang = false;
                await db.Database.OpenConnectionAsync();
                try
                {
                    using var command = db.Database.GetDbConnection().CreateCommand();
                    command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'tin_dang'";
                    hasTinDang = Convert.ToInt64(await command.ExecuteScalarAsync()) == 1;
                }
                finally
                {
                    await db.Database.CloseConnectionAsync();
                }

                if (hasTinDang)
                {
                    if (expiration != null)
                    {
                        await expiration.ExpireAsync();
                    }
                    model.QuanHuyens = await db.ToaNhas.AsNoTracking()
                        .Where(t => t.QuanHuyen != null && t.QuanHuyen.Trim() != "")
                        .Select(t => t.QuanHuyen!.Trim())
                        .Distinct()
                        .OrderBy(q => q)
                        .ToListAsync();

                    var query = from post in requests.PublicListings()
                                join room in db.PhongTros.AsNoTracking() on post.PhongId equals room.Id
                                join building in db.ToaNhas.AsNoTracking() on room.ToaNhaId equals building.Id
                                orderby post.NgayDang descending, post.Id descending
                                select new
                                {
                                    post.Id,
                                    post.TieuDe,
                                    post.PhongId,
                                    room.GiaThue,
                                    room.DienTich,
                                    room.SoNguoiToiDa,
                                    building.TenToaNha,
                                    building.PhuongXa,
                                    building.QuanHuyen,
                                    building.DiaChi
                                };

                    var items = await query.Take(6).ToListAsync();
                    var phongIds = items.Select(x => x.PhongId).Distinct().ToList();

                    var images = await db.AnhPhongs.AsNoTracking()
                        .Where(a => phongIds.Contains(a.PhongId))
                        .OrderBy(a => a.ThuTu)
                        .ToListAsync();

                    var imageMap = images
                        .GroupBy(a => a.PhongId)
                        .ToDictionary(g => g.Key, g => g.First().DuongDanAnhNho);

                    model.TinMoiNhat = items.Select(x => new HomeTinDangItemViewModel
                    {
                        Id = x.Id,
                        TieuDe = x.TieuDe,
                        GiaThue = x.GiaThue,
                        DienTich = x.DienTich,
                        SoNguoiToiDa = x.SoNguoiToiDa,
                        TenToaNha = x.TenToaNha,
                        PhuongXa = x.PhuongXa,
                        QuanHuyen = x.QuanHuyen,
                        DiaChi = x.DiaChi,
                        AnhDaiDien = imageMap.GetValueOrDefault(x.PhongId)
                    }).ToList();

                    if (User.Identity?.IsAuthenticated == true &&
                        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var accountId))
                    {
                        var role = User.FindFirstValue(ClaimTypes.Role);
                        var visibleRooms = db.PhongTros.AsNoTracking().Where(room => db.ToaNhas.Any(building =>
                            building.Id == room.ToaNhaId && (role == "ADMIN" ||
                            (role == "CHU_NHA" && building.ChuNhaId == accountId) ||
                            (role == "QUAN_LY" && building.QuanLyId == accountId))));
                        model.Dashboard.Rooms = await (from room in visibleRooms
                            join building in db.ToaNhas on room.ToaNhaId equals building.Id
                            orderby room.MaPhong
                            select new HomeRoomItem(room.Id, room.MaPhong, building.TenToaNha, room.TrangThai, room.GiaThue)).Take(6).ToListAsync();
                        model.Dashboard.Requests = await (from request in db.YeuCauThues.AsNoTracking()
                            join tenant in db.KhachThues on request.KhachThueId equals tenant.Id
                            join listing in db.TinDangs on request.TinDangId equals listing.Id
                            join room in db.PhongTros on listing.PhongId equals room.Id
                            where role == "KHACH_THUE" ? tenant.TaiKhoanId == accountId : visibleRooms.Any(x => x.Id == room.Id)
                            orderby request.NgayTao descending
                            select new HomeRequestItem(request.Id, request.MaYeuCau, room.MaPhong, tenant.HoTen, request.TrangThai, request.NgayTao)).Take(5).ToListAsync();
                        if (role == "ADMIN")
                        {
                            var roleCounts = await db.TaiKhoans.Where(x => !x.IsDeleted).GroupBy(x => x.VaiTro)
                                .Select(x => new { Role = x.Key, Count = x.Count() }).ToListAsync();
                            model.Dashboard.Roles = roleCounts.Select(x => new HomeRoleItem(x.Role switch {
                                "KHACH_THUE" => "Khách thuê", "CHU_NHA" => "Chủ nhà", "QUAN_LY" => "Quản lý", _ => "Admin" }, x.Count)).ToList();
                            model.Dashboard.Activities = await db.NhatKyHoatDongs.AsNoTracking().OrderByDescending(x => x.ThoiDiem).Take(6).ToListAsync();
                            model.Dashboard.TongTaiKhoan = await db.TaiKhoans.CountAsync(x => x.DangHoatDong && !x.IsDeleted);
                            model.Dashboard.TongChuNha = await db.TaiKhoans.CountAsync(x => x.DangHoatDong && !x.IsDeleted && x.VaiTro == "CHU_NHA");
                            model.Dashboard.TongToaNha = await db.ToaNhas.CountAsync(x => x.DangHoatDong);
                            model.Dashboard.TongPhong = await db.PhongTros.CountAsync();
                            model.Dashboard.YeuCauChoXuLy = await db.YeuCauThues.CountAsync(x =>
                                x.TrangThai == TrangThaiYeuCau.Moi || x.TrangThai == TrangThaiYeuCau.DaHenLich);
                        }
                        else if (role is "CHU_NHA" or "QUAN_LY")
                        {
                            var buildingIds = await db.ToaNhas.AsNoTracking()
                                .Where(x => x.DangHoatDong && (role == "CHU_NHA" ? x.ChuNhaId == accountId : x.QuanLyId == accountId))
                                .Select(x => x.Id).ToListAsync();
                            var rooms = db.PhongTros.AsNoTracking().Where(x => buildingIds.Contains(x.ToaNhaId));
                            model.Dashboard.TongToaNha = buildingIds.Count;
                            model.Dashboard.TongPhong = await rooms.CountAsync();
                            model.Dashboard.PhongTrong = await rooms.CountAsync(x => x.TrangThai == "TRONG");
                            model.Dashboard.PhongDangThue = await rooms.CountAsync(x => x.TrangThai == "DANG_THUE");
                            if (role == "CHU_NHA")
                            {
                                model.Dashboard.PhongDatCoc = await rooms.CountAsync(x => x.TrangThai == "DA_DAT_COC");
                                model.Dashboard.Buildings = await db.ToaNhas.AsNoTracking()
                                    .Where(x => buildingIds.Contains(x.Id))
                                    .OrderBy(x => x.TenToaNha)
                                    .Select(x => new HomeBuildingItem(x.Id, x.TenToaNha, x.DiaChi,
                                        db.PhongTros.Count(r => r.ToaNhaId == x.Id),
                                        db.PhongTros.Count(r => r.ToaNhaId == x.Id && r.TrangThai == "DANG_THUE")))
                                    .ToListAsync();
                            }
                            model.Dashboard.YeuCauChoXuLy = await (from request in db.YeuCauThues.AsNoTracking()
                                join listing in db.TinDangs.AsNoTracking() on request.TinDangId equals listing.Id
                                join room in rooms on listing.PhongId equals room.Id
                                where request.TrangThai == TrangThaiYeuCau.Moi || request.TrangThai == TrangThaiYeuCau.DaHenLich
                                select request.Id).CountAsync();
                        }
                        else if (role == "KHACH_THUE")
                        {
                            model.Dashboard.YeuCauCuaToi = await (from request in db.YeuCauThues.AsNoTracking()
                                join tenant in db.KhachThues.AsNoTracking() on request.KhachThueId equals tenant.Id
                                where tenant.TaiKhoanId == accountId
                                select request.Id).CountAsync();
                            model.Dashboard.YeuCauChoXuLy = await (from request in db.YeuCauThues.AsNoTracking()
                                join tenant in db.KhachThues.AsNoTracking() on request.KhachThueId equals tenant.Id
                                where tenant.TaiKhoanId == accountId &&
                                    (request.TrangThai == TrangThaiYeuCau.Moi || request.TrangThai == TrangThaiYeuCau.DaHenLich)
                                select request.Id).CountAsync();
                        }
                    }
                }
            }
            catch
            {
                // Fallback nếu schema CSDL chưa sẵn sàng trong các môi trường kiểm thử tối giản
            }
        }

        if (meterProgress != null && clock != null && User.IsInRole("CHU_NHA")
            && int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var ownerId))
        {
            var today = DateOnly.FromDateTime(clock.UtcNow.AddHours(7));
            model.Dashboard.MeterAlerts = await meterProgress.LayCanhBaoAsync(ownerId, today, HttpContext.RequestAborted);
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult GioiThieu() => View();

    public IActionResult Privacy() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    [QL_PhongTro.Authorization.ModuleAccess("TAI_KHOAN", write: true)]
    public IActionResult ToggleRegistration()
    {
        settings.EnableDuplicateCheck = !settings.EnableDuplicateCheck;
        TempData["Msg"] = settings.EnableDuplicateCheck ? "Duplicate check enabled" : "Duplicate check disabled";
        return RedirectToAction("Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
    });
}
