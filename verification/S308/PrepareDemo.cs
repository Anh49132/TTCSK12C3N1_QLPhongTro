using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Data;
using QL_PhongTro.Models;
using QL_PhongTro.Services;

internal static class PrepareDemo
{
    public static async Task Run(string root, string database)
    {
        root = Path.GetFullPath(root); database = Path.GetFullPath(database);
        var allowed = Path.Combine(root, "data", "s308-demo") + Path.DirectorySeparatorChar;
        if (!database.StartsWith(allowed, StringComparison.OrdinalIgnoreCase) || !File.Exists(database))
            throw new InvalidOperationException("Only an existing copy under data/s308-demo is allowed.");
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite("Data Source=" + database).Options;
        using var read = new AppDbContext(options);
        var actor = await read.TaiKhoans.Where(x => x.Email == "owner.demo@demo.local").Select(x => x.Id).SingleAsync();
        using var db = new AppDbContext(options, new HttpContextAccessor { HttpContext = new DefaultHttpContext {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, actor.ToString()),
                new Claim(ClaimTypes.Role, "CHU_NHA")], "demo")) } });
        var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
        var first = new DateOnly(today.Year, today.Month, 1); var last = first.AddMonths(1).AddDays(-1);
        var contracts = await (from h in db.HopDongs join p in db.PhongTros on h.PhongId equals p.Id
            join b in db.ToaNhas on p.ToaNhaId equals b.Id
            where b.ChuNhaId == actor && h.TrangThai == "DANG_HIEU_LUC" select new { h.Id, p.ToaNhaId }).ToListAsync();
        var meters = await db.DichVus.Where(x => x.MaDichVu == "DIEN" || x.MaDichVu == "NUOC").ToListAsync();
        // Sprint 2 water is billed per person. This disposable copy needs a new meter-price period for the monthly preview.
        foreach (var meter in meters) {
            var configs = await db.CauHinhDichVus.Where(x => x.DichVuId == meter.Id && x.TuNgay < first
                && (x.DenNgay == null || x.DenNgay >= first) && x.CachTinh != "THEO_CHI_SO").ToListAsync();
            foreach (var old in configs) {
                var previousEnd = old.DenNgay; old.DenNgay = first.AddDays(-1);
                db.CauHinhDichVus.Add(new() { ToaNhaId = old.ToaNhaId, PhongId = old.PhongId, DichVuId = meter.Id,
                    CachTinh = "THEO_CHI_SO", DonViTinh = meter.MaDichVu == "DIEN" ? "kWh" : "m³", DonGia = old.DonGia,
                    TuNgay = first, DenNgay = previousEnd, NguoiTaoId = actor, NgayTao = DateTime.UtcNow });
            }
        }
        await db.SaveChangesAsync();
        foreach (var contract in contracts)
            foreach (var meter in meters)
                if (!await db.ChiSoDienNuocs.AnyAsync(x => x.HopDongId == contract.Id && x.DichVuId == meter.Id && x.TuNgay == first && x.DenNgay == last))
                    db.ChiSoDienNuocs.Add(new() { HopDongId = contract.Id, DichVuId = meter.Id, TuNgay = first, DenNgay = last,
                        ChiSoDau = meter.MaDichVu == "DIEN" ? 100 : 10, ChiSoCuoi = meter.MaDichVu == "DIEN" ? 120 : 12,
                        NguoiNhapId = actor, NgayNhap = DateTime.UtcNow, DaXacNhanBatThuong = true });
        await db.SaveChangesAsync();
        var svc = new HoaDonDichVuService(db, new DichVuService(db));
        foreach (var building in contracts.Select(x => x.ToaNhaId).Distinct()) {
            var preview = await svc.XemThangAsync(actor, building, today.Year, today.Month);
            foreach (var skipped in preview.BoQua) Console.WriteLine(skipped.MaPhong + ": " + skipped.LyDo);
            if (preview.DuKien.Count == 0) continue;
            var id = await svc.TaoNhapThangAsync(actor, building, preview.DuKien[0].HoaDon.HopDongId, today.Year, today.Month);
            await File.WriteAllTextAsync(Path.Combine(Path.GetDirectoryName(database)!, "draft-id.txt"), id.ToString());
            Console.WriteLine("Prepared draft ID " + id + " on copy only: " + database);
            return;
        }
        throw new InvalidOperationException("No eligible invoice in the copied demo.");
    }
}
