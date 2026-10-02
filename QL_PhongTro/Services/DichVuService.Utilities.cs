using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Models;
using QL_PhongTro.ViewModels;

namespace QL_PhongTro.Services;

public sealed partial class DichVuService
{
    public static string DonViDienNuoc(string code, string method) => method == CachTinhDichVu.TheoNguoi
        ? "người/tháng" : code == "DIEN" ? "kWh" : "m³";

    public async Task<DienNuocViewModel> DienNuocAsync(int accountId, int buildingId, string code)
    {
        if (code is not ("DIEN" or "NUOC")) throw new InvalidOperationException("Chỉ cấu hình điện hoặc nước.");
        if (!await SoHuuToaNhaAsync(accountId, buildingId)) throw new UnauthorizedAccessException();
        var versions = await db.CauHinhDichVus.AsNoTracking().Include(x => x.DichVu)
            .Where(x => x.ToaNhaId == buildingId && x.PhongId == null && x.DichVu.MaDichVu == code)
            .OrderBy(x => x.TuNgay).ToListAsync();
        var latest = versions.LastOrDefault();
        return new DienNuocViewModel { ToaNhaId = buildingId, MaDichVu = code, LichSu = versions,
            PhienBan = latest?.Id ?? 0, CachTinh = latest?.CachTinh ?? CachTinhDichVu.TheoChiSo,
            DonGia = latest?.DaChotGia == true ? latest.DonGia : null,
            KyApDung = latest is null ? HomNay() : KySau() };
    }

    public async Task LuuDienNuocAsync(int accountId, DienNuocViewModel input)
    {
        System.ComponentModel.DataAnnotations.Validator.ValidateObject(input, new(input), true);
        if (input.CachTinh is not (CachTinhDichVu.TheoChiSo or CachTinhDichVu.TheoNguoi))
            throw new InvalidOperationException("Điện/nước chỉ tính theo chỉ số hoặc đầu người.");
        await using var tx = await db.Database.BeginTransactionAsync();
        var current = await DienNuocAsync(accountId, input.ToaNhaId, input.MaDichVu);
        if (current.PhienBan != input.PhienBan || current.KyApDung != input.KyApDung)
            throw new InvalidOperationException("Cấu hình hoặc kỳ áp dụng đã thay đổi. Hãy tải lại trang trước khi lưu.");
        if (current.LichSu.Any(x => x.TuNgay >= current.KyApDung))
            throw new InvalidOperationException("Đã có cấu hình cho kỳ kế tiếp. Không ghi đè phiên bản đã lên lịch.");
        var catalog = await db.DichVus.SingleOrDefaultAsync(x => x.MaDichVu == input.MaDichVu);
        if (catalog is null)
        {
            catalog = new DichVu { MaDichVu = input.MaDichVu, TenDichVu = input.MaDichVu == "DIEN" ? "Điện" : "Nước" };
            db.DichVus.Add(catalog);
        }
        if (current.LichSu.LastOrDefault() is { } previous)
        {
            var tracked = await db.CauHinhDichVus.SingleAsync(x => x.Id == previous.Id);
            tracked.DenNgay = current.KyApDung!.Value.AddDays(-1);
            await db.SaveChangesAsync();
        }
        db.CauHinhDichVus.Add(new CauHinhDichVu { ToaNhaId = input.ToaNhaId, DichVu = catalog,
            CachTinh = input.CachTinh!, DonViTinh = DonViDienNuoc(input.MaDichVu, input.CachTinh!),
            DonGia = input.DonGia!.Value, TuNgay = current.KyApDung!.Value, DaChotGia = true,
            DangApDung = current.LichSu.LastOrDefault()?.DangApDung ?? true,
            NguoiTaoId = accountId, NgayTao = DateTime.UtcNow });
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }
}
