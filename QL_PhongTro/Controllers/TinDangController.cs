using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Data;
using QL_PhongTro.ViewModels;

namespace QL_PhongTro.Controllers;

[Route("TinDang")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class TinDangController(AppDbContext db) : Controller
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
                .ToListAsync()
        };
    }
}