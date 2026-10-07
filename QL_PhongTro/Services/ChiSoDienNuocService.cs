using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Data;
using QL_PhongTro.ViewModels;

namespace QL_PhongTro.Services;

public sealed class ChiSoDienNuocService(AppDbContext db, ITimeProvider clock)
{
    private sealed record Reading(int HopDongId, int Id, DateOnly TuNgay, DateOnly DenNgay,
        string MaDichVu, decimal? ChiSoCuoi, int LineId);

    public async Task<ChiSoDienNuocViewModel> DanhSachAsync(int accountId, int? toaNhaId, CancellationToken ct = default)
    {
        if (!await db.TaiKhoans.AsNoTracking().AnyAsync(x => x.Id == accountId && x.VaiTro == "QUAN_LY"
                && x.DangHoatDong && !x.IsDeleted, ct)) throw new UnauthorizedAccessException();
        var today = DateOnly.FromDateTime(clock.UtcNow.AddHours(7));
        var model = new ChiSoDienNuocViewModel { DauKy = new(today.Year, today.Month, 1) };
        model.ToaNhas = await db.ToaNhas.AsNoTracking()
            .Where(x => x.QuanLyId == accountId && x.DangHoatDong).OrderBy(x => x.TenToaNha).ThenBy(x => x.Id)
            .Select(x => new ToaNhaGhiChiSo(x.Id, x.TenToaNha)).ToListAsync(ct);
        if (toaNhaId.HasValue && !model.ToaNhas.Any(x => x.Id == toaNhaId)) throw new UnauthorizedAccessException();
        model.ToaNhaId = toaNhaId ?? model.ToaNhas.FirstOrDefault()?.Id;
        if (model.ToaNhaId is null) return model;
        model.Phongs = await LayPhongAsync(model.ToaNhaId.Value, model.DauKy, ct);
        return model;
    }

    private async Task<List<PhongGhiChiSo>> LayPhongAsync(int buildingId, DateOnly first, CancellationToken ct)
    {
        var last = first.AddMonths(1).AddDays(-1);
        var candidates = await (from p in db.PhongTros.AsNoTracking()
            join h in db.HopDongs on p.Id equals h.PhongId
            join k in db.KyHopDongs on h.Id equals k.HopDongId
            where p.ToaNhaId == buildingId && p.TrangThai == "DANG_THUE" && h.TrangThai == "DANG_HIEU_LUC"
                && k.NgayBatDau <= last && k.NgayKetThuc >= first
                && (h.NgayTraPhong == null || (h.NgayTraPhong >= first && h.NgayTraPhong >= k.NgayBatDau))
            select new { p.Id, p.MaPhong, p.Tang, HopDongId = h.Id, k.NgayBatDau }).ToListAsync(ct);
        // Multiple terms must not duplicate a room. For successive contracts in one month,
        // use the most recently starting intersecting contract, never combine their readings.
        var rooms = candidates.GroupBy(x => x.Id).Select(g => g.OrderByDescending(x => x.NgayBatDau)
            .ThenByDescending(x => x.HopDongId).First()).OrderBy(x => x.Tang).ThenBy(x => x.MaPhong, StringComparer.Ordinal).ToList();
        var ids = rooms.Select(x => x.HopDongId).Distinct().ToArray();
        var readings = new List<Reading>();
        // Invoices are an optional module in existing databases. Missing history is
        // not a reason to manufacture readings or install that module on a GET.
        var connection = db.Database.GetDbConnection();
        var close = connection.State != System.Data.ConnectionState.Open;
        if (close) await connection.OpenAsync(ct);
        bool hasInvoices;
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name IN ('hoa_don','chi_tiet_hoa_don')";
            hasInvoices = Convert.ToInt32(await command.ExecuteScalarAsync(ct)) == 2;
        }
        finally { if (close) await connection.CloseAsync(); }
        if (hasInvoices) readings = await (from invoice in db.HoaDons.AsNoTracking()
            join line in db.ChiTietHoaDons on invoice.Id equals line.HoaDonId
            join service in db.DichVus on line.DichVuId equals service.Id
            where ids.Contains(invoice.HopDongId) && invoice.TrangThai == "DA_PHAT_HANH"
                && line.LoaiKhoan == "DICH_VU" && line.CachTinhApDung == "THEO_CHI_SO" && line.ChiSoCuoi != null
                && (service.MaDichVu == "DIEN" || service.MaDichVu == "NUOC") && invoice.DenNgay <= last
            select new Reading(invoice.HopDongId, invoice.Id, invoice.TuNgay, invoice.DenNgay, service.MaDichVu, line.ChiSoCuoi, line.Id)).ToListAsync(ct);
        var handovers = await db.HopDongChiSoDauKys.AsNoTracking()
            .Where(x => ids.Contains(x.HopDongId) && x.NgayBanGiao <= last).ToDictionaryAsync(x => x.HopDongId, ct);
        return rooms.Select(room =>
        {
            ChiSoThamChieu Previous(string code)
            {
                var previous = readings.Where(x => x.HopDongId == room.HopDongId && x.MaDichVu == code && x.DenNgay < first)
                    .OrderByDescending(x => x.DenNgay).ThenByDescending(x => x.Id).ThenByDescending(x => x.LineId).FirstOrDefault();
                if (previous != null) return new(previous.ChiSoCuoi);
                return handovers.TryGetValue(room.HopDongId, out var handover)
                    ? new(code == "DIEN" ? handover.ChiSoDien : handover.ChiSoNuoc, true) : new(null);
            }
            bool Current(string code) => readings.Any(x => x.HopDongId == room.HopDongId && x.MaDichVu == code
                && x.TuNgay >= first && x.DenNgay <= last);
            return new PhongGhiChiSo(room.Id, room.HopDongId, room.MaPhong, room.Tang,
                Previous("DIEN"), Previous("NUOC"), Current("DIEN") && Current("NUOC"));
        }).ToList();
    }
}
