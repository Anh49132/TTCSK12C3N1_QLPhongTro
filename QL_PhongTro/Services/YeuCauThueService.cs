using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Data;
using QL_PhongTro.Models;
using QL_PhongTro.ViewModels;

namespace QL_PhongTro.Services;

public enum KetQuaHuyYeuCauThue
{
    DaHuy,
    KhongTimThay,
    KhongTheHuy,
    DaThayDoi
}

public sealed class YeuCauThueService(AppDbContext db, ITimeProvider timeProvider)
{
    public async Task<IReadOnlyList<YeuCauThueItemViewModel>> DanhSachCuaKhachAsync(int taiKhoanId)
    {
        var khachThueId = await db.KhachThues.AsNoTracking()
            .Where(x => x.TaiKhoanId == taiKhoanId)
            .Select(x => (int?)x.Id)
            .SingleOrDefaultAsync();
        if (khachThueId is null) return [];

        var requests = await db.YeuCauThues.AsNoTracking()
            .Where(x => x.KhachThueId == khachThueId.Value)
            .OrderByDescending(x => x.NgayTao)
            .ThenByDescending(x => x.Id)
            .ToListAsync();
        return requests.Select(x => new YeuCauThueItemViewModel(
            x.Id,
            x.MaYeuCau,
            ThongTinPhong(x),
            x.NgayTao,
            x.LichHen,
            x.TrangThai,
            TenTrangThai(x.TrangThai),
            TrangThaiYeuCauThue.CoTheHuy(x.TrangThai))).ToArray();
    }

    public async Task<KetQuaHuyYeuCauThue> HuyCuaKhachAsync(int taiKhoanId, int yeuCauId)
    {
        var khachThueId = await db.KhachThues.AsNoTracking()
            .Where(x => x.TaiKhoanId == taiKhoanId)
            .Select(x => (int?)x.Id)
            .SingleOrDefaultAsync();
        if (khachThueId is null) return KetQuaHuyYeuCauThue.KhongTimThay;

        var request = await db.YeuCauThues.SingleOrDefaultAsync(x =>
            x.Id == yeuCauId && x.KhachThueId == khachThueId.Value);
        if (request is null) return KetQuaHuyYeuCauThue.KhongTimThay;
        if (!TrangThaiYeuCauThue.CoTheHuy(request.TrangThai))
            return KetQuaHuyYeuCauThue.KhongTheHuy;

        request.TrangThai = TrangThaiYeuCauThue.DaHuy;
        request.NguoiXuLyId = taiKhoanId;
        request.NgayXuLy = timeProvider.UtcNow;
        request.PhienBan++;
        try
        {
            await db.SaveChangesAsync();
            return KetQuaHuyYeuCauThue.DaHuy;
        }
        catch (DbUpdateConcurrencyException)
        {
            return KetQuaHuyYeuCauThue.DaThayDoi;
        }
    }

    private static string TenTrangThai(string trangThai) => trangThai switch
    {
        TrangThaiYeuCauThue.Moi => "Mới",
        TrangThaiYeuCauThue.DaHenLich => "Đã hẹn lịch",
        TrangThaiYeuCauThue.DaDuyet => "Đã duyệt",
        TrangThaiYeuCauThue.TuChoi => "Từ chối",
        TrangThaiYeuCauThue.DaHuy => "Đã huỷ",
        _ => "Không xác định"
    };

    private static string ThongTinPhong(YeuCauThue request)
    {
        if (string.IsNullOrWhiteSpace(request.LoaiYeuCau))
            return "Chưa xác định";

        return request.LoaiYeuCau switch
        {
            "XEM_PHONG" => "Xem phòng",
            "THUE_NGAY" => "Thuê ngay",
            _ => request.LoaiYeuCau
        };
    }
}
