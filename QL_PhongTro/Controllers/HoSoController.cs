using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Data;
using QL_PhongTro.Models;
using QL_PhongTro.ViewModels;
using QL_PhongTro.Services;

namespace QL_PhongTro.Controllers;

[Authorize]
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
public class HoSoController(AppDbContext db, GiayToImageStore images, HoSoAccess access) : Controller
{
    private async Task<TaiKhoan?> CurrentViewerAsync()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)) return null;
        return await db.TaiKhoans.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.DangHoatDong);
    }
    private async Task<TaiKhoan?> CurrentTenantAsync()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
            return null;
        return await db.TaiKhoans.SingleOrDefaultAsync(x => x.Id == id && x.DangHoatDong && x.VaiTro == "KHACH_THUE");
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var tenant = await CurrentTenantAsync();
        if (tenant is null) return Forbid();
        var profile = await db.KhachThues.AsNoTracking().SingleOrDefaultAsync(x => x.TaiKhoanId == tenant.Id);
        return View(new HoSoViewModel
        {
            CoHoSo = profile is not null,
            HoTen = profile?.HoTen ?? tenant.HoTen,
            NgaySinh = profile?.NgaySinh,
            CanCuocDaLuu = HoSoAccess.Mask(profile?.SoGiayTo),
            QueQuan = profile?.QueQuan ?? string.Empty,
            NgheNghiep = profile?.NgheNghiep ?? string.Empty,
            CoAnhMatTruoc = profile?.AnhGiayToTruoc is not null,
            CoAnhMatSau = profile?.AnhGiayToSau is not null
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(16 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 16 * 1024 * 1024)]
    public async Task<IActionResult> Index(HoSoViewModel model)
    {
        var tenant = await CurrentTenantAsync();
        if (tenant is null) return Forbid();
        var profile = await db.KhachThues.SingleOrDefaultAsync(x => x.TaiKhoanId == tenant.Id);
        model.CoHoSo = profile is not null;
        model.CoAnhMatTruoc = profile?.AnhGiayToTruoc is not null;
        model.CoAnhMatSau = profile?.AnhGiayToSau is not null;
        model.CanCuocDaLuu = HoSoAccess.Mask(profile?.SoGiayTo);
        if (string.IsNullOrEmpty(model.SoCanCuoc) && string.IsNullOrEmpty(profile?.SoGiayTo))
            ModelState.AddModelError(nameof(model.SoCanCuoc), "Vui lòng nhập số căn cước.");
        async Task<PreparedImage?> ValidateImage(IFormFile? file, string field)
        {
            try { return await images.PrepareAsync(file); }
            catch (InvalidDataException ex) { ModelState.AddModelError(field, ex.Message); return null; }
        }
        var front = await ValidateImage(model.AnhMatTruoc, nameof(model.AnhMatTruoc));
        var back = await ValidateImage(model.AnhMatSau, nameof(model.AnhMatSau));
        if (!ModelState.IsValid) return InvalidForm(model);
        var isNew = profile is null;
        if (profile is null)
        {
            profile = new KhachThue { TaiKhoanId = tenant.Id, NgayTao = DateTime.UtcNow };
            db.KhachThues.Add(profile);
        }
        profile.HoTen = model.HoTen.Trim();
        profile.NgaySinh = model.NgaySinh;
        if (!string.IsNullOrEmpty(model.SoCanCuoc)) profile.SoGiayTo = model.SoCanCuoc;
        profile.QueQuan = model.QueQuan.Trim();
        profile.NgheNghiep = model.NgheNghiep.Trim();
        var oldFront = profile.AnhGiayToTruoc;
        var oldBack = profile.AnhGiayToSau;
        string? newFront = null, newBack = null;
        try
        {
            if (front is not null) profile.AnhGiayToTruoc = newFront = await images.SaveAsync(front);
            if (back is not null) profile.AnhGiayToSau = newBack = await images.SaveAsync(back);
            await db.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or DbUpdateException)
        {
            images.Delete(newFront);
            images.Delete(newBack);
            ModelState.AddModelError("", "Không thể lưu hồ sơ lúc này. Vui lòng thử lại và chọn lại ảnh.");
            return InvalidForm(model);
        }
        if (newFront is not null) images.Delete(oldFront);
        if (newBack is not null) images.Delete(oldBack);
        TempData["Success"] = isNew
            ? "Đã tạo hồ sơ cá nhân thành công."
            : "Đã cập nhật hồ sơ cá nhân thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Xoa()
    {
        var tenant = await CurrentTenantAsync();
        if (tenant is null) return Forbid();
        var profile = await db.KhachThues.SingleOrDefaultAsync(x => x.TaiKhoanId == tenant.Id);
        if (profile is null)
        {
            TempData["Error"] = "Bạn chưa có hồ sơ để xóa.";
            return RedirectToAction(nameof(Index));
        }

        db.KhachThues.Remove(profile);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.Sqlite.SqliteException { SqliteExtendedErrorCode: 787 })
        {
            TempData["Error"] = "Không thể xóa hồ sơ đang được hợp đồng hoặc dữ liệu khác sử dụng.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Không thể xóa hồ sơ lúc này. Vui lòng thử lại.";
            return RedirectToAction(nameof(Index));
        }

        images.Delete(profile.AnhGiayToTruoc);
        images.Delete(profile.AnhGiayToSau);
        TempData["Success"] = "Đã xóa hồ sơ cá nhân thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Anh(string mat, int? id)
    {
        var viewer = await CurrentViewerAsync();
        if (viewer is null) return Forbid();
        if (mat is not ("truoc" or "sau")) return NotFound();
        var profile = id.HasValue
            ? await db.KhachThues.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id.Value)
            : await db.KhachThues.AsNoTracking().SingleOrDefaultAsync(x => x.TaiKhoanId == viewer.Id);
        if (profile is null) return NotFound();
        if (profile.TaiKhoanId != viewer.Id && !await access.CanReadFullAsync(viewer, profile.Id)) return NotFound();
        var path = images.GetPath(mat == "truoc" ? profile?.AnhGiayToTruoc : profile?.AnhGiayToSau);
        if (path is null || !System.IO.File.Exists(path)) return NotFound();
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return PhysicalFile(path, Path.GetExtension(path) == ".jpg" ? "image/jpeg" : "image/png");
    }

    private IActionResult InvalidForm(HoSoViewModel model)
    {
        // Do not echo the submitted full number through Razor's ModelState value.
        model.SoCanCuoc = null;
        ModelState.SetModelValue(nameof(model.SoCanCuoc), new Microsoft.AspNetCore.Mvc.ModelBinding.ValueProviderResult(string.Empty));
        return View("Index", model);
    }

    [HttpGet]
    public async Task<IActionResult> Xem(int? id)
    {
        var viewer = await CurrentViewerAsync();
        if (viewer is null) return Forbid();
        var profile = id.HasValue
            ? await db.KhachThues.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id.Value)
            : await db.KhachThues.AsNoTracking().SingleOrDefaultAsync(x => x.TaiKhoanId == viewer.Id);
        if (profile is null) return id.HasValue ? NotFound() : View(new XemHoSoViewModel());
        var full = await access.CanReadFullAsync(viewer, profile.Id);
        var own = profile.TaiKhoanId == viewer.Id;
        return View(new XemHoSoViewModel
        {
            Id = profile.Id, HoTen = profile.HoTen,
            CanCuocHienThi = full ? profile.SoGiayTo ?? string.Empty : HoSoAccess.Mask(profile.SoGiayTo),
            XemDayDu = full, LaChuHoSo = own,
            NgaySinh = full || own ? profile.NgaySinh : null,
            QueQuan = full || own ? profile.QueQuan : null,
            NgheNghiep = full || own ? profile.NgheNghiep : null,
            CoAnhMatTruoc = (full || own) && profile.AnhGiayToTruoc is not null,
            CoAnhMatSau = (full || own) && profile.AnhGiayToSau is not null
        });
    }
}
