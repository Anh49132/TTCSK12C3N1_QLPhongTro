using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Data;
using QL_PhongTro.Models;
using QL_PhongTro.ViewModels.Auth;

namespace QL_PhongTro.Services;

public class AuthService(AppDbContext db, TokenService tokenService, JwtSettings jwtSettings)
{
    private readonly AppDbContext _db = db;
    private readonly TokenService _tokenService = tokenService;
    private readonly JwtSettings _jwtSettings = jwtSettings;

    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 15;

    public async Task<(bool success, LoginResponse? response, string? error)> LoginAsync(LoginRequest request)
    {
        var emailNorm = request.Email?.Trim().ToLower();
        var phoneNorm = request.SoDienThoai?.Trim();

        var user = await _db.TaiKhoans
            .FirstOrDefaultAsync(u => u.Email == emailNorm || u.SoDienThoai == phoneNorm);

        if (user is null)
        {
            await Task.Delay(1000);
            return (false, null, "Sai email/SĐT hoặc mật khẩu");
        }

        if (user.LockedUntil is not null && user.LockedUntil > DateTime.UtcNow)
        {
            var remaining = (int)Math.Ceiling((user.LockedUntil.Value - DateTime.UtcNow).TotalMinutes);
            return (false, null, $"Tài khoản đã khoá. Vui lòng thử lại sau {remaining} phút");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.MatKhau ?? string.Empty, user.MatKhau))
        {
            await HandleFailedLogin(user);
            return (false, null, "Sai email/SĐT hoặc mật khẩu");
        }

        await HandleSuccessfulLogin(user);

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenHash = ComputeHash(refreshToken);

        user.RefreshTokenHash = refreshTokenHash;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays);
        user.LastLogin = DateTime.UtcNow;
        user.NgayCapNhat = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return (true, new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresIn = _jwtSettings.AccessTokenMinutes * 60,
            RefreshTokenExpiresIn = _jwtSettings.RefreshTokenDays * 24 * 60 * 60,
            UserId = user.Id.ToString(),
            HoTen = user.HoTen,
            VaiTro = user.VaiTro ?? "KHACH_THUE"
        }, null);
    }

    public async Task<(bool success, RefreshTokenResponse? response, string? error)> RefreshTokenAsync(string refreshToken)
    {
        var tokenHash = ComputeHash(refreshToken);
        var user = await _db.TaiKhoans
            .FirstOrDefaultAsync(u => u.RefreshTokenHash == tokenHash && u.RefreshTokenExpiry > DateTime.UtcNow);

        if (user is null)
        {
            return (false, null, "Refresh token không hợp lệ hoặc đã hết hạn");
        }

        if (user.LockedUntil is not null && user.LockedUntil > DateTime.UtcNow)
        {
            var remaining = (int)Math.Ceiling((user.LockedUntil.Value - DateTime.UtcNow).TotalMinutes);
            return (false, null, $"Tài khoản đã khoá. Vui lòng thử lại sau {remaining} phút");
        }

        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        user.RefreshTokenHash = ComputeHash(newRefreshToken);
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays);
        user.NgayCapNhat = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return (true, new RefreshTokenResponse
        {
            AccessToken = newAccessToken,
            AccessTokenExpiresIn = _jwtSettings.AccessTokenMinutes * 60
        }, null);
    }

    public async Task<(bool success, string message)> LogoutAsync(string refreshToken)
    {
        var tokenHash = ComputeHash(refreshToken);
        var user = await _db.TaiKhoans
            .FirstOrDefaultAsync(u => u.RefreshTokenHash == tokenHash);

        if (user is not null)
        {
            user.RefreshTokenHash = null;
            user.RefreshTokenExpiry = null;
            await _db.SaveChangesAsync();
        }

        return (true, "Đăng xuất thành công");
    }

    public async Task<(bool blocked, int remainingMinutes)> CheckLockoutAsync(string emailOrPhone)
    {
        var emailNorm = emailOrPhone.Trim().ToLower();
        var phoneNorm = emailOrPhone.Trim();

        var user = await _db.TaiKhoans
            .FirstOrDefaultAsync(u => u.Email == emailNorm || u.SoDienThoai == phoneNorm);

        if (user is null) return (false, 0);

        if (user.LockedUntil is not null && user.LockedUntil > DateTime.UtcNow)
        {
            return (true, (int)Math.Ceiling((user.LockedUntil.Value - DateTime.UtcNow).TotalMinutes));
        }

        if (user.FailedLoginCount >= MaxFailedAttempts)
        {
            user.LockedUntil = DateTime.UtcNow.AddMinutes(LockoutMinutes);
            await _db.SaveChangesAsync();
            return (true, LockoutMinutes);
        }

        return (false, 0);
    }

    private async Task HandleFailedLogin(TaiKhoan user)
    {
        var now = DateTime.UtcNow;
        var windowStart = now.AddMinutes(-15);

        user.FailedLoginCount = (user.FailedLoginCount ?? 0) + 1;
        user.NgayCapNhat = now;

        if (user.FailedLoginCount >= MaxFailedAttempts)
        {
            user.LockedUntil = now.AddMinutes(LockoutMinutes);
        }

        await _db.SaveChangesAsync();
    }

    private async Task HandleSuccessfulLogin(TaiKhoan user)
    {
        user.FailedLoginCount = 0;
        user.LockedUntil = null;
        await _db.SaveChangesAsync();
    }

    private static string ComputeHash(string input)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}
