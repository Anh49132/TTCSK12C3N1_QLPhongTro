using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using QL_PhongTro.Data;
using QL_PhongTro.Models;
using QL_PhongTro.Services;
using QL_PhongTro.ViewModels.Auth;

namespace QL_PhongTro.Controllers;

public class AccountController(AppDbContext db, RegistrationSettings settings, AuthService authService) : Controller
{
    private static bool ValidatePhone(string? phone) =>
        !string.IsNullOrWhiteSpace(phone) && Regex.IsMatch(phone, @"^0\d{9}$");

    private static bool ValidatePassword(string? password) =>
        !string.IsNullOrEmpty(password) && password.Length >= 8 &&
        Regex.IsMatch(password, "[A-Za-z]") && Regex.IsMatch(password, @"\d");

    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Email) && string.IsNullOrWhiteSpace(request?.SoDienThoai))
        {
            ModelState.AddModelError("", "Phải nhập email hoặc số điện thoại");
            return View(request);
        }

        var (success, response, error) = await authService.LoginAsync(request);

        if (!success)
        {
            ModelState.AddModelError("", error);
            return View(request);
        }

        return RedirectToAction("Index", "Home");
    }

    public IActionResult Register() => View();

    [HttpPost]
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
        await db.SaveChangesAsync();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.HoTen),
            new Claim(ClaimTypes.Role, user.VaiTro)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        TempData["RegisterSuccess"] = "Đăng ký thành công";
        return RedirectToAction("Index", "Home");
    }
}
