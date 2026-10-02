using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Data;
using QL_PhongTro.Models;
using QL_PhongTro.Services;
using QL_PhongTro.ViewModels;

namespace QL_PhongTro.Controllers;

[Route("TinDang")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class TinDangController(AppDbContext db, DichVuService services) : Controller
{
    [HttpGet("ChiTiet/{id:int}")]
    public async Task<IActionResult> ChiTiet(int id)
    {
        var listing = await GetPublicListingAsync(id);
        return listing is null ? NotFound() : View(listing);
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
        var servicePrices = await GetPublicServicePricesAsync(result.BuildingId, result.RoomId);
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

    private async Task<List<DichVuTinChiTietViewModel>> GetPublicServicePricesAsync(int buildingId, int roomId)
    {
        if (!await services.SanSangAsync())
            return [];

        var today = DichVuService.HomNay();
        var candidates = await db.CauHinhDichVus.AsNoTracking()
            .Include(price => price.DichVu)
            .Where(price => price.ToaNhaId == buildingId
                && (price.PhongId == null || price.PhongId == roomId)
                && price.TuNgay <= today
                && (price.DenNgay == null || price.DenNgay >= today))
            .OrderBy(price => price.DichVu.TenDichVu)
            .ToListAsync();

        var result = new List<DichVuTinChiTietViewModel>();
        foreach (var group in candidates.GroupBy(price => price.DichVuId))
        {
            var roomPrices = group.Where(price => price.PhongId == roomId).ToList();
            var selectedScope = roomPrices.Count > 0
                ? roomPrices
                : group.Where(price => price.PhongId is null).ToList();
            if (selectedScope.Count != 1)
                continue;

            var price = selectedScope[0];
            if (!price.DangApDung || !price.DaChotGia || !price.DichVu.DangHoatDong
                || string.IsNullOrWhiteSpace(price.DichVu.TenDichVu)
                || string.IsNullOrWhiteSpace(price.DonViTinh)
                || !CachTinhDichVu.HopLe(price.CachTinh)
                || price.DonGia < 0)
                continue;

            var isUtility = price.DichVu.MaDichVu.Equals("DIEN", StringComparison.OrdinalIgnoreCase)
                || price.DichVu.MaDichVu.Equals("NUOC", StringComparison.OrdinalIgnoreCase);
            if (isUtility && price.DonGia == 0)
                continue;

            result.Add(new DichVuTinChiTietViewModel
            {
                TenDichVu = price.DichVu.TenDichVu.Trim(),
                CachTinh = price.CachTinh,
                DonViTinh = price.DonViTinh.Trim(),
                DonGia = price.DonGia
            });
        }

        return result;
    }
}