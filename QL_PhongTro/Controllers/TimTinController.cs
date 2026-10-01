using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Data;
using QL_PhongTro.Models;
using QL_PhongTro.ViewModels;

namespace QL_PhongTro.Controllers;

[AllowAnonymous]
public class TimTinController(AppDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? quanHuyen, CancellationToken cancellationToken)
    {
        var model = new TimTinViewModel { QuanHuyen = quanHuyen?.Trim() };
        // Districts come from the existing building data; no separate catalogue is needed.
        model.QuanHuyens = await db.ToaNhas.AsNoTracking()
            .Where(t => t.QuanHuyen != null && t.QuanHuyen.Trim() != "")
            .Select(t => t.QuanHuyen!.Trim()).Distinct().OrderBy(q => q)
            .ToListAsync(cancellationToken);

        // Do not create or upgrade a local database from a search request.
        await db.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            using var command = db.Database.GetDbConnection().CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'tin_dang'";
            model.SchemaReady = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken)) == 1;
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }

        if (!model.SchemaReady) return View(model);
        if (!string.IsNullOrEmpty(model.QuanHuyen) && !model.QuanHuyens.Contains(model.QuanHuyen))
        {
            ModelState.AddModelError(nameof(model.QuanHuyen), "Quận/huyện không thuộc danh sách hiện có.");
            return View(model);
        }

        // Expiration is a UTC timestamp; equality is still within the validity period.
        var now = DateTime.UtcNow;
        var query = from tin in db.Set<TinDang>().AsNoTracking()
                    join phong in db.PhongTros.AsNoTracking() on tin.PhongId equals phong.Id
                    join toa in db.ToaNhas.AsNoTracking() on phong.ToaNhaId equals toa.Id
                    where tin.TrangThai == "DANG_HIEN_THI"
                        && tin.NgayHetHan != null && tin.NgayHetHan >= now
                    select new TinTimKiem
                    {
                        TieuDe = tin.TieuDe, DiaChi = toa.DiaChi,
                        QuanHuyen = toa.QuanHuyen == null ? null : toa.QuanHuyen.Trim(),
                        GiaThue = phong.GiaThue, DienTich = phong.DienTich
                    };
        if (!string.IsNullOrEmpty(model.QuanHuyen))
            query = query.Where(t => t.QuanHuyen == model.QuanHuyen);
        model.TinDangs = await query.ToListAsync(cancellationToken);
        return View(model);
    }
}
