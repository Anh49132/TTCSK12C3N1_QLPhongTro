using System.Globalization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Data;
using QL_PhongTro.Models;
using QL_PhongTro.ViewModels;

namespace QL_PhongTro.Services;

public sealed class RequestCodeExhaustedException() : Exception("Đã hết mã yêu cầu trong tháng. Vui lòng liên hệ quản lý.");
public sealed class DesiredDateException(string message) : Exception(message);

public class YeuCauThueService(AppDbContext db, ITimeProvider clock)
{
    public DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(
        DateTime.SpecifyKind(clock.UtcNow, DateTimeKind.Utc),
        TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh")));

    public string? ValidateDesiredDate(DateOnly? date)
    {
        var today = Today;
        if (date is null) return "Vui lòng nhập ngày mong muốn hợp lệ.";
        if (date < today) return "Ngày mong muốn không được là ngày trong quá khứ.";
        if (date > today.AddDays(60)) return "Ngày mong muốn không được quá 60 ngày kể từ hôm nay.";
        return null;
    }

    public IQueryable<TinDang> PublicListings()
    {
        var now = clock.UtcNow;
        return db.TinDangs.AsNoTracking().Where(t => t.TrangThai == "DANG_HIEN_THI"
            && t.NgayHetHan != null && t.NgayHetHan > now
            && db.PhongTros.Any(p => p.Id == t.PhongId && p.TrangThai == "TRONG"));
    }

    public async Task<bool> IsInstalled()
    {
        await db.Database.OpenConnectionAsync();
        using var cmd = db.Database.GetDbConnection().CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name IN ('tin_dang','yeu_cau_thue','rental_request_counter','rental_request_schema')";
        return Convert.ToInt32(await cmd.ExecuteScalarAsync()) == 4;
    }

    public async Task<YeuCauThue?> Send(int listingId, int accountId, GuiYeuCauViewModel form)
    {
        if (ValidateDesiredDate(form.NgayMongMuon) is { } dateError)
            throw new DesiredDateException(dateError);
        // Serialize counter allocation and profile creation across all web processes.
        await db.Database.OpenConnectionAsync();
        using var tx = ((SqliteConnection)db.Database.GetDbConnection()).BeginTransaction(deferred: false);
        await using var efTransaction = await db.Database.UseTransactionAsync(tx);
        var account = await db.TaiKhoans.AsNoTracking().SingleOrDefaultAsync(a => a.Id == accountId
            && a.VaiTro == "KHACH_THUE" && a.DangHoatDong && !a.IsDeleted && a.EmailConfirmed && !a.MustChangePassword);
        if (account is null) throw new UnauthorizedAccessException();
        if (!await PublicListings().AnyAsync(t => t.Id == listingId)) return null;
        var profile = await db.KhachThues.SingleOrDefaultAsync(k => k.TaiKhoanId == accountId);
        if (profile is null)
        {
            profile = new KhachThue { TaiKhoanId = accountId, HoTen = account.HoTen, NgayTao = clock.UtcNow };
            db.KhachThues.Add(profile);
            await db.SaveChangesAsync();
        }
        var vietnam = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        var month = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(clock.UtcNow, DateTimeKind.Utc), vietnam)
            .ToString("yyyyMM", CultureInfo.InvariantCulture);
        using var cmd = tx.Connection!.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = """
            INSERT INTO rental_request_counter(thang,so_cuoi) VALUES($month,1)
            ON CONFLICT(thang) DO UPDATE SET so_cuoi=so_cuoi+1 WHERE so_cuoi<9999
            RETURNING so_cuoi;
            """;
        cmd.Parameters.AddWithValue("$month", month);
        var number = await cmd.ExecuteScalarAsync();
        if (number is null) throw new RequestCodeExhaustedException();
        var request = new YeuCauThue
        {
            MaYeuCau = $"YC-{month}-{Convert.ToInt32(number):D4}", TinDangId = listingId,
            KhachThueId = profile.Id, LoaiYeuCau = form.LoaiYeuCau!, NgayMongMuon = form.NgayMongMuon!.Value,
            SoNguoiDuKien = form.SoNguoiDuKien!.Value, LoiNhan = form.LoiNhan?.Trim(), NgayTao = clock.UtcNow
        };
        db.YeuCauThues.Add(request);
        await db.SaveChangesAsync();
        tx.Commit();
        return request;
    }
}
