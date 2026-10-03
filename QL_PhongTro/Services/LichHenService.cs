using System.Globalization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Data;
using QL_PhongTro.Models;

namespace QL_PhongTro.Services;

public sealed class LichHenConflictException(string message) : Exception(message);

/// <summary>
/// Everything S2-08 needs about one request, read with plain SQL. The request row is
/// owned by S2-06, so this type deliberately does not reuse the YeuCauThue entity and
/// LichHenService never registers it in the DbContext.
/// </summary>
public sealed record LichHenYeuCau
{
    public int Id { get; init; }
    public string MaYeuCau { get; init; } = string.Empty;
    public string LoaiYeuCau { get; init; } = string.Empty;
    public string TrangThai { get; init; } = string.Empty;
    public DateTime NgayTao { get; init; }
    public DateTime? LichHen { get; init; }
    public string? LyDoTuChoi { get; init; }

    /// <summary>Free text the landlord left when rejecting, read back from the newest history row.</summary>
    public string? GhiChuTuChoi { get; init; }

    /// <summary>
    /// Slot the request held before the last reschedule. yeu_cau_thue has no such column and
    /// belongs to S2-06, so the previous slot is read back from yeu_cau_thue_lich_su.
    /// </summary>
    public DateTime? LichHenCu { get; init; }

    /// <summary>True once the request has been rescheduled at least once.</summary>
    public bool DaDoiLich { get; init; }

    /// <summary>
    /// Current room status and version, read through tin_dang. Approving an instant-rental
    /// request moves the room to DA_DAT_COC, so both values are needed to guard the update.
    /// </summary>
    public string TrangThaiPhong { get; init; } = string.Empty;

    public int PhienBanPhong { get; init; }

    public int? NguoiXuLyId { get; init; }
    public DateTime? NgayXuLy { get; init; }
    public int PhienBan { get; init; }
    public string NgayMongMuon { get; init; } = string.Empty;
    public int SoNguoiDuKien { get; init; }
    public string? LoiNhan { get; init; }

    public int PhongId { get; init; }
    public string MaPhong { get; init; } = string.Empty;
    public string TenToaNha { get; init; } = string.Empty;
    public string? DiaChiToaNha { get; init; }
    public int ChuNhaId { get; init; }
    public string TenChuNha { get; init; } = string.Empty;

    public int KhachThueId { get; init; }
    public int? TaiKhoanKhachId { get; init; }
    public string TenKhach { get; init; } = string.Empty;
    public string? SoDienThoaiKhach { get; init; }
}

public class LichHenService(AppDbContext db, ITimeProvider clock)
{
    /// <summary>Two appointments of the same room closer than this are reported as a clash.</summary>
    public static readonly TimeSpan KhoangTrungLich = TimeSpan.FromMinutes(30);

    public const int GhiChuToiDa = 500;
    public const int GhiChuToiThieuLyDoKhac = 5;

    private const string DocSql = """
        SELECT r.id, r.ma_yeu_cau, r.loai_yeu_cau, r.trang_thai, r.ngay_tao, r.lich_hen,
               r.ly_do_tu_choi, r.nguoi_xu_ly_id, r.ngay_xu_ly, r.phien_ban,
               r.ngay_mong_muon, r.so_nguoi_du_kien, r.loi_nhan,
               p.id, p.ma_phong, b.ten_toa_nha, b.dia_chi, b.chu_nha_id, c.ho_ten AS ten_chu_nha,
               k.id AS khach_thue_id, k.tai_khoan_id, k.ho_ten AS ten_khach, t.so_dien_thoai,
               (SELECT h.ghi_chu_tu_choi FROM yeu_cau_thue_lich_su h
                WHERE h.yeu_cau_thue_id = r.id AND h.ghi_chu_tu_choi IS NOT NULL
                ORDER BY h.id DESC LIMIT 1) AS ghi_chu_tu_choi,
               (SELECT h.lich_hen_cu FROM yeu_cau_thue_lich_su h
                WHERE h.yeu_cau_thue_id = r.id AND h.hanh_dong = $hanhDongDoiLich AND h.lich_hen_cu IS NOT NULL
                ORDER BY h.id DESC LIMIT 1) AS lich_hen_cu,
               EXISTS (SELECT 1 FROM yeu_cau_thue_lich_su h
                       WHERE h.yeu_cau_thue_id = r.id AND h.hanh_dong = $hanhDongDoiLich) AS da_doi_lich,
               p.trang_thai AS trang_thai_phong, p.phien_ban AS phien_ban_phong
        FROM yeu_cau_thue r
        JOIN tin_dang d ON d.id = r.tin_dang_id
        JOIN phong_tro p ON p.id = d.phong_id
        JOIN toa_nha b ON b.id = p.toa_nha_id
        JOIN khach_thue k ON k.id = r.khach_thue_id
        LEFT JOIN tai_khoan c ON c.id = b.chu_nha_id
        LEFT JOIN tai_khoan t ON t.id = k.tai_khoan_id
        WHERE r.id = $id;
        """;

