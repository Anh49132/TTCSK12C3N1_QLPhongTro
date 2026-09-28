using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Data;
using QL_PhongTro.Models;
using QL_PhongTro.ViewModels;
using Microsoft.Extensions.Options;

namespace QL_PhongTro.Services;

// Immutable result: invoice code must persist this snapshot, not query a live price when displaying an issued invoice.
public sealed record DonGiaDichVu(int CauHinhId, int DichVuId, string TenDichVu, string CachTinh, string DonViTinh, long DonGia);

public sealed partial class DichVuService(AppDbContext db, IOptions<DichVuMacDinhOptions>? defaults = null)
{
    public static DateOnly HomNay() => DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));

    public Task<bool> SoHuuToaNhaAsync(int accountId, int buildingId) =>
        (from building in db.ToaNhas
         join account in db.TaiKhoans on building.ChuNhaId equals account.Id
         where building.Id == buildingId && building.ChuNhaId == accountId && building.DangHoatDong
               && account.DangHoatDong && account.VaiTro == "CHU_NHA"
         select building.Id).AnyAsync();

    public async Task<bool> SanSangAsync()
    {
        var connection = db.Database.GetDbConnection();
        var close = connection.State != System.Data.ConnectionState.Open;
        if (close) await connection.OpenAsync();
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name IN ('dich_vu','cau_hinh_dich_vu','khoi_tao_dich_vu')";
            if (Convert.ToInt32(await command.ExecuteScalarAsync()) != 3) return false;
            command.CommandText = "SELECT COUNT(*) FROM pragma_table_info('cau_hinh_dich_vu') WHERE name='da_chot_gia'";
            return Convert.ToInt32(await command.ExecuteScalarAsync()) == 1;
        }
        finally { if (close) await connection.CloseAsync(); }
    }

    public async Task<int> ThemAsync(int accountId, TaoDichVuViewModel input)
    {
        Validator.ValidateObject(input, new ValidationContext(input), validateAllProperties: true);
        await using var transaction = await db.Database.BeginTransactionAsync();
        if (!await SoHuuToaNhaAsync(accountId, input.ToaNhaId!.Value))
            throw new UnauthorizedAccessException("Tòa nhà không thuộc chủ nhà hiện tại hoặc đã ngừng hoạt động.");
        var price = new CauHinhDichVu
        {
            ToaNhaId = input.ToaNhaId.Value,
            DichVu = new DichVu
            {
                MaDichVu = "DV" + Guid.NewGuid().ToString("N")[..18],
                TenDichVu = input.TenDichVu!.Trim()
            },
            CachTinh = input.CachTinh!, DonViTinh = input.DonViTinh!.Trim(), DonGia = input.DonGia!.Value,
            TuNgay = HomNay(), NguoiTaoId = accountId, NgayTao = DateTime.UtcNow
        };
        db.CauHinhDichVus.Add(price);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return price.Id;
    }

    public async Task<List<CauHinhDichVu>> DanhSachAsync(int accountId, int buildingId)
    {
        if (!await SoHuuToaNhaAsync(accountId, buildingId)) throw new UnauthorizedAccessException();
        return await db.CauHinhDichVus.AsNoTracking().Include(x => x.DichVu)
            .Where(x => x.ToaNhaId == buildingId && x.PhongId == null)
            .OrderBy(x => x.DichVu.TenDichVu).ThenBy(x => x.Id).ToListAsync();
    }

    // AC1 supplies building-level prices. Room overrides and invoice persistence are separate stories.
    public async Task<DonGiaDichVu?> LayDonGiaAsync(int accountId, int buildingId, int serviceId, DateOnly ngayChot)
    {
        if (!await SoHuuToaNhaAsync(accountId, buildingId)) throw new UnauthorizedAccessException();
        var prices = await db.CauHinhDichVus.AsNoTracking()
            .Where(x => x.ToaNhaId == buildingId && x.PhongId == null && x.DichVuId == serviceId
                        && x.TuNgay <= ngayChot && (x.DenNgay == null || x.DenNgay >= ngayChot))
            .Include(x => x.DichVu).Take(2).ToListAsync();
        if (prices.Count > 1) throw new InvalidOperationException("Có nhiều đơn giá cùng hiệu lực. Cần kiểm tra cấu hình dịch vụ.");
        var price = prices.SingleOrDefault();
        if (price is null || !price.DangApDung || !price.DichVu.DangHoatDong || !price.DaChotGia) return null;
        return new(price.Id, price.DichVuId, price.DichVu.TenDichVu, price.CachTinh, price.DonViTinh, price.DonGia);
    }
}
