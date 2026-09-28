using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.ViewModels.Auth;

namespace QL_PhongTro.Controllers;

public partial class AccountController
{
    [HttpGet]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "KHACH_THUE")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "KHACH_THUE")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)) return Challenge();
        var account = await db.TaiKhoans.AsNoTracking().SingleOrDefaultAsync(a => a.Id == id && a.DangHoatDong && a.VaiTro == "KHACH_THUE");
        if (account is null) return Forbid();
        var valid = false;
        try { valid = BCrypt.Net.BCrypt.Verify(model.CurrentPassword, account.MatKhau); }
        catch (BCrypt.Net.SaltParseException) { }
        if (!valid)
            ModelState.AddModelError(nameof(model.CurrentPassword), "Mật khẩu hiện tại không đúng.");
        else if (BCrypt.Net.BCrypt.Verify(model.NewPassword, account.MatKhau))
            ModelState.AddModelError(nameof(model.NewPassword), "Mật khẩu mới phải khác mật khẩu hiện tại.");
        if (!ModelState.IsValid) return View(model);

        var hash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        var updated = await db.TaiKhoans.Where(a => a.Id == id && a.MatKhau == account.MatKhau && a.DangHoatDong && a.VaiTro == "KHACH_THUE")
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.MatKhau, hash).SetProperty(a => a.NgayCapNhat, DateTime.UtcNow));
        if (updated != 1)
        {
            ModelState.AddModelError("", "Tài khoản vừa thay đổi. Vui lòng thử lại.");
            return View(model);
        }
        TempData["PasswordSuccess"] = "Đổi mật khẩu thành công.";
        return RedirectToAction(nameof(ChangePassword));
    }
}
