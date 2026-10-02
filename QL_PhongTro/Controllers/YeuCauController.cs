using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Mvc.Rendering; using Microsoft.EntityFrameworkCore; using QL_PhongTro.Authorization; using QL_PhongTro.Data; using QL_PhongTro.Models; using QL_PhongTro.ViewModels;
namespace QL_PhongTro.Controllers;
[Authorize(Roles = "CHU_NHA,QUAN_LY,ADMIN")][ModuleAccess("PHONG_TRO")][ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class YeuCauController(AppDbContext db) : Controller
{
    [HttpGet] public async Task<IActionResult> Index(string? trangThai, int? toaNhaId)
    {
        var accountId = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0; if (accountId == 0) return Forbid();
        var buildings = await db.ToaNhas.AsNoTracking().Where(x => x.ChuNhaId == accountId && x.DangHoatDong).OrderBy(x => x.TenToaNha).Select(x => new SelectListItem(x.TenToaNha, x.Id.ToString())).ToListAsync();
        var validBuilding = toaNhaId.HasValue && buildings.Any(x => x.Value == toaNhaId.Value.ToString()) ? toaNhaId : null;
        var validStatus = Models.TrangThaiYeuCau.Labels.ContainsKey(trangThai ?? "") ? trangThai : null;
        var query = from request in db.YeuCaus.AsNoTracking() join building in db.ToaNhas on request.ToaNhaId equals building.Id join room in db.PhongTros on request.PhongId equals room.Id join tenant in db.KhachThues on request.KhachThueId equals tenant.Id join account in db.TaiKhoans on tenant.TaiKhoanId equals account.Id into accounts from account in accounts.DefaultIfEmpty() where building.ChuNhaId == accountId && (!validBuilding.HasValue || request.ToaNhaId == validBuilding) && (validStatus == null || request.TrangThai == validStatus) orderby request.NgayTao descending, request.Id descending select new YeuCauListItemViewModel { MaYeuCau = request.MaYeuCau, TenKhach = tenant.HoTen, SoDienThoai = tenant.SoDienThoai ?? account.SoDienThoai, MaPhong = room.MaPhong, LoaiYeuCau = request.LoaiYeuCau, NgayMongMuon = request.NgayMongMuon, TrangThai = request.TrangThai, NgayTao = request.NgayTao };
        return View(new DanhSachYeuCauViewModel { YeuCaus = await query.ToListAsync(), TrangThai = validStatus, ToaNhaId = validBuilding, ToaNhaOptions = buildings });
    }
}
