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
    private const string DocSql = """
        SELECT r.id, r.ma_yeu_cau, r.loai_yeu_cau, r.trang_thai, r.ngay_tao, r.lich_hen,
               r.ly_do_tu_choi, r.nguoi_xu_ly_id, r.ngay_xu_ly, r.phien_ban,
               r.ngay_mong_muon, r.so_nguoi_du_kien, r.loi_nhan,
               p.id, p.ma_phong, b.ten_toa_nha, b.dia_chi, b.chu_nha_id, c.ho_ten AS ten_chu_nha,
               k.id AS khach_thue_id, k.tai_khoan_id, k.ho_ten AS ten_khach, t.so_dien_thoai
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
            SoDienThoaiKhach = reader.IsDBNull(22) ? null : reader.GetString(22)
        };
    }

    /// <summary>Owner check walks request to listing to room to building, never yeu_cau_thue.phong_id.</summary>
    public async Task<bool> ChuNhaCuaYeuCauAsync(int accountId, int yeuCauId, CancellationToken ct) =>
        await LoadAsync(yeuCauId, ct) is { } row && row.ChuNhaId == accountId;

    /// <summary>A tenant only reaches a request linked to their own profile.</summary>
    public async Task<bool> KhachChuYeuCauAsync(int accountId, int yeuCauId, CancellationToken ct) =>
        await LoadAsync(yeuCauId, ct) is { } row && row.TaiKhoanKhachId == accountId;

    public Task<LichHenYeuCau?> DocAsync(int yeuCauId, CancellationToken ct) => LoadAsync(yeuCauId, ct);

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
