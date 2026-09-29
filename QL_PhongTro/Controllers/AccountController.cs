using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using QL_PhongTro.Data;
using QL_PhongTro.Models;

using QL_PhongTro.ViewModels;
using Microsoft.EntityFrameworkCore;

using QL_PhongTro.Services;
using QL_PhongTro.ViewModels.Auth;


namespace QL_PhongTro.Controllers;

public partial class AccountController(AppDbContext db, RegistrationSettings settings, SessionVersionStore sessions, IRegistrationEmailSender registrationEmail) : Controller
{
    private static bool ValidatePhone(string? phone) =>
        !string.IsNullOrWhiteSpace(phone) && Regex.IsMatch(phone, @"^0\d{9}$");

    private static bool ValidatePassword(string? password) =>
        !string.IsNullOrEmpty(password) && password.Length >= 8 &&
        Regex.IsMatch(password, "[A-Za-z]") && Regex.IsMatch(password, @"\d");

    [HttpGet]
    public IActionResult Login(string? returnUrl = null, bool timeout = false)
    {
        if (timeout) ViewData["TimeoutMessage"] = "Phiên đăng nhập đã hết hạn sau 30 phút. Vui lòng đăng nhập lại.";
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var identifier = model.Identifier.Trim().ToLowerInvariant();
        var account = await db.TaiKhoans.AsNoTracking().FirstOrDefaultAsync(a =>
            !a.IsDeleted && (a.Email.Trim().ToLower() == identifier || a.SoDienThoai == identifier));
        bool valid = false;
        try { valid = BCrypt.Net.BCrypt.Verify(model.Password, account?.MatKhau ?? DummyHash); }
        catch (BCrypt.Net.SaltParseException) { }
        if (!valid || account is null || !account.DangHoatDong || !account.EmailConfirmed)
        {
            ModelState.AddModelError("", "Thông tin đăng nhập không hợp lệ.");
            return View(model);
        }
        var sessionVersion = sessions.Capture(account.Id, account.MatKhau);
        if (sessionVersion is null)
        {
            ModelState.AddModelError("", "Thông tin đăng nhập vừa thay đổi. Vui lòng đăng nhập lại.");
            return View(model);
        }
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new Claim(ClaimTypes.Name, account.HoTen),
            new Claim(ClaimTypes.Role, account.VaiTro ?? ""),
            new Claim(SessionVersionStore.ClaimType, sessionVersion)
        }, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        if (account.MustChangePassword) return RedirectToAction(nameof(ChangePassword));
        return Url.IsLocalUrl(model.ReturnUrl) ? LocalRedirect(model.ReturnUrl!) : RedirectToAction("Index", "Home");
    }

    private static readonly string DummyHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString());

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult SessionStatus() => Json(new
    {
        authenticated = User.Identity?.IsAuthenticated == true,
        mustChangePassword = User.FindFirst("must_change_password")?.Value == "true"
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        Response.Headers.CacheControl = "no-store";
        return View();

    }

    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel? model)
    {
        if (model is null)
        {
            ModelState.AddModelError("", "Invalid request");
            return View(model);
        }

        if (string.IsNullOrWhiteSpace(model.HoTen))
            ModelState.AddModelError("HoTen", "Họ tên là bắt buộc");
        if (string.IsNullOrWhiteSpace(model.Email))
            ModelState.AddModelError("Email", "Email là bắt buộc");
        if (!ValidatePhone(model.SoDienThoai))
            ModelState.AddModelError("SoDienThoai", "Số điện thoại phải bắt đầu bằng 0 và đúng 10 chữ số");
        if (!ValidatePassword(model.MatKhau))
            ModelState.AddModelError("MatKhau", "Mật khẩu tối thiểu 8 ký tự, ít nhất một chữ cái và một chữ số");

        if (!ModelState.IsValid)
            return View(model);

        var emailNorm = model.Email!.ToLower().Trim();
        var phoneNorm = model.SoDienThoai!;

        // A retry after registration/email delivery must resume confirmation, not
        // create another account or replace the password of the pending account.
        var pendingAccount = await db.TaiKhoans.AsNoTracking().AnyAsync(a =>
            !a.IsDeleted && a.DangHoatDong && !a.EmailConfirmed && a.VaiTro == "KHACH_THUE" &&
            a.Email.Trim().ToLower() == emailNorm && a.SoDienThoai == phoneNorm);
        if (pendingAccount)
        {
            TempData["RegisterEmail"] = emailNorm;
            TempData["RegisterMessage"] = "Tài khoản đã được tạo và đang chờ xác nhận email. Nhập mã đã nhận hoặc chọn Gửi lại mã. Mật khẩu vẫn là mật khẩu của lần đăng ký trước.";
            return RedirectToAction(nameof(ConfirmEmail));
        }

        if (settings.EnableDuplicateCheck)
        {
            if (db.TaiKhoans.Any(user => !user.IsDeleted && user.Email.Trim().ToLower() == emailNorm))
                ModelState.AddModelError("Email", "Email đã được sử dụng");
            if (db.TaiKhoans.Any(user => !user.IsDeleted && user.SoDienThoai == phoneNorm))
                ModelState.AddModelError("SoDienThoai", "Số điện thoại đã được sử dụng");
        }

        if (!ModelState.IsValid)
            return View(model);

        var now = DateTime.UtcNow;
        var user = new TaiKhoan
        {
            HoTen = model.HoTen!,
            Email = emailNorm,
            SoDienThoai = phoneNorm,
            MatKhau = BCrypt.Net.BCrypt.HashPassword(model.MatKhau!),
            VaiTro = "KHACH_THUE",
            DangHoatDong = true,
            IsStaff = false,
            IsSuperuser = false,
            EmailConfirmed = false,
            LastLogin = null,
            NgayTao = now,
            NgayCapNhat = now
        };

        db.TaiKhoans.Add(user);
        db.SelfRegisteringAccount = user;
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.Sqlite.SqliteException { SqliteErrorCode: 19 })
        {
            ModelState.AddModelError("", "Email hoặc số điện thoại đã được sử dụng.");
            return View(model);
        }

        var code = Random.Shared.Next(100000, 1000000).ToString();
        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(code)));
        await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO email_confirmation(account_id,token_hash,expires_at,requested_at) VALUES ({user.Id},{hash},{DateTime.UtcNow.AddMinutes(15).Ticks},{DateTime.UtcNow.Ticks})");
        await SendConfirmationNoticeAsync(user, code);
        TempData["RegisterEmail"] = user.Email;
        return RedirectToAction(nameof(ConfirmEmail));
    }

    [HttpGet]
    public IActionResult ConfirmEmail(string? email) => View(new EmailConfirmationViewModel { Email = email ?? TempData["RegisterEmail"] as string ?? "" });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmEmail(EmailConfirmationViewModel model)
    {
        await using var tx = await db.Database.BeginTransactionAsync();
        var email = model.Email.Trim().ToLowerInvariant();
        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(model.Code?.Trim() ?? "")));
        var account = await db.TaiKhoans.SingleOrDefaultAsync(a => !a.IsDeleted && a.Email.Trim().ToLower() == email && !a.EmailConfirmed && a.DangHoatDong);
        if (account is null) { ModelState.AddModelError("", "Thông tin xác nhận không hợp lệ."); return View(model); }
        var valid = await db.Database.SqlQueryRaw<long>("SELECT COUNT(*) AS Value FROM email_confirmation WHERE account_id = {0} AND token_hash = {1} AND expires_at > {2}", account.Id, hash, DateTime.UtcNow.Ticks).SingleAsync() == 1;
        if (!valid)
        {
            if (await db.Database.SqlQueryRaw<long>("SELECT COUNT(*) AS Value FROM email_confirmation WHERE account_id = {0} AND expires_at <= {1}", account.Id, DateTime.UtcNow.Ticks).SingleAsync() > 0)
            {
                await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE tai_khoan SET dang_hoat_dong=0, is_deleted=1 WHERE id={account.Id} AND email_confirmed=0 AND is_deleted=0");
                await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM email_confirmation WHERE account_id={account.Id}");
                await tx.CommitAsync();
                ModelState.AddModelError("", "Mã đã hết hạn. Tài khoản chờ xác nhận đã hủy; vui lòng đăng ký lại.");
            }
            else ModelState.AddModelError("Code", "Mã không đúng hoặc đã hết hạn.");
            return View(model);
        }
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE tai_khoan SET email_confirmed = 1 WHERE id = {account.Id} AND is_deleted=0 AND dang_hoat_dong=1");
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM email_confirmation WHERE account_id={account.Id}");
        await tx.CommitAsync();
        TempData["PasswordSuccess"] = "Xác nhận thành công. Hãy đăng nhập.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendConfirmation(string email)
    {
        TempData.Remove("RegisterMessage");
        TempData.Remove("RegisterError");
        TempData["RegisterEmail"] = email;
        var account = await db.TaiKhoans.SingleOrDefaultAsync(a => !a.IsDeleted && a.Email.Trim().ToLower() == email.Trim().ToLowerInvariant() && !a.EmailConfirmed && a.DangHoatDong);
        if (account is not null)
        {
            var code = Random.Shared.Next(100000, 1000000).ToString();
            var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(code)));
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM email_confirmation WHERE account_id={account.Id}");
            await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO email_confirmation(account_id,token_hash,expires_at,requested_at) VALUES ({account.Id},{hash},{DateTime.UtcNow.AddMinutes(15).Ticks},{DateTime.UtcNow.Ticks})");
            await SendConfirmationNoticeAsync(account, code);
        }
        else TempData["RegisterMessage"] = "Yêu cầu đã được xử lý. Chỉ tài khoản đang chờ xác nhận mới được cấp mã. Nếu đã xác nhận, hãy đăng nhập.";
        return RedirectToAction(nameof(ConfirmEmail));
    }

    private async Task SendConfirmationNoticeAsync(TaiKhoan account, string code)
    {
        TempData.Remove("RegisterMessage");
        TempData.Remove("RegisterError");
        try
        {
            var mode = await registrationEmail.SendConfirmationAsync(account.Email, account.HoTen, code);
            TempData["RegisterMessage"] = mode == EmailDeliveryMode.Pickup
                ? "Chế độ thử nghiệm: mã chỉ được lưu vào file email trên máy chạy ứng dụng, chưa gửi đến hộp thư. Cần cấu hình SMTP để nhận email thật."
                : "Máy chủ gửi thư đã nhận email chứa mã. Kiểm tra Hộp thư đến và Spam; trạng thái này chưa xác nhận thư đã tới hộp thư.";
        }
        catch (Exception ex) when (ex is System.Net.Mail.SmtpException or InvalidOperationException or FormatException or IOException or UnauthorizedAccessException or OperationCanceledException)
        {
            TempData["RegisterError"] = ex switch
            {
                OperationCanceledException => "Gửi mã quá thời gian chờ 15 giây. Kiểm tra kết nối SMTP rồi thử lại.",
                System.Net.Mail.SmtpException => "Không gửi được mã qua SMTP. Kiểm tra máy chủ, tài khoản gửi và mật khẩu ứng dụng Gmail rồi thử lại.",
                _ => "Không gửi được mã: cấu hình gửi email hoặc thư mục lưu thư chưa hợp lệ. Tài khoản vẫn đang chờ xác nhận."
            };
        }
    }
}
