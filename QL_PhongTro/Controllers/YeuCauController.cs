using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Mvc.Rendering; using Microsoft.EntityFrameworkCore; using QL_PhongTro.Authorization; using QL_PhongTro.Data; using QL_PhongTro.Models; using QL_PhongTro.Services; using QL_PhongTro.ViewModels;
namespace QL_PhongTro.Controllers;
[Authorize(Roles = "CHU_NHA,QUAN_LY,ADMIN,KHACH_THUE")][ModuleAccess("YEU_CAU_THUE")][ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class YeuCauController(AppDbContext db, ITimeProvider clock, YeuCauThueService requests, LichHenService appointments) : Controller
{
    [HttpGet] public async Task<IActionResult> Index(string? trangThai, int? toaNhaId)
    {
        var accountId = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0; if (accountId == 0) return Forbid();
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        var laKhachThue = role == LichHenTrangThai.VaiTroKhachThue;
        var visibleBuildings = db.ToaNhas.AsNoTracking().Where(x => x.DangHoatDong &&
            (role == "ADMIN" || x.ChuNhaId == accountId || (role == "QUAN_LY" && x.QuanLyId == accountId)));
        var buildings = await visibleBuildings.OrderBy(x => x.TenToaNha).Select(x => new SelectListItem(x.TenToaNha, x.Id.ToString())).ToListAsync();
        var validBuilding = toaNhaId.HasValue && buildings.Any(x => x.Value == toaNhaId.Value.ToString()) ? toaNhaId : null;
        var validStatus = Models.TrangThaiYeuCau.Labels.ContainsKey(trangThai ?? "") ? trangThai : null;
        if (!await requests.IsInstalled())
            return View(new DanhSachYeuCauViewModel { LaKhachThue = laKhachThue, TrangThai = validStatus, ToaNhaId = validBuilding, ToaNhaOptions = buildings });
        if (laKhachThue)
        {
            var tenantRequests = await (from request in db.YeuCauThues.AsNoTracking()
                                        join tenant in db.KhachThues.AsNoTracking() on request.KhachThueId equals tenant.Id
                                        join listing in db.TinDangs.AsNoTracking() on request.TinDangId equals listing.Id
                                        join room in db.PhongTros.AsNoTracking() on listing.PhongId equals room.Id
                                        where tenant.TaiKhoanId == accountId
                                            && (validStatus == null || request.TrangThai == validStatus)
                                        orderby request.NgayTao descending, request.Id descending
                                        select new
                                        {
                                            request.Id,
                                            request.MaYeuCau,
                                            request.TinDangId,
                                            room.MaPhong,
                                            request.NgayTao,
                                            request.TrangThai,
                                            request.LichHen,
                                            request.LyDoTuChoi
                                        }).ToListAsync();

            var tenantListingIds = tenantRequests.Select(request => request.TinDangId).Distinct().ToArray();
            var availableTenantListingIds = tenantListingIds.Length == 0
                ? new HashSet<int>()
                : await requests.PublicListings()
                    .Where(listing => tenantListingIds.Contains(listing.Id))
                    .Select(listing => listing.Id)
                    .ToHashSetAsync();

            var tenantItems = tenantRequests.Select(request => new YeuCauThueItemViewModel(
                request.Id,
                request.MaYeuCau,
                request.TinDangId,
                availableTenantListingIds.Contains(request.TinDangId),
                request.MaPhong,
                request.NgayTao,
                request.TrangThai,
                TrangThaiYeuCau.Labels.GetValueOrDefault(request.TrangThai, request.TrangThai),
                request.LichHen,
                request.TrangThai == LichHenTrangThai.TuChoi
                    ? string.IsNullOrWhiteSpace(request.LyDoTuChoi)
                        ? "Không có lý do"
                        : LichHenService.LyDoLabel(request.LyDoTuChoi)
                    : null)).ToList();

            return View(new DanhSachYeuCauViewModel
            {
                YeuCausCuaKhach = tenantItems,
                LaKhachThue = true,
                TrangThai = validStatus,
                ToaNhaOptions = buildings
            });
        }
        var buildingIds = buildings.Select(x => int.Parse(x.Value)).ToArray();
        var query = from request in db.YeuCauThues.AsNoTracking()
                    join listing in db.TinDangs.AsNoTracking() on request.TinDangId equals listing.Id
                    join room in db.PhongTros.AsNoTracking() on listing.PhongId equals room.Id
                    join tenant in db.KhachThues.AsNoTracking() on request.KhachThueId equals tenant.Id
                    join account in db.TaiKhoans.AsNoTracking() on tenant.TaiKhoanId equals account.Id into accounts
                    from account in accounts.DefaultIfEmpty()
                    where (laKhachThue ? tenant.TaiKhoanId == accountId : buildingIds.Contains(room.ToaNhaId))
                        && (laKhachThue || !validBuilding.HasValue || room.ToaNhaId == validBuilding)
                        && (validStatus == null || request.TrangThai == validStatus)
                    orderby request.NgayTao descending, request.Id descending
                    select new YeuCauListItemViewModel
                    {
                        Id = request.Id,
                        MaYeuCau = request.MaYeuCau,
                        TenKhach = tenant.HoTen,
                        SoDienThoai = tenant.SoDienThoai ?? (account == null ? "" : account.SoDienThoai),
                        MaPhong = room.MaPhong,
                        LoaiYeuCau = request.LoaiYeuCau,
                        NgayMongMuon = request.NgayMongMuon,
                        TrangThai = request.TrangThai,
                        NgayTao = request.NgayTao
                    };
        var items = await query.ToListAsync();
        var nowUtc = clock.UtcNow;
        foreach (var item in items)
        {
            var createdUtc = item.NgayTao.Kind == DateTimeKind.Utc ? item.NgayTao : DateTime.SpecifyKind(item.NgayTao, DateTimeKind.Utc);
            item.QuaHanChuaXuLy = TrangThaiYeuCau.ChuaXuLy(item.TrangThai) && nowUtc - createdUtc >= TimeSpan.FromHours(24);
        }
        return View(new DanhSachYeuCauViewModel { YeuCaus = items, LaKhachThue = laKhachThue, TrangThai = validStatus, ToaNhaId = validBuilding, ToaNhaOptions = buildings });
    }

    [HttpGet("/YeuCau/MoTinDang/{id:int}"), Authorize(Roles = "KHACH_THUE")]
    public async Task<IActionResult> MoTinDang(int id)
    {
        var accountId = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var parsedId) ? parsedId : 0;
        if (accountId == 0) return Forbid();

        var listingId = await (from request in db.YeuCauThues.AsNoTracking()
                               join tenant in db.KhachThues.AsNoTracking() on request.KhachThueId equals tenant.Id
                               where request.Id == id && tenant.TaiKhoanId == accountId
                               select (int?)request.TinDangId).SingleOrDefaultAsync();
        if (listingId is null) return NotFound();

        if (!await requests.PublicListings().AnyAsync(listing => listing.Id == listingId.Value))
        {
            TempData["Error"] = "Tin đăng này hiện không thể xem hoặc không còn khả dụng.";
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction("ChiTiet", "TinDang", new { id = listingId.Value });
    }

    [HttpPost("/YeuCau/Huy/{id:int}"), Authorize(Roles = "KHACH_THUE"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Huy(int id, CancellationToken ct)
    {
        var accountId = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var parsedId) ? parsedId : 0;
        if (accountId == 0) return Forbid();

        try
        {
            await appointments.HuyAsync(id, accountId, ct);
            TempData["Success"] = "Đã huỷ yêu cầu.";
        }
        catch (Exception ex) when (ex is KeyNotFoundException or UnauthorizedAccessException)
        {
            TempData["Error"] = "Không thể huỷ yêu cầu này. Vui lòng tải lại danh sách.";
        }
        catch (Exception ex) when (ex is InvalidOperationException or LichHenConflictException)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
