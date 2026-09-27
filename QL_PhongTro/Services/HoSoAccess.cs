using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Data;
using QL_PhongTro.Models;

namespace QL_PhongTro.Services;

public class HoSoAccess(AppDbContext db)
{
    public static string Mask(string? value) => string.IsNullOrEmpty(value) ? string.Empty
        : value.Length <= 4 ? new string('*', value.Length)
        : new string('*', value.Length - 4) + value[^4..];

    public async Task<bool> CanReadFullAsync(TaiKhoan viewer, int profileId)
    {
        if (!viewer.DangHoatDong) return false;
        if (viewer.VaiTro == "ADMIN") return true;
        if (viewer.VaiTro != "CHU_NHA") return false;
        var connection = db.Database.GetDbConnection();
        var openedHere = connection.State != ConnectionState.Open;
        try
        {
            if (openedHere) await connection.OpenAsync();
            // A missing/incompatible rental schema must never grant access.
            using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT EXISTS (
                    SELECT 1 FROM hop_dong h
                    JOIN phong_tro p ON p.id = h.phong_id
                    JOIN toa_nha t ON t.id = p.toa_nha_id
                    WHERE t.chu_nha_id = @viewer AND h.trang_thai = 'DANG_HIEU_LUC'
                      AND (h.ngay_tra_phong IS NULL OR date(h.ngay_tra_phong) >= @today)
                      AND EXISTS (SELECT 1 FROM ky_hop_dong k WHERE k.hop_dong_id = h.id
                          AND date(k.ngay_bat_dau) <= @today AND date(k.ngay_ket_thuc) >= @today)
                      AND (h.khach_dung_ten_id = @profile OR EXISTS (
                          SELECT 1 FROM nguoi_o_ghep g WHERE g.hop_dong_id = h.id
                            AND g.khach_thue_id = @profile AND date(g.ngay_vao) <= @today
                            AND (g.ngay_chuyen_di IS NULL OR date(g.ngay_chuyen_di) > @today)))
                )
                """;
            foreach (var pair in new (string, object)[] { ("@viewer", viewer.Id), ("@profile", profileId),
                ("@today", DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)).ToString("yyyy-MM-dd")) })
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = pair.Item1;
                parameter.Value = pair.Item2;
                command.Parameters.Add(parameter);
            }
            return Convert.ToInt64(await command.ExecuteScalarAsync()) == 1;
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 1) { return false; }
        finally { if (openedHere) await connection.CloseAsync(); }
    }
}
