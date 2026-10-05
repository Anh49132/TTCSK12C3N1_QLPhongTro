using System.Diagnostics;
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
    TinDangExpirationService? expiration = null) : Controller
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
                }
            }
            catch
            {
                // Fallback nếu schema CSDL chưa sẵn sàng trong các môi trường kiểm thử tối giản
            }
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
