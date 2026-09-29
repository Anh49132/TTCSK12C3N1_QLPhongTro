using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using QL_PhongTro.Data;
using QL_PhongTro.Models;
using QL_PhongTro.ViewModels.Auth;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Services;

namespace QL_PhongTro.Controllers;

public partial class AccountController(AppDbContext db, RegistrationSettings settings, SessionVersionStore sessions, ICredentialValidationService credentialValidator, AuthService authService) : Controller
{
    private static bool ValidatePhone(string? phone) =>
        !string.IsNullOrWhiteSpace(phone) && Regex.IsMatch(phone, @"^0\d{9}$");

    private static bool ValidatePassword(string? password) =>
        !string.IsNullOrEmpty(password) && password.Length >= 8 &&
        Regex.IsMatch(password, "[A-Za-z]") && Regex.IsMatch(password, @"\d");

    [HttpGet]
    public IActionResult Login(string? returnUrl = null) => View(new LoginRequest { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        // S1-02: trinh duyet gui bang fetch de nhan duoc access token + refresh token
        // va luu xuong thiet bi. Van giu duong POST binh thuong de hoat dong khi
        // khong co JavaScript.
        var isAjax = string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest",
            StringComparison.OrdinalIgnoreCase);

        if (!ModelState.IsValid)
        {
            var invalid = ModelState.Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage).FirstOrDefault() ?? "Thông tin đăng nhập không hợp lệ.";
            if (isAjax) return Json(new { success = false, message = invalid, locked = false, remainingMinutes = 0 });
            return View(request);
        }

        // Validate credentials using shared service (MVC cookie auth flow)
        var validation = await credentialValidator.ValidateAsync(request);

        if (!validation.Success)
        {
            if (isAjax)
            {
                return Json(new
                {
                    success = false,
                    message = validation.Error ?? "Thông tin đăng nhập không hợp lệ.",
                    locked = validation.IsLocked,
                    remainingMinutes = validation.LockoutRemainingMinutes
                });
            }
            ModelState.AddModelError("", validation.Error ?? "Thông tin đăng nhập không hợp lệ.");
            return View(request);
        }

        var user = validation.User!;
        var sessionVersion = sessions.Capture(user.Id, user.MatKhau);
        if (sessionVersion is null)
        {
            const string changed = "Thông tin đăng nhập vừa thay đổi. Vui lòng đăng nhập lại.";
            if (isAjax) return Json(new { success = false, message = changed, locked = false, remainingMinutes = 0 });
            ModelState.AddModelError("", changed);
            return View(request);
        }

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.HoTen),
            new Claim(ClaimTypes.Role, user.VaiTro ?? ""),
            new Claim(SessionVersionStore.ClaimType, sessionVersion)
        }, CookieAuthenticationDefaults.AuthenticationScheme);
await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        // S1-03: Check MustChangePassword (from dev)
        if (user.MustChangePassword)
        {
            return isAjax
                ? Json(new { success = true, mustChangePassword = true, returnUrl = Url.Action("ChangePassword", "Account") })
                : (IActionResult)RedirectToAction(nameof(ChangePassword));
        }

        // S1-02: phát access token (30 phut) + refresh token (7 ngay) cho phia may.
        if (isAjax)
        {
            var (issued, tokens, error) = await authService.LoginAsync(request);
            if (!issued)
            {
                return Json(new { success = false, message = error ?? "Đăng nhập thất bại", locked = false, remainingMinutes = 0 });
            }
            return Json(new
            {
                success = true,
                mustChangePassword = false,
                accessToken = tokens!.AccessToken,
                refreshToken = tokens.RefreshToken,
                returnUrl = Url.IsLocalUrl(request.ReturnUrl) ? request.ReturnUrl : Url.Action("Index", "Home")!
            });
        }

        return Url.IsLocalUrl(request.ReturnUrl) ? LocalRedirect(request.ReturnUrl!) : RedirectToAction("Index", "Home");
    }

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
        // For proper dual auth logout: need to revoke tokens if user has them
        // Since we only have cookie here, we sign out the cookie
        // The refresh token revocation would require the refresh token which we don't have in cookie-only flow
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

        if (settings.EnableDuplicateCheck)
        {
            if (db.TaiKhoans.Any(user => user.Email == emailNorm))
                ModelState.AddModelError("Email", "Email đã được sử dụng");
            if (db.TaiKhoans.Any(user => user.SoDienThoai == phoneNorm))
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

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.HoTen),
            new Claim(ClaimTypes.Role, user.VaiTro),
            new Claim(SessionVersionStore.ClaimType, "0")
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        TempData["RegisterSuccess"] = "Đăng ký thành công";
        return RedirectToAction("Index", "Home");
    }
}