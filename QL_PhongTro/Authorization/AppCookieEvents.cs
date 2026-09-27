using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Data;

namespace QL_PhongTro.Authorization;

public sealed class AppCookieEvents(AppDbContext db) : CookieAuthenticationEvents
{
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        if (!int.TryParse(context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
        {
            context.RejectPrincipal();
            return;
        }
        var account = await db.TaiKhoans.AsNoTracking().SingleOrDefaultAsync(a => a.Id == id);
        if (account is null || !account.DangHoatDong)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync();
            return;
        }
        // Never rely on a stale role in a previously issued cookie.
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new Claim(ClaimTypes.Name, account.HoTen),
            new Claim(ClaimTypes.Role, account.VaiTro ?? "")
        }, CookieAuthenticationDefaults.AuthenticationScheme);
        context.ReplacePrincipal(new ClaimsPrincipal(identity));
    }
    public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
        => Reply(context, 401, "UNAUTHENTICATED", "Phiên đăng nhập không hợp lệ hoặc đã hết hạn.");
    public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
        => Reply(context, 403, "FORBIDDEN", "Bạn không có quyền truy cập chức năng này.");

    private static Task Reply(RedirectContext<CookieAuthenticationOptions> context, int status, string code, string message)
    {
        context.Response.Headers.CacheControl = "no-store";
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = status;
            return context.Response.WriteAsJsonAsync(new { code, message });
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    }
}

