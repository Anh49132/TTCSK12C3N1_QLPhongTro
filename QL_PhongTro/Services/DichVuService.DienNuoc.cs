using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Models;
using QL_PhongTro.ViewModels;

namespace QL_PhongTro.Services;

public sealed partial class DichVuService
{
    private Task<List<CauHinhDichVu>> CauHinhDienNuocAsync(int buildingId) =>
        db.CauHinhDichVus.Include(x => x.DichVu)
            .Where(x => x.ToaNhaId == buildingId && x.PhongId == null &&
                (x.DichVu.MaDichVu == "DIEN" || x.DichVu.MaDichVu == "NUOC"))
            .OrderBy(x => x.TuNgay).ToListAsync();

    private static CauHinhDichVu? HienTai(List<CauHinhDichVu> rows, string code) =>
        rows.LastOrDefault(x => x.DichVu.MaDichVu == code && x.TuNgay <= HomNay() &&
            (x.DenNgay == null || x.DenNgay >= HomNay()));

    public async Task<CauHinhDienNuocViewModel> LayCauHinhDienNuocAsync(int accountId, int buildingId)
    {
        if (!await SoHuuToaNhaAsync(accountId, buildingId)) throw new UnauthorizedAccessException();
        var rows = await CauHinhDienNuocAsync(buildingId);
        return new CauHinhDienNuocViewModel
        {
            ToaNhaId = buildingId,
            TenToaNha = await db.ToaNhas.Where(x => x.Id == buildingId).Select(x => x.TenToaNha).SingleAsync(),
            Dien = Input(HienTai(rows, "DIEN")), Nuoc = Input(HienTai(rows, "NUOC"))
        };

        static CauHinhTienDichVuViewModel Input(CauHinhDichVu? row) => new()
        {
            CachTinh = row?.CachTinh ?? CachTinhDichVu.TheoChiSo,
            DonGiaChiSo = row is { DaChotGia: true, CachTinh: CachTinhDichVu.TheoChiSo } ? row.DonGia : null,
            TienMotNguoi = row is { DaChotGia: true, CachTinh: CachTinhDichVu.TheoNguoi } ? row.DonGia : null
        };
    }

    public async Task LuuCauHinhDienNuocAsync(int accountId, CauHinhDienNuocViewModel input)
    {
        await using var tx = await db.Database.BeginTransactionAsync();
        if (!await SoHuuToaNhaAsync(accountId, input.ToaNhaId)) throw new UnauthorizedAccessException();
        Validator.ValidateObject(input.Dien, new ValidationContext(input.Dien), true);
        Validator.ValidateObject(input.Nuoc, new ValidationContext(input.Nuoc), true);
        var rows = await CauHinhDienNuocAsync(input.ToaNhaId);
        await Save("DIEN", "Điện", "kWh", input.Dien);
        await Save("NUOC", "Nước", "m³", input.Nuoc);
        await db.SaveChangesAsync();
        await tx.CommitAsync();

        async Task Save(string code, string name, string unit, CauHinhTienDichVuViewModel value)
        {
            var row = HienTai(rows, code);
            if (row is null)
            {
                var catalog = await db.DichVus.SingleOrDefaultAsync(x => x.MaDichVu == code)
                    ?? new DichVu { MaDichVu = code, TenDichVu = name };
                row = new CauHinhDichVu
                {
                    ToaNhaId = input.ToaNhaId, DichVu = catalog, TuNgay = HomNay(),
                    DenNgay = rows.Where(x => x.DichVu.MaDichVu == code && x.TuNgay > HomNay())
                        .Select(x => (DateOnly?)x.TuNgay.AddDays(-1)).FirstOrDefault(),
                    NguoiTaoId = accountId, NgayTao = DateTime.UtcNow
                };
                db.CauHinhDichVus.Add(row);
                if (catalog.Id == 0 || !await db.DichVuToaNhas.AnyAsync(x => x.ToaNhaId == input.ToaNhaId && x.DichVuId == catalog.Id))
                    db.DichVuToaNhas.Add(new DichVuToaNha { ToaNhaId = input.ToaNhaId, DichVu = catalog });
            }
            // Slice 1 saves the current configuration immediately. Invoice snapshots and scheduled versions remain intact.
            row.CachTinh = value.CachTinh;
            row.DonViTinh = value.CachTinh == CachTinhDichVu.TheoChiSo ? unit : "người/tháng";
            row.DonGia = value.GiaApDung!.Value;
            row.DaChotGia = true;
        }
    }
}