    public DateTime UtcHienTai() => DateTime.SpecifyKind(clock.UtcNow, DateTimeKind.Utc);

    /// <summary>Shows a stored UTC instant in Vietnam time, which is what users read.</summary>
    public static string HienThoiGio(DateTime utc) => TimeZoneInfo.ConvertTimeFromUtc(
        DateTime.SpecifyKind(utc, DateTimeKind.Utc),
        TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh")).ToString("dd/MM/yyyy HH:mm");

    // SQLite has no boolean or date type, so every value crosses the boundary as text.
    internal static string SqlUtc(DateTime utc) =>
        utc.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

    private static DateTime? DocUtc(string? raw) => string.IsNullOrWhiteSpace(raw) ? null : DateTime.Parse(raw, CultureInfo.InvariantCulture);

    private async Task<LichHenYeuCau?> LoadAsync(int id, CancellationToken ct)
    {
        await db.Database.OpenConnectionAsync();
        using var cmd = ((SqliteConnection)db.Database.GetDbConnection()).CreateCommand();
        cmd.CommandText = DocSql;
        cmd.Parameters.AddWithValue("$id", id);
        cmd.Parameters.AddWithValue("$hanhDongDoiLich", HanhDongYeuCau.DoiLich);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;
        return new LichHenYeuCau
        {
            Id = reader.GetInt32(0),
            MaYeuCau = reader.GetString(1),
            LoaiYeuCau = reader.GetString(2),
            TrangThai = reader.GetString(3),
            NgayTao = DocUtc(reader.GetString(4)) ?? default,
            LichHen = DocUtc(reader.IsDBNull(5) ? null : reader.GetString(5)),
            LyDoTuChoi = reader.IsDBNull(6) ? null : reader.GetString(6),
            NguoiXuLyId = reader.IsDBNull(7) ? null : reader.GetInt32(7),
            NgayXuLy = DocUtc(reader.IsDBNull(8) ? null : reader.GetString(8)),
            PhienBan = reader.GetInt32(9),
            NgayMongMuon = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
            SoNguoiDuKien = reader.GetInt32(11),
            LoiNhan = reader.IsDBNull(12) ? null : reader.GetString(12),
            PhongId = reader.GetInt32(13),
            MaPhong = reader.GetString(14),
            TenToaNha = reader.IsDBNull(15) ? string.Empty : reader.GetString(15),
            DiaChiToaNha = reader.IsDBNull(16) ? null : reader.GetString(16),
            ChuNhaId = reader.GetInt32(17),
            TenChuNha = reader.IsDBNull(18) ? string.Empty : reader.GetString(18),
            KhachThueId = reader.GetInt32(19),
            TaiKhoanKhachId = reader.IsDBNull(20) ? null : reader.GetInt32(20),
            TenKhach = reader.IsDBNull(21) ? string.Empty : reader.GetString(21),
            SoDienThoaiKhach = reader.IsDBNull(22) ? null : reader.GetString(22),
            GhiChuTuChoi = reader.IsDBNull(23) ? null : reader.GetString(23),
            LichHenCu = DocUtc(reader.IsDBNull(24) ? null : reader.GetString(24)),
            DaDoiLich = !reader.IsDBNull(25) && reader.GetInt64(25) != 0,
            TrangThaiPhong = reader.GetString(26),
            PhienBanPhong = reader.GetInt32(27)
        };
    }

    /// <summary>Owner check walks request to listing to room to building, never yeu_cau_thue.phong_id.</summary>
    public async Task<bool> ChuNhaCuaYeuCauAsync(int accountId, int yeuCauId, CancellationToken ct) =>
        await LoadAsync(yeuCauId, ct) is { } row && row.ChuNhaId == accountId;

    /// <summary>A tenant only reaches a request linked to their own profile.</summary>
    public async Task<bool> KhachChuYeuCauAsync(int accountId, int yeuCauId, CancellationToken ct) =>
        await LoadAsync(yeuCauId, ct) is { } row && row.TaiKhoanKhachId == accountId;

    public Task<LichHenYeuCau?> DocAsync(int yeuCauId, CancellationToken ct) => LoadAsync(yeuCauId, ct);

    /// <summary>One other confirmed appointment of the same room that clashes with the picked slot.</summary>
    public sealed record LichTrung
    {
        public int YeuCauId { get; init; }
        public string MaYeuCau { get; init; } = string.Empty;
        public string TenKhach { get; init; } = string.Empty;
        public DateTime LichHen { get; init; }
        public string TenPhong { get; init; } = string.Empty;
    }

    private const string LichTrungSql = """
        SELECT r.id, r.ma_yeu_cau, r.lich_hen, p.ma_phong, k.ho_ten
        FROM yeu_cau_thue r
        JOIN tin_dang d ON d.id = r.tin_dang_id
        JOIN phong_tro p ON p.id = d.phong_id
        JOIN khach_thue k ON k.id = r.khach_thue_id
        WHERE d.phong_id = $phongId AND r.id <> $yeuCauId
              AND r.lich_hen IS NOT NULL
              AND r.lich_hen >= $batDau AND r.lich_hen <= $ketThuc
              AND r.trang_thai IN ($daHenLich, $daDuyet)
        ORDER BY r.lich_hen;
        """;

    /// <summary>
    /// Advisory lookup for the clash warning. It reads yeu_cau_thue the same way the rest of
    /// S2-08 does, through tin_dang to reach the room, and stays in raw SQL so nothing here
    /// depends on the S2-06 entity. Only DA_HEN_LICH and DA_DUYET hold a slot, and the request
    /// under review is excluded so it never warns about itself. The window is symmetric and
    /// inclusive, so a booking exactly 30 minutes away is still a clash worth showing.
    /// </summary>
    public async Task<List<LichTrung>> TimLichTrungAsync(int phongId, DateTime lichHenUtc, int yeuCauIdHienTai, CancellationToken ct)
    {
        await db.Database.OpenConnectionAsync();
        using var cmd = ((SqliteConnection)db.Database.GetDbConnection()).CreateCommand();
        cmd.CommandText = LichTrungSql;
        cmd.Parameters.AddWithValue("$phongId", phongId);
        cmd.Parameters.AddWithValue("$yeuCauId", yeuCauIdHienTai);
        cmd.Parameters.AddWithValue("$batDau", SqlUtc(lichHenUtc - KhoangTrungLich));
        cmd.Parameters.AddWithValue("$ketThuc", SqlUtc(lichHenUtc + KhoangTrungLich));
        cmd.Parameters.AddWithValue("$daHenLich", LichHenTrangThai.DaHenLich);
        cmd.Parameters.AddWithValue("$daDuyet", LichHenTrangThai.DaDuyet);

        var ketQua = new List<LichTrung>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            ketQua.Add(new LichTrung
            {
                YeuCauId = reader.GetInt32(0),
                MaYeuCau = reader.GetString(1),
                LichHen = DocUtc(reader.IsDBNull(2) ? null : reader.GetString(2)) ?? default,
                TenPhong = reader.GetString(3),
                TenKhach = reader.IsDBNull(4) ? string.Empty : reader.GetString(4)
            });
        return ketQua;
    }

    /// <summary>
    /// Confirms a viewing slot. Only the six columns S2-06 already provides are written;
    /// the previous schedule stays in yeu_cau_thue_lich_su. The conditional update on
    /// phien_ban is what stops a second landlord click from overwriting a newer decision.
    /// </summary>
    public async Task XacNhanLichAsync(int yeuCauId, int accountId, DateTime lichHenUtc, CancellationToken ct)
    {
        var yeuCau = await LoadAsync(yeuCauId, ct) ?? throw new KeyNotFoundException("Yêu cầu không tồn tại.");
        if (yeuCau.ChuNhaId != accountId)
            throw new UnauthorizedAccessException("Chỉ chủ nhà của phòng này mới xác nhận được lịch hẹn.");
        if (yeuCau.TrangThai != LichHenTrangThai.Moi)
            throw new InvalidOperationException("Yêu cầu này không còn ở trạng thái chờ xác nhận.");
        var now = UtcHienTai();
        if (lichHenUtc <= now) throw new InvalidOperationException("Lịch hẹn phải nằm trong tương lai.");

        await db.Database.OpenConnectionAsync();
        using var tx = ((SqliteConnection)db.Database.GetDbConnection()).BeginTransaction(deferred: false);
        using (var update = tx.Connection!.CreateCommand())
        {
            update.Transaction = tx;
            update.CommandText = """
                UPDATE yeu_cau_thue
                SET trang_thai = $trangThai, lich_hen = $lichHen, nguoi_xu_ly_id = $nguoiXuLy,
                    ngay_xu_ly = $now, phien_ban = phien_ban + 1
                WHERE id = $id AND phien_ban = $phienBan AND trang_thai = $trangThaiCu;
                """;
            update.Parameters.AddWithValue("$trangThai", LichHenTrangThai.DaHenLich);
            update.Parameters.AddWithValue("$lichHen", SqlUtc(lichHenUtc));
            update.Parameters.AddWithValue("$nguoiXuLy", accountId);
            update.Parameters.AddWithValue("$now", SqlUtc(now));
            update.Parameters.AddWithValue("$id", yeuCauId);
            update.Parameters.AddWithValue("$phienBan", yeuCau.PhienBan);
            update.Parameters.AddWithValue("$trangThaiCu", LichHenTrangThai.Moi);
            if (await update.ExecuteNonQueryAsync(ct) != 1)
                throw new LichHenConflictException("Yêu cầu vừa được cập nhật bởi người khác. Vui lòng tải lại trang.");
        }

        var (ten, vaiTro) = await DocNguoiThucHienAsync(tx, accountId, ct);
        await GhiLichSuAsync(tx, yeuCauId, LichHenTrangThai.Moi, LichHenTrangThai.DaHenLich,
            HanhDongYeuCau.XacNhan, accountId, ten, vaiTro, null, lichHenUtc, null, null, now, ct);
        await GhiThongBaoAsync(tx, yeuCau, LoaiThongBaoYeuCau.XacNhanLich,
            "Lịch hẹn đã được xác nhận",
            $"Chủ nhà đã xác nhận lịch hẹn {HienThoiGio(lichHenUtc)} cho yêu cầu {yeuCau.MaYeuCau}.",
            now, ct);
        tx.Commit();
    }

    /// <summary>
    /// Moves a booked slot to a new time. yeu_cau_thue has no lich_hen_cu or da_doi_lich
    /// column and belongs to S2-06, so the previous slot is kept in yeu_cau_thue_lich_su and
    /// read back from there. Rescheduling is a business no-op: trang_thai stays DA_HEN_LICH,
    /// because the request still holds a slot, only at another time.
    /// </summary>
    public async Task DoiLichAsync(int yeuCauId, int accountId, DateTime lichHenMoiUtc, CancellationToken ct)
    {
        var yeuCau = await LoadAsync(yeuCauId, ct) ?? throw new KeyNotFoundException("Yêu cầu không tồn tại.");
        if (yeuCau.ChuNhaId != accountId)
            throw new UnauthorizedAccessException("Chỉ chủ nhà của phòng này mới đổi được lịch hẹn.");
        if (yeuCau.TrangThai != LichHenTrangThai.DaHenLich || yeuCau.LichHen is not { } lichHenCu)
            throw new InvalidOperationException("Chỉ yêu cầu đã có lịch hẹn mới đổi lịch được.");
        if (lichHenMoiUtc == lichHenCu)
            throw new InvalidOperationException("Ngày giờ mới phải khác ngày giờ đã hẹn.");
        var now = UtcHienTai();
        if (lichHenMoiUtc <= now) throw new InvalidOperationException("Ngày giờ mới phải sau thời điểm hiện tại.");

        await db.Database.OpenConnectionAsync();
        using var tx = ((SqliteConnection)db.Database.GetDbConnection()).BeginTransaction(deferred: false);
        using (var update = tx.Connection!.CreateCommand())
        {
            update.Transaction = tx;
            update.CommandText = """
                UPDATE yeu_cau_thue
                SET lich_hen = $lichHen, nguoi_xu_ly_id = $nguoiXuLy, ngay_xu_ly = $now,
                    phien_ban = phien_ban + 1
                WHERE id = $id AND phien_ban = $phienBan AND trang_thai = $trangThai
                      AND lich_hen = $lichHenCu;
                """;
            update.Parameters.AddWithValue("$lichHen", SqlUtc(lichHenMoiUtc));
            update.Parameters.AddWithValue("$nguoiXuLy", accountId);
            update.Parameters.AddWithValue("$now", SqlUtc(now));
            update.Parameters.AddWithValue("$id", yeuCauId);
            update.Parameters.AddWithValue("$phienBan", yeuCau.PhienBan);
            update.Parameters.AddWithValue("$trangThai", LichHenTrangThai.DaHenLich);
            update.Parameters.AddWithValue("$lichHenCu", SqlUtc(lichHenCu));
            if (await update.ExecuteNonQueryAsync(ct) != 1)
                throw new LichHenConflictException("Yêu cầu vừa được cập nhật bởi người khác. Vui lòng tải lại trang.");
        }

        var (ten, vaiTro) = await DocNguoiThucHienAsync(tx, accountId, ct);
        await GhiLichSuAsync(tx, yeuCauId, LichHenTrangThai.DaHenLich, LichHenTrangThai.DaHenLich,
            HanhDongYeuCau.DoiLich, accountId, ten, vaiTro, lichHenCu, lichHenMoiUtc, null, null, now, ct);
        await GhiThongBaoAsync(tx, yeuCau, LoaiThongBaoYeuCau.YeuCauDoiLich,
            "Lịch hẹn đã được đổi",
            $"Lịch hẹn cũ {HienThoiGio(lichHenCu)} của yêu cầu {yeuCau.MaYeuCau} đã đổi sang {HienThoiGio(lichHenMoiUtc)}.",
            now, ct);
        tx.Commit();
    }

    /// <summary>
    /// Approves an instant-rental request and moves the room to Đã đặt cọc. Only a THUE_NGAY
    /// request can be approved this way: a viewing request still needs an appointment or a
    /// rejection. Request and room move inside one transaction, so a failure between them rolls
    /// back instead of leaving a request approved for a room that is still free.
    /// </summary>
    public async Task DuyetThueNgayAsync(int yeuCauId, int accountId, CancellationToken ct)
    {
        var yeuCau = await LoadAsync(yeuCauId, ct) ?? throw new KeyNotFoundException("Yêu cầu không tồn tại.");
        if (yeuCau.ChuNhaId != accountId)
            throw new UnauthorizedAccessException("Chỉ chủ nhà của phòng này mới duyệt được yêu cầu.");
        if (yeuCau.LoaiYeuCau != LichHenTrangThai.LoaiThueNgay)
            throw new InvalidOperationException("Chỉ yêu cầu loại Thuê ngay mới duyệt theo cách này.");
        if (yeuCau.TrangThai is not (LichHenTrangThai.Moi or LichHenTrangThai.DaHenLich))
            throw new InvalidOperationException("Yêu cầu này không còn ở trạng thái cho phép duyệt.");
        if (yeuCau.TrangThaiPhong != LichHenTrangThai.PhongTrong)
            throw new InvalidOperationException("Phòng đã không còn trống nên không thể duyệt đặt cọc.");
        var now = UtcHienTai();

        await db.Database.OpenConnectionAsync();
        using var tx = ((SqliteConnection)db.Database.GetDbConnection()).BeginTransaction(deferred: false);
        using (var update = tx.Connection!.CreateCommand())
        {
            update.Transaction = tx;
            update.CommandText = """
                UPDATE yeu_cau_thue
                SET trang_thai = $trangThai, nguoi_xu_ly_id = $nguoiXuLy, ngay_xu_ly = $now,
                    phien_ban = phien_ban + 1
                WHERE id = $id AND phien_ban = $phienBan AND trang_thai = $trangThaiCu;
                UPDATE phong_tro
                SET trang_thai = $phongTrangThai, phien_ban = phien_ban + 1
                WHERE id = $phongId AND phien_ban = $phienBanPhong AND trang_thai = $phongTrong;
                """;
            update.Parameters.AddWithValue("$trangThai", LichHenTrangThai.DaDuyet);
            update.Parameters.AddWithValue("$nguoiXuLy", accountId);
            update.Parameters.AddWithValue("$now", SqlUtc(now));
            update.Parameters.AddWithValue("$id", yeuCauId);
            update.Parameters.AddWithValue("$phienBan", yeuCau.PhienBan);
            update.Parameters.AddWithValue("$trangThaiCu", yeuCau.TrangThai);
            update.Parameters.AddWithValue("$phongTrangThai", LichHenTrangThai.PhongDaDatCoc);
            update.Parameters.AddWithValue("$phongId", yeuCau.PhongId);
            update.Parameters.AddWithValue("$phienBanPhong", yeuCau.PhienBanPhong);
            update.Parameters.AddWithValue("$phongTrong", LichHenTrangThai.PhongTrong);
            if (await update.ExecuteNonQueryAsync(ct) != 2)
                throw new LichHenConflictException("Yêu cầu hoặc phòng vừa được cập nhật bởi người khác. Vui lòng tải lại trang.");
        }

        var (ten, vaiTro) = await DocNguoiThucHienAsync(tx, accountId, ct);
        await GhiLichSuAsync(tx, yeuCauId, yeuCau.TrangThai, LichHenTrangThai.DaDuyet,
            HanhDongYeuCau.DuyetThueNgay, accountId, ten, vaiTro, yeuCau.LichHen, yeuCau.LichHen, null, null, now, ct);
        await GhiThongBaoAsync(tx, yeuCau, LoaiThongBaoYeuCau.YeuCauDuyet,
            "Yêu cầu thuê ngay đã được duyệt",
            $"Yêu cầu {yeuCau.MaYeuCau} đã được duyệt. Phòng {yeuCau.MaPhong} chuyển sang Đã đặt cọc.",
            now, ct);
        tx.Commit();
    }

    /// <summary>
    /// Rejects a request with a reason and an optional note. yeu_cau_thue has no note column
    /// and S2-06 owns that table, so the note lives in yeu_cau_thue_lich_su and is read back
    /// from there for the detail page. A landlord may still walk away from a slot they already
    /// confirmed, so DA_HEN_LICH is accepted alongside MOI.
    /// </summary>
    public async Task TuChoiAsync(int yeuCauId, int accountId, string lyDo, string? ghiChu, CancellationToken ct)
    {
        var note = ghiChu?.Trim() ?? string.Empty;
        ValidateLyDoTuChoi(lyDo, note);

        var yeuCau = await LoadAsync(yeuCauId, ct) ?? throw new KeyNotFoundException("Yêu cầu không tồn tại.");
        if (yeuCau.ChuNhaId != accountId)
            throw new UnauthorizedAccessException("Chỉ chủ nhà của phòng này mới từ chối được yêu cầu.");
        if (yeuCau.TrangThai is not (LichHenTrangThai.Moi or LichHenTrangThai.DaHenLich))
            throw new InvalidOperationException("Yêu cầu này không còn ở trạng thái cho phép từ chối.");
        var now = UtcHienTai();

        await db.Database.OpenConnectionAsync();
        using var tx = ((SqliteConnection)db.Database.GetDbConnection()).BeginTransaction(deferred: false);
        using (var update = tx.Connection!.CreateCommand())
        {
            update.Transaction = tx;
            update.CommandText = """
                UPDATE yeu_cau_thue
                SET trang_thai = $trangThai, ly_do_tu_choi = $lyDo, nguoi_xu_ly_id = $nguoiXuLy,
                    ngay_xu_ly = $now, phien_ban = phien_ban + 1
                WHERE id = $id AND phien_ban = $phienBan AND trang_thai = $trangThaiCu;
                """;
            update.Parameters.AddWithValue("$trangThai", LichHenTrangThai.TuChoi);
            update.Parameters.AddWithValue("$lyDo", lyDo);
            update.Parameters.AddWithValue("$nguoiXuLy", accountId);
            update.Parameters.AddWithValue("$now", SqlUtc(now));
            update.Parameters.AddWithValue("$id", yeuCauId);
            update.Parameters.AddWithValue("$phienBan", yeuCau.PhienBan);
            update.Parameters.AddWithValue("$trangThaiCu", yeuCau.TrangThai);
            if (await update.ExecuteNonQueryAsync(ct) != 1)
                throw new LichHenConflictException("Yêu cầu vừa được cập nhật bởi người khác. Vui lòng tải lại trang.");
        }

        var (ten, vaiTro) = await DocNguoiThucHienAsync(tx, accountId, ct);
        await GhiLichSuAsync(tx, yeuCauId, yeuCau.TrangThai, LichHenTrangThai.TuChoi,
            HanhDongYeuCau.TuChoi, accountId, ten, vaiTro, yeuCau.LichHen, yeuCau.LichHen,
            lyDo, note.Length == 0 ? null : note, now, ct);
        await GhiThongBaoAsync(tx, yeuCau, LoaiThongBaoYeuCau.YeuCauTuChoi,
            "Yêu cầu đã bị từ chối",
            $"Yêu cầu {yeuCau.MaYeuCau} đã bị từ chối. Lý do: {LyDoLabel(lyDo)}.",
            now, ct);
        tx.Commit();
    }

    private static void ValidateLyDoTuChoi(string lyDo, string note)
    {
        if (!LyDoTuChoi.All.Contains(lyDo))
            throw new InvalidOperationException("Lý do từ chối không hợp lệ.");
        if (lyDo == LyDoTuChoi.Khac && note.Length < GhiChuToiThieuLyDoKhac)
            throw new InvalidOperationException("Chọn lý do khác thì phải nhập ghi chú ít nhất 5 ký tự.");
        if (note.Length > GhiChuToiDa)
            throw new InvalidOperationException($"Ghi chú không được dài quá {GhiChuToiDa} ký tự.");
    }

    public static string LyDoLabel(string lyDo) => lyDo switch
    {
        LyDoTuChoi.DaCoKhachThue => "Phòng đã có khách thuê",
        LyDoTuChoi.KhongPhuHop => "Không phù hợp số người",
        LyDoTuChoi.KhachKhongLienLacDuoc => "Khách không liên lạc được",
        LyDoTuChoi.Khac => "Lý do khác",
        _ => lyDo
    };

    private static async Task<(string? Ten, string? VaiTro)> DocNguoiThucHienAsync(SqliteTransaction tx, int accountId, CancellationToken ct)
    {
        using var cmd = tx.Connection!.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = "SELECT ho_ten, vai_tro FROM tai_khoan WHERE id = $id;";
        cmd.Parameters.AddWithValue("$id", accountId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return (null, null);
        return (reader.IsDBNull(0) ? null : reader.GetString(0), reader.IsDBNull(1) ? null : reader.GetString(1));
    }

    private static async Task GhiLichSuAsync(SqliteTransaction tx, int yeuCauId, string? trangThaiCu, string trangThaiMoi,
        string hanhDong, int? nguoiThucHienId, string? ten, string? vaiTro, DateTime? lichHenCu, DateTime? lichHenMoi,
        string? lyDoTuChoi, string? ghiChuTuChoi, DateTime thoiDiem, CancellationToken ct)
    {
        using var cmd = tx.Connection!.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = """
            INSERT INTO yeu_cau_thue_lich_su
                (yeu_cau_thue_id, trang_thai_cu, trang_thai_moi, hanh_dong, nguoi_thuc_hien_id,
                 ten_nguoi_thuc_hien, vai_tro_luc_thuc_hien, lich_hen_cu, lich_hen_moi,
                 ly_do_tu_choi, ghi_chu_tu_choi, thoi_diem)
            VALUES ($id, $cu, $moi, $hanhDong, $nguoi, $ten, $vaiTro, $cuLich, $moiLich,
                 $lyDo, $ghiChu, $thoiDiem);
            """;
        cmd.Parameters.AddWithValue("$id", yeuCauId);
        cmd.Parameters.AddWithValue("$cu", (object?)trangThaiCu ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$moi", trangThaiMoi);
        cmd.Parameters.AddWithValue("$hanhDong", hanhDong);
        cmd.Parameters.AddWithValue("$nguoi", (object?)nguoiThucHienId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$ten", (object?)ten ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$vaiTro", (object?)vaiTro ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$cuLich", lichHenCu is { } cu ? SqlUtc(cu) : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$moiLich", lichHenMoi is { } moi ? SqlUtc(moi) : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$lyDo", (object?)lyDoTuChoi ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$ghiChu", (object?)ghiChuTuChoi ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$thoiDiem", SqlUtc(thoiDiem));
        await cmd.ExecuteNonQueryAsync(ct);
    }

    // A profile without a linked account means the tenant cannot be reached; the
    // scheduling decision still stands, so the notification is skipped, not fatal.
    private static async Task GhiThongBaoAsync(SqliteTransaction tx, LichHenYeuCau yeuCau, string loai,
        string tieuDe, string noiDung, DateTime ngayTao, CancellationToken ct)
    {
        if (yeuCau.TaiKhoanKhachId is not { } nguoiNhan) return;
        using var cmd = tx.Connection!.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = """
            INSERT INTO yeu_cau_thue_thong_bao
                (yeu_cau_thue_id, nguoi_nhan_id, loai, tieu_de, noi_dung, duong_dan, da_doc, ngay_tao)
            VALUES ($id, $nguoiNhan, $loai, $tieuDe, $noiDung, $duongDan, 0, $ngayTao);
            """;
        cmd.Parameters.AddWithValue("$id", yeuCau.Id);
        cmd.Parameters.AddWithValue("$nguoiNhan", nguoiNhan);
        cmd.Parameters.AddWithValue("$loai", loai);
        cmd.Parameters.AddWithValue("$tieuDe", tieuDe);
        cmd.Parameters.AddWithValue("$noiDung", noiDung);
        cmd.Parameters.AddWithValue("$duongDan", $"/LichHen/ChiTiet/{yeuCau.Id}");
        cmd.Parameters.AddWithValue("$ngayTao", SqlUtc(ngayTao));
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
