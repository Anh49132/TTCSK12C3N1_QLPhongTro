using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Data;
using QL_PhongTro.Models;
using QL_PhongTro.ViewModels.Auth;

namespace QL_PhongTro.Services;

public sealed class CredentialValidationResult
{
    public bool Success { get; init; }
    public TaiKhoan? User { get; init; }
    public string? Error { get; init; }
    public bool IsLocked { get; init; }
    public int LockoutRemainingMinutes { get; init; }
}

public interface ICredentialValidationService
{
    Task<CredentialValidationResult> ValidateAsync(LoginRequest request);
    Task<(bool blocked, int remainingMinutes)> CheckLockoutAsync(string taiKhoanDangNhap);
}

public class CredentialValidationService(AppDbContext db, ITimeProvider timeProvider) : ICredentialValidationService
{
    private readonly AppDbContext _db = db;
    private readonly ITimeProvider _timeProvider = timeProvider;

    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 15;

    // S1-02: thong bao loi khong duoc tiet lo tai khoan co ton tai hay khong, ke ca qua
    // thoi gian phan hoi. Khi khong tim thay tai khoan van phai chay BCrypt voi mot hash
    // gia de thoi gian tra ve ngang bang truong hop tai khoan co that.
    private static readonly string DummyHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString());

    private static bool BurnPasswordTiming(string? password)
    {
        try { BCrypt.Net.BCrypt.Verify(password ?? string.Empty, DummyHash); }
        catch (BCrypt.Net.SaltParseException) { }
        return false;
    }

    public async Task<CredentialValidationResult> ValidateAsync(LoginRequest request)
    {
        var input = request.TaiKhoanDangNhap?.Trim() ?? "";
        if (string.IsNullOrEmpty(input))
        {
            return new CredentialValidationResult { Success = false, Error = "Phải nhập email hoặc số điện thoại" };
        }

        if (string.IsNullOrWhiteSpace(request.MatKhau) || request.MatKhau.Length < 8)
        {
            return new CredentialValidationResult { Success = false, Error = "Mật khẩu tối thiểu 8 ký tự" };
        }

        bool isEmail = input.Contains("@");
        var user = await _db.TaiKhoans
            .FirstOrDefaultAsync(u => !u.IsDeleted && (isEmail ? u.Email == input.ToLower() : u.SoDienThoai == input));

        // Tai khoan da xoa hoa chua xac nhan email deu tra cung thong bao chung
        // "khong chinh xac" de khong lo tai khoan co that ra.
        if (user is null || !user.DangHoatDong || !user.EmailConfirmed)
        {
            BurnPasswordTiming(request.MatKhau);
            return new CredentialValidationResult { Success = false, Error = "Thông tin đăng nhập không chính xác" };
        }

        var now = _timeProvider.UtcNow;
        if (user.LockedUntil is not null && user.LockedUntil > now)
        {
            var remaining = (int)Math.Ceiling((user.LockedUntil.Value - now).TotalMinutes);
            return new CredentialValidationResult
            {
                Success = false,
                Error = $"Tài khoản đã khoá. Vui lòng thử lại sau {remaining} phút",
                IsLocked = true,
                LockoutRemainingMinutes = remaining
            };
        }

        if (!BCrypt.Net.BCrypt.Verify(request.MatKhau ?? string.Empty, user.MatKhau))
        {
            await HandleFailedLogin(user, now);
            return new CredentialValidationResult { Success = false, Error = "Thông tin đăng nhập không chính xác" };
        }

        await HandleSuccessfulLogin(user, now);
        await _db.SaveChangesAsync();

        return new CredentialValidationResult { Success = true, User = user };
    }

    public async Task<(bool blocked, int remainingMinutes)> CheckLockoutAsync(string taiKhoanDangNhap)
    {
        var input = taiKhoanDangNhap?.Trim() ?? "";
        if (string.IsNullOrEmpty(input)) return (false, 0);

        bool isEmail = input.Contains("@");
        var user = await _db.TaiKhoans
            .FirstOrDefaultAsync(u => !u.IsDeleted && (isEmail ? u.Email == input.ToLower() : u.SoDienThoai == input));

        if (user is null || !user.DangHoatDong || !user.EmailConfirmed) return (false, 0);

        var now = _timeProvider.UtcNow;
        if (user.LockedUntil is not null && user.LockedUntil > now)
        {
            return (true, (int)Math.Ceiling((user.LockedUntil.Value - now).TotalMinutes));
        }

        if (user.FailedLoginCount >= MaxFailedAttempts)
        {
            user.LockedUntil = now.AddMinutes(LockoutMinutes);
            await _db.SaveChangesAsync();
            return (true, LockoutMinutes);
        }

        return (false, 0);
    }

    private async Task HandleFailedLogin(TaiKhoan user, DateTime now)
    {
        user.FailedLoginCount = (user.FailedLoginCount ?? 0) + 1;
        user.NgayCapNhat = now;

        if (user.FailedLoginCount >= MaxFailedAttempts)
        {
            user.LockedUntil = now.AddMinutes(LockoutMinutes);
        }

        await _db.SaveChangesAsync();
    }

    private async Task HandleSuccessfulLogin(TaiKhoan user, DateTime now)
    {
        user.FailedLoginCount = 0;
        user.LockedUntil = null;
        user.NgayCapNhat = now;
        await _db.SaveChangesAsync();
    }
}