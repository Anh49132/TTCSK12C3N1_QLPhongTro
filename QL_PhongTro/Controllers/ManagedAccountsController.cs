using System.Security.Claims;
using System.Security.Cryptography;
using System.Net.Mail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QL_PhongTro.Data;
using QL_PhongTro.Models;
using QL_PhongTro.Services;
using QL_PhongTro.ViewModels;

namespace QL_PhongTro.Controllers;

[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "ADMIN")]
[AutoValidateAntiforgeryToken]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class ManagedAccountsController(AppDbContext db, ITemporaryPasswordEmailSender email,
    IOptions<PasswordResetOptions> options) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? role, string? status, int page = 1)
    {
        if (role is not (null or "" or "ADMIN" or "CHU_NHA" or "QUAN_LY" or "KHACH_THUE") ||
            status is not (null or "" or "active" or "locked")) return BadRequest("Bộ lọc không hợp lệ.");
        var query = db.TaiKhoans.AsNoTracking();
        if (!string.IsNullOrEmpty(role)) query = query.Where(a => a.VaiTro == role);
        if (!string.IsNullOrEmpty(status)) query = query.Where(a => a.DangHoatDong == (status == "active"));
        var total = await query.CountAsync();
        page = Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling(total / 20d)));
        return View(new ManagedAccountsViewModel
        {
            Role = role, Status = status, Total = total, Page = page,
            Accounts = await query.OrderByDescending(a => a.Id).Skip((page - 1) * 20).Take(20).ToListAsync()
        });
    }

    [HttpGet]
    public IActionResult Create() => View(new CreateManagedAccountViewModel());

    [HttpPost]
    public async Task<IActionResult> Create(CreateManagedAccountViewModel model)
    {
        model.Email = (model.Email ?? "").Trim().ToLowerInvariant();
        model.HoTen = (model.HoTen ?? "").Trim();
        if (string.IsNullOrWhiteSpace(model.HoTen)) ModelState.AddModelError(nameof(model.HoTen), "Nhập họ tên.");
        if (!ModelState.IsValid) return View(model);
        if (await db.TaiKhoans.AnyAsync(a => a.Email.Trim().ToLower() == model.Email))
            ModelState.AddModelError(nameof(model.Email), "Email đã được sử dụng.");
        if (await db.TaiKhoans.AnyAsync(a => a.SoDienThoai == model.SoDienThoai))
            ModelState.AddModelError(nameof(model.SoDienThoai), "Số điện thoại đã được sử dụng.");
        if (!ModelState.IsValid) return View(model);
        var password = TemporaryPassword();
        var now = DateTime.UtcNow;
        var account = new TaiKhoan
        {
            HoTen = model.HoTen, Email = model.Email, SoDienThoai = model.SoDienThoai,
            VaiTro = model.VaiTro, MatKhau = BCrypt.Net.BCrypt.HashPassword(password),
            DangHoatDong = true, MustChangePassword = true, NgayTao = now, NgayCapNhat = now
        };
        db.TaiKhoans.Add(account);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteErrorCode: 19 })
        {
            ModelState.AddModelError("", "Email hoặc số điện thoại đã được sử dụng. Vui lòng kiểm tra lại.");
            return View(model);
        }
        await SendAsync(account, password);
        return RedirectToAction(nameof(Index));
    }

    // Retry after delivery failure without storing a plaintext temporary password.
    [HttpPost]
    public async Task<IActionResult> Resend(int id)
    {
        await using var tx = await db.Database.BeginTransactionAsync();
        var account = await db.TaiKhoans.SingleOrDefaultAsync(a => a.Id == id && a.MustChangePassword && a.DangHoatDong);
        if (account is null || account.VaiTro is not ("CHU_NHA" or "QUAN_LY")) return NotFound();
        var password = TemporaryPassword();
        db.TemporaryPasswordResentFor = account.Id;
        account.MatKhau = BCrypt.Net.BCrypt.HashPassword(password);
        account.NgayCapNhat = DateTime.UtcNow;
        account.RefreshTokenHash = null; account.RefreshTokenExpiry = null;
        await db.SaveChangesAsync();
        await RevokeAsync(id);
        await tx.CommitAsync();
        await SendAsync(account, password);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> SetActive(int id, bool active, bool confirmed = false)
    {
        if (!active && !confirmed) return BadRequest("Cần xác nhận khoá tài khoản.");
        if (User.FindFirstValue(ClaimTypes.NameIdentifier) == id.ToString()) return BadRequest("Không được tự khoá tài khoản.");
        await using var tx = await db.Database.BeginTransactionAsync();
        var account = await db.TaiKhoans.SingleOrDefaultAsync(a => a.Id == id);
        if (account is null) return NotFound();
        if (account.VaiTro is not ("CHU_NHA" or "QUAN_LY")) return Forbid();
        account.DangHoatDong = active;
        account.RefreshTokenHash = null; account.RefreshTokenExpiry = null;
        account.FailedLoginCount = 0; account.LockedUntil = null;
        account.NgayCapNhat = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await RevokeAsync(id);
        await tx.CommitAsync();
        TempData["AccountMessage"] = active ? "Đã mở khoá. Người dùng cần đăng nhập lại." : "Đã khoá và thu hồi mọi phiên đăng nhập.";
        return RedirectToAction(nameof(Index));
    }

    private Task<int> RevokeAsync(int id)
    {
        var version = Guid.NewGuid().ToString("N");
        return db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO account_session_version(account_id,version) VALUES ({id},{version}) ON CONFLICT(account_id) DO UPDATE SET version=excluded.version");
    }

    private async Task SendAsync(TaiKhoan account, string password)
    {
        try
        {
            await email.SendTemporaryAsync(account.Email, account.HoTen, ManagedAccountsViewModel.RoleName(account.VaiTro), password);
            TempData["AccountMessage"] = string.IsNullOrWhiteSpace(options.Value.PickupDirectory)
                ? "Tài khoản sẵn sàng. Email mật khẩu tạm đã được chuyển đến máy chủ gửi thư."
                : "Tài khoản sẵn sàng. Email thử nghiệm đã lưu thành file .eml, chưa gửi đến hộp thư thật.";
        }
        catch (Exception ex) when (ex is SmtpException or InvalidOperationException or FormatException or IOException or UnauthorizedAccessException)
        {
            TempData["AccountError"] = "Tài khoản đã lưu nhưng chưa gửi được email. Kiểm tra cấu hình email, rồi chọn Gửi lại mật khẩu tạm. Mật khẩu tạm cũ sẽ bị thay thế.";
        }
    }

    private static string TemporaryPassword()
    {
        string[] groups = ["ABCDEFGHJKLMNPQRSTUVWXYZ", "abcdefghijkmnopqrstuvwxyz", "23456789", "!@#$%&*?"];
        var all = string.Concat(groups);
        var chars = new char[16];
        for (var i = 0; i < chars.Length; i++)
        {
            var source = i < groups.Length ? groups[i] : all;
            chars[i] = source[RandomNumberGenerator.GetInt32(source.Length)];
        }
        RandomNumberGenerator.Shuffle(chars.AsSpan());
        return new string(chars);
    }
}
