using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Models;
using QL_PhongTro.ViewModels;

namespace QL_PhongTro.Services;

public sealed partial class HoaDonDichVuService
{
    private async Task KiemTraChuNhaAsync(int actor, int building)
    {
        if (!await db.TaiKhoans.AnyAsync(x => x.Id == actor && x.VaiTro == "CHU_NHA" && x.DangHoatDong && !x.IsDeleted)
            || !await services.SoHuuToaNhaAsync(actor, building)) throw new UnauthorizedAccessException();
    }

    public async Task<PhatHanhThangViewModel> XemThangAsync(int actor, int building, int year, int month)
    {
        await KiemTraChuNhaAsync(actor, building);
        if (year is < 1900 or > 9998 || month is < 1 or > 12) throw new InvalidOperationException("Kỳ hóa đơn không hợp lệ.");
        var model = new PhatHanhThangViewModel { ToaNhaId = building, Nam = year, Thang = month, SanSang = await SanSangAsync() };
        if (!model.SanSang) return model;
        // A GET never installs optional database modules.
        if (!await db.Database.SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM sqlite_master WHERE type='table' AND name='chi_so_dien_nuoc'").AnyAsync(x => x == 1))
        { model.SanSang = false; return model; }
        var first = new DateOnly(year, month, 1);
        var last = first.AddMonths(1).AddDays(-1);
        var contracts = await (from h in db.HopDongs.AsNoTracking() join p in db.PhongTros on h.PhongId equals p.Id
            where p.ToaNhaId == building && p.TrangThai == "DANG_THUE" && h.TrangThai == "DANG_HIEU_LUC"
            orderby p.MaPhong select new { Contract = h, Room = p }).ToListAsync();
        var meterServices = await db.DichVus.AsNoTracking().Where(x => x.MaDichVu == "DIEN" || x.MaDichVu == "NUOC").OrderBy(x => x.MaDichVu).ToListAsync();
        var pricing = new DichVuPhongService(db, services);
        foreach (var context in contracts)
        {
            var h = context.Contract;
            var terms = await db.KyHopDongs.AsNoTracking().Where(x => x.HopDongId == h.Id && x.NgayBatDau <= first && x.NgayKetThuc >= last).ToListAsync();
            model.TongPhong++;
            // Retain the existing full-month rule. Partial/renewal months need a separate policy.
            if (terms.Count != 1 || h.NgayTraPhong < last || h.NgayChot is < 1 or > 31) continue;
            if (await db.HoaDons.AnyAsync(x => x.HopDongId == h.Id && x.Nam == year && x.Thang == month && x.TrangThai != "DA_HUY")) continue;
            var cutoff = new DateOnly(year, month, Math.Min(h.NgayChot, last.Day));
            var occupancy = await LaySoNguoiAsync(actor, h.Id, building, first);
            var now = (clock ?? new SystemTimeProvider()).UtcNow;
            var invoice = new HoaDon { MaHoaDon = "HD" + Guid.NewGuid().ToString("N")[..24], HopDongId = h.Id,
                Nam = year, Thang = month, TuNgay = first, DenNgay = last, NgayChot = cutoff,
                SoNguoiTinhPhi = occupancy.SoNguoi, NgayLap = now, HanThanhToan = DateOnly.FromDateTime(now.AddHours(7)).AddDays(7), NguoiLapId = actor };
            invoice.ChiTiet.Add(new() { SoThuTu = 1, LoaiKhoan = "TIEN_PHONG", TenKhoan = "Tiền phòng", DonViTinh = "tháng",
                KyHopDongId = terms[0].Id, SoLuong = 1, DonGia = terms[0].GiaThue, ThanhTien = terms[0].GiaThue });
            var readings = new List<ChiSoDienNuoc>();
            var agreed = await db.HopDongDichVus.Where(x => x.HopDongId == h.Id).Select(x => x.DichVuId).ToListAsync();
            foreach (var service in meterServices)
            {
                if (agreed.Count > 0 && !agreed.Contains(service.Id)) continue;
                var price = await pricing.LayGiaHoaDonAsync(actor, context.Room.Id, service.Id, cutoff);
                if (price is null || price.DonGia <= 0 || price.CachTinh != CachTinhDichVu.TheoChiSo) continue;
                var reading = await db.ChiSoDienNuocs.AsNoTracking().SingleOrDefaultAsync(x => x.HopDongId == h.Id && x.DichVuId == service.Id && x.TuNgay == first && x.DenNgay == last);
                if (reading is null || reading.DaKhoa || reading.ChiSoDau < 0 || reading.ChiSoCuoi < reading.ChiSoDau) continue;
                readings.Add(reading);
                invoice.ChiTiet.Add(new() { SoThuTu = invoice.ChiTiet.Count + 1, DichVuId = service.Id, CauHinhDichVuId = price.CauHinhId,
                    TenKhoan = price.TenDichVu, CachTinhApDung = price.CachTinh, DonViTinh = price.DonViTinh, DonGia = price.DonGia,
                    ChiSoDau = reading.ChiSoDau, ChiSoCuoi = reading.ChiSoCuoi, SoLuong = reading.ChiSoCuoi - reading.ChiSoDau,
                    ThanhTien = ThanhTien(reading.ChiSoCuoi - reading.ChiSoDau, price.DonGia) });
            }
            // S3-06 requires both electricity and water, never an incomplete invoice.
            if (readings.Count != 2) continue;
            invoice.TongTien = invoice.ChiTiet.Aggregate(0L, (sum, line) => checked(sum + line.ThanhTien));
            model.DuKien.Add(new(context.Room.MaPhong, invoice, readings));
        }
        var history = await (from invoice in db.HoaDons.AsNoTracking().Include(x => x.ChiTiet)
            join h in db.HopDongs on invoice.HopDongId equals h.Id join p in db.PhongTros on h.PhongId equals p.Id
            where p.ToaNhaId == building && invoice.Nam == year && invoice.Thang == month && invoice.TrangThai == "DA_PHAT_HANH"
            orderby p.MaPhong select new { p.MaPhong, Invoice = invoice }).ToListAsync();
        model.DaPhatHanh = history.Select(x => new HoaDonThangDuKien(x.MaPhong, x.Invoice, [])).ToList();
        return model;
    }

    public async Task<int> PhatHanhThangAsync(int actor, int building, int year, int month)
    {
        await db.Database.OpenConnectionAsync();
        await using var sqlite = ((SqliteConnection)db.Database.GetDbConnection()).BeginTransaction(deferred: false);
        await using var tx = await db.Database.UseTransactionAsync(sqlite);
        var model = await XemThangAsync(actor, building, year, month);
        if (!model.SanSang) throw new InvalidOperationException("Chức năng hóa đơn chưa được thiết lập.");
        foreach (var row in model.DuKien)
        {
            db.HoaDons.Add(row.HoaDon);
            foreach (var reading in row.ChiSo)
            {
                var tracked = await db.ChiSoDienNuocs.FindAsync(reading.Id);
                tracked!.DaKhoa = true; tracked.PhienBan++;
            }
        }
        await db.SaveChangesAsync();
        foreach (var row in model.DuKien)
        {
            row.HoaDon.TrangThai = "DA_PHAT_HANH";
            row.HoaDon.NgayPhatHanh = (clock ?? new SystemTimeProvider()).UtcNow;
            row.HoaDon.NguoiPhatHanhId = actor;
        }
        await db.SaveChangesAsync();
        await sqlite.CommitAsync();
        return model.DuKien.Count;
    }
}
