using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Data;
using QL_PhongTro.Models;
using QL_PhongTro.ViewModels;

namespace QL_PhongTro.Services;

// The only place allowed to move a request between states. Every method opens its own
// transaction, checks ownership before writing, and leaves the request, its history row,
// the in-app notification and the admin activity log inside one atomic change.
public sealed class YeuCauThueService(AppDbContext db, ITimeProvider clock)
{
    private static readonly string[] LyDoTuChoi =
    [
        "DA_CO_KHACH_THUE", "KHONG_PHU_HOP_SO_NGUOI", "KHACH_KHONG_LIEN_LAC_DUOC", "LY_DO_KHAC"
    ];

    public static IReadOnlyList<string> DanhSachLyDoTuChoi() => LyDoTuChoi;

    public static string LyDoLabel(string lyDo) => lyDo switch
    {
        "DA_CO_KHACH_THUE" => "Đã có khách thuê",
        "KHONG_PHU_HOP_SO_NGUOI" => "Không phù hợp số người",
        "KHACH_KHONG_LIEN_LAC_DUOC" => "Khách không liên lạc được",
        "LY_DO_KHAC" => "Lý do khác",
        _ => lyDo
    };

    public DateTime UtcHienTai() => clock.UtcNow;

    public static string HienThoiGio(DateTime utc) =>
        utc.AddHours(7).ToString("dd/MM/yyyy HH:mm");

    private async Task<YeuCauThue?> LoadAsync(int id, CancellationToken ct) =>
        await db.YeuCauThues.FirstOrDefaultAsync(x => x.Id == id, ct);

    // The landlord of the room the request is about. Mirrors DichVuService.SoHuuToaNhaAsync
    // so a request can never be handled by a landlord who does not own the room.
    private async Task<bool> ChuNhaCuaPhongAsync(int accountId, int phongId, CancellationToken ct) =>
        await (from room in db.PhongTros
               join building in db.ToaNhas on room.ToaNhaId equals building.Id
               join account in db.TaiKhoans on building.ChuNhaId equals account.Id
               where room.Id == phongId && building.ChuNhaId == accountId && building.DangHoatDong
                     && account.DangHoatDong && account.VaiTro == "CHU_NHA" && !account.IsDeleted
               select room.Id).AnyAsync(ct);

    public async Task<bool> ChuNhaCuaYeuCauAsync(int accountId, int yeuCauId, CancellationToken ct) =>
        await db.YeuCauThues.AsNoTracking()
            .Where(x => x.Id == yeuCauId)
            .SelectMany(x => db.PhongTros.Where(p => p.Id == x.PhongId)
                .Where(p => db.ToaNhas.Any(b => b.Id == p.ToaNhaId && b.ChuNhaId == accountId && b.DangHoatDong)))
            .AnyAsync(ct);

    // KHACH_THUE has WRITE on the YEU_CAU_THUE module, so ownership must be re-checked here
    // instead of trusting the controller attribute.
    public async Task<bool> KhachChuYeuCauAsync(int accountId, int yeuCauId, CancellationToken ct) =>
        await (from request in db.YeuCauThues
               join profile in db.KhachThues on request.KhachThueId equals profile.Id
               join account in db.TaiKhoans on profile.TaiKhoanId equals account.Id
               where request.Id == yeuCauId && account.Id == accountId && account.DangHoatDong
                     && !account.IsDeleted && account.VaiTro == "KHACH_THUE"
               select request.Id).AnyAsync(ct);

    public async Task<bool> QuyenXemAsync(int accountId, int yeuCauId, CancellationToken ct) =>
        await ChuNhaCuaYeuCauAsync(accountId, yeuCauId, ct) || await KhachChuYeuCauAsync(accountId, yeuCauId, ct);

    private async Task<TaiKhoan> ActorAsync(int accountId, CancellationToken ct) =>
        await db.TaiKhoans.AsNoTracking().SingleOrDefaultAsync(a => a.Id == accountId, ct)
        ?? throw new UnauthorizedAccessException("Không xác minh được tài khoản thực hiện.");

    // History + notification are written in the same transaction as the status change, so a
    // rejected operation can never leave a half-written record behind.
    protected void GhiLichSu(YeuCauThue yeuCau, TaiKhoan actor, string hanhDong, string? trangThaiCu,
        DateTime? lichHenCu, DateTime? lichHenMoi, DateTime now)
    {
        db.YeuCauThueLichSus.Add(new YeuCauThueLichSu
        {
            YeuCauThueId = yeuCau.Id,
            TrangThaiCu = trangThaiCu,
            TrangThaiMoi = yeuCau.TrangThai,
            HanhDong = hanhDong,
            NguoiThucHienId = actor.Id,
            TenNguoiThucHien = actor.HoTen,
            VaiTroLucThucHien = actor.VaiTro,
            LichHenCu = lichHenCu,
            LichHenMoi = lichHenMoi,
            LyDoTuChoi = yeuCau.LyDoTuChoi,
            GhiChuTuChoi = yeuCau.GhiChuTuChoi,
            ThoiDiem = now
        });
    }

    // Never notify the account that just performed the action: they already know.
    protected void ThongBao(int? nguoiNhanId, int actorId, string loai, string tieuDe, string noiDung, int yeuCauId)
    {
        // A tenant profile with no linked account cannot receive in-app mail; skip instead of failing.
        if (nguoiNhanId is not int nhan || nhan == actorId) return;
        db.ThongBaos.Add(new ThongBao
        {
            NguoiNhanId = nhan,
            Loai = loai,
            TieuDe = tieuDe,
            NoiDung = noiDung,
            DuongDan = $"/YeuCauThue/Detail/{yeuCauId}",
            NgayTao = UtcHienTai()
        });
    }

    protected int? TaiKhoanCuaKhach(YeuCauThue yeuCau) =>
        db.KhachThues.AsNoTracking().Where(x => x.Id == yeuCau.KhachThueId).Select(x => x.TaiKhoanId).Single();

    public async Task XacNhanLichAsync(int yeuCauId, int accountId, DateTime lichHen, CancellationToken ct)
    {
        if (lichHen <= UtcHienTai()) throw new InvalidOperationException("Ngày giờ hẹn phải sau thời điểm hiện tại.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var actor = await ActorAsync(accountId, ct);
        var yeuCau = await LoadAsync(yeuCauId, ct) ?? throw new KeyNotFoundException("Yêu cầu không tồn tại.");
        if (!await ChuNhaCuaPhongAsync(accountId, yeuCau.PhongId, ct))
            throw new UnauthorizedAccessException("Chỉ chủ nhà của phòng này mới xác nhận được lịch hẹn.");
        if (yeuCau.TrangThai != YeuCauThueTrangThai.Moi)
            throw new InvalidOperationException("Chỉ yêu cầu đang chờ xác nhận mới xác nhận lịch được.");

        var now = UtcHienTai();
        yeuCau.LichHen = lichHen;
        yeuCau.TrangThai = YeuCauThueTrangThai.DaHenLich;
        yeuCau.NguoiXuLyId = accountId;
        yeuCau.NgayXuLy = now;
        yeuCau.PhienBan++;
        GhiLichSu(yeuCau, actor, HanhDongYeuCau.XacNhan, YeuCauThueTrangThai.Moi, null, lichHen, now);
        ThongBao(TaiKhoanCuaKhach(yeuCau), accountId, LoaiThongBao.YeuCauXacNhan,
            "Lịch hẹn đã được xác nhận",
            $"Chủ nhà đã xác nhận lịch hẹn {HienThoiGio(lichHen)} cho yêu cầu {yeuCau.MaYeuCau}.", yeuCau.Id);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
}
