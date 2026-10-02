using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Data;
using QL_PhongTro.Models;
using QL_PhongTro.Services;
using QL_PhongTro.ViewModels;

namespace QL_PhongTro.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class TinDangController(AppDbContext db, YeuCauThueService requests, DichVuService services, DichVuPhongService roomServices) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!await requests.IsInstalled()) return View("ChuaCaiDat");
        return View(await requests.PublicListings().OrderByDescending(t => t.NgayDang).Take(100).ToListAsync());
    }

    [HttpGet]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> ChiTiet(int id)
    {
        var model = await Detail(id, new());
        return model is null ? NotFound() : View(model);
    }

    [Authorize(Roles = "KHACH_THUE"), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> GuiYeuCau(int id, [Bind(Prefix = "Form")] GuiYeuCauViewModel form)
    {
        if (!await requests.IsInstalled()) return View("ChuaCaiDat");
        var model = await Detail(id, form);
        if (model is null) return NotFound();
        if (form.NgayMongMuon is not null && requests.ValidateDesiredDate(form.NgayMongMuon) is { } dateError)
            ModelState.AddModelError("Form.NgayMongMuon", dateError);
        if (!ModelState.IsValid) return View("ChiTiet", model);
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var accountId)) return Forbid();
        try
        {
            var request = await requests.Send(id, accountId, form);
            return request is null ? NotFound() : RedirectToAction(nameof(ThanhCong), new { id = request.Id });
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (OpenRequestExistsException error)
        {
            ModelState.AddModelError("", error.Message);
            return View("ChiTiet", model with { OpenRequestId = error.RequestId });
        }
        catch (RoomCapacityException error)
        {
            ModelState.AddModelError("Form.SoNguoiDuKien", error.Message);
            // Reflect the capacity read inside the transaction if it changed after Detail.
            model.Phong.SoNguoiToiDa = error.Maximum;
            return View("ChiTiet", model);
        }
        catch (DesiredDateException error)
        {
            ModelState.AddModelError("Form.NgayMongMuon", error.Message);
            return View("ChiTiet", model with { Today = requests.Today });
        }
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
        var publicDetail = await GetPublicListingAsync(id);
        if (publicDetail is null) return null;
        var tin = await db.TinDangs.AsNoTracking().SingleOrDefaultAsync(t => t.Id == id);
        if (tin is null) return null;
        var room = await db.PhongTros.AsNoTracking().SingleAsync(p => p.Id == tin.PhongId);
        var installed = await requests.IsInstalled();
        var existing = installed && User.IsInRole("KHACH_THUE")
            && int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var accountId)
            ? await requests.FindOpenRequest(id, accountId) : null;
        return new(tin, room, form, requests.Today) { OpenRequestId = existing?.Id, PublicDetail = publicDetail, RequestModuleInstalled = installed };
    }

    [Authorize(Roles = "KHACH_THUE"), HttpGet]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> YeuCau(int id)
    {
        if (!await requests.IsInstalled()) return NotFound();
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var accountId)) return Forbid();
        var request = await db.YeuCauThues.AsNoTracking().SingleOrDefaultAsync(r => r.Id == id
            && db.KhachThues.Any(k => k.Id == r.KhachThueId && k.TaiKhoanId == accountId));
        return request is null ? NotFound() : View(request);
    }
    [HttpGet("/api/tin-dang/{id:int}")]
    [Produces("application/json")]
    public async Task<IActionResult> ChiTietApi(int id)
    {
        var listing = await GetPublicListingAsync(id);
        return listing is null ? NotFound() : Ok(listing);
    }

    private async Task<TinDangChiTietViewModel?> GetPublicListingAsync(int id)
    {
        var now = DateTime.UtcNow;
        var result = await (
            from post in db.TinDangs.AsNoTracking()
            join room in db.PhongTros.AsNoTracking() on post.PhongId equals room.Id
            join building in db.ToaNhas.AsNoTracking() on room.ToaNhaId equals building.Id
            where post.Id == id
                && post.TrangThai == "DANG_HIEN_THI"
                && (post.NgayHetHan == null || post.NgayHetHan > now)
                && room.TrangThai == "TRONG"
                && building.DangHoatDong
            select new
            {
                RoomId = room.Id,
                BuildingId = building.Id,
                OwnerId = building.ChuNhaId,
                Listing = new TinDangChiTietViewModel
                {
                    Id = post.Id,
                    TieuDe = post.TieuDe,
                    MoTa = post.NoiDung ?? room.MoTa,
                    GiaThue = room.GiaThue,
                    DienTich = room.DienTich,
                    SoNguoiToiDa = room.SoNguoiToiDa,
                    TienCocDuKien = room.TienCocDuKien,
                    DiaChi = building.DiaChi,
                    PhuongXa = building.PhuongXa,
                    QuanHuyen = building.QuanHuyen,
                    TinhThanh = building.TinhThanh
                }
            }).SingleOrDefaultAsync();

        if (result is null)
            return null;

        var listing = result.Listing;
        var servicePrices = await GetPublicServicePricesAsync(result.BuildingId, result.RoomId, result.OwnerId);
        return new TinDangChiTietViewModel
        {
            Id = listing.Id,
            TieuDe = listing.TieuDe,
            MoTa = listing.MoTa,
            GiaThue = listing.GiaThue,
            DienTich = listing.DienTich,
            SoNguoiToiDa = listing.SoNguoiToiDa,
            TienCocDuKien = listing.TienCocDuKien,
            DiaChi = listing.DiaChi,
            PhuongXa = listing.PhuongXa,
            QuanHuyen = listing.QuanHuyen,
            TinhThanh = listing.TinhThanh,
            Anh = await db.AnhPhongs.AsNoTracking()
                .Where(image => image.PhongId == result.RoomId)
                .OrderBy(image => image.ThuTu)
                .Select(image => new AnhPhongChiTietViewModel
                {
                    DuongDan = image.DuongDan,
                    DuongDanAnhNho = image.DuongDanAnhNho,
                    MoTa = image.MoTa
                })
                .ToListAsync(),
            DichVuTheoSuDung = servicePrices.Where(price => price.CachTinh != CachTinhDichVu.CoDinh).ToList(),
            KhoanCoDinh = servicePrices.Where(price => price.CachTinh == CachTinhDichVu.CoDinh).ToList()
        };
    }

    private async Task<List<DichVuTinChiTietViewModel>> GetPublicServicePricesAsync(int buildingId, int roomId, int ownerId)
    {
        if (!await services.SanSangAsync() || !await services.SoHuuToaNhaAsync(ownerId, buildingId))
            return [];

        var today = DichVuService.HomNay();
        var servicesForBuilding = await db.DichVuToaNhas.AsNoTracking()
            .Include(item => item.DichVu)
            .Where(item => item.ToaNhaId == buildingId)
            .OrderBy(item => item.DichVu.TenDichVu)
            .ToListAsync();

        var result = new List<DichVuTinChiTietViewModel>();
        foreach (var service in servicesForBuilding)
        {
            var price = await roomServices.LayGiaHoaDonAsync(ownerId, roomId, service.DichVuId, today);
            if (price is null || string.IsNullOrWhiteSpace(price.TenDichVu)
                || string.IsNullOrWhiteSpace(price.DonViTinh)
                || !CachTinhDichVu.HopLe(price.CachTinh)
                || price.DonGia < 0)
                continue;

            var isUtility = service.DichVu.MaDichVu.Equals("DIEN", StringComparison.OrdinalIgnoreCase)
                || service.DichVu.MaDichVu.Equals("NUOC", StringComparison.OrdinalIgnoreCase);
            if (isUtility && price.DonGia == 0)
                continue;

            result.Add(new DichVuTinChiTietViewModel
            {
                TenDichVu = price.TenDichVu.Trim(),
                CachTinh = price.CachTinh,
                DonViTinh = price.DonViTinh.Trim(),
                DonGia = price.DonGia
            });
        }

        return result;
    }
}
