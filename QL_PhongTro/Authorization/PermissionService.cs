using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Data;
using QL_PhongTro.Models;

namespace QL_PhongTro.Authorization;

// Scoped to one request: menu, page and API read the same current database snapshot.
public sealed class PermissionService(AppDbContext db, IHttpContextAccessor accessor)
{
    private Task<List<RolePermission>>? permissions;
    public string? RoleCode => accessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
    public Task<List<RolePermission>> CurrentAsync() => permissions ??= LoadAsync();
    private async Task<List<RolePermission>> LoadAsync()
    {
        if (accessor.HttpContext?.User.Identity?.IsAuthenticated != true || RoleCode is null)
            return [];
        return await db.RolePermissions.AsNoTracking().Where(p => p.RoleCode == RoleCode).ToListAsync();
    }
    public async Task<bool> AllowsAsync(string module, bool write = false)
    {
        var permission = (await CurrentAsync()).SingleOrDefault(p => p.ModuleCode == module);
        return permission is not null && (write
            ? permission.AccessLevel is "WRITE" or "FULL"
            : permission.AccessLevel is "READ" or "WRITE" or "FULL");
    }
    public async Task<List<AppModule>> MenuAsync()
    {
        var allowed = (await CurrentAsync()).Where(p => p.AccessLevel is "READ" or "WRITE" or "FULL")
            // PHONG_TRO currently links to staff management, not a tenant room page.
            .Where(p => RoleCode != "KHACH_THUE" || p.ModuleCode != "PHONG_TRO")
            .Select(p => p.ModuleCode).ToList();
        return await db.AppModules.AsNoTracking().Where(m => allowed.Contains(m.Code))
            .OrderBy(m => m.SortOrder).ToListAsync();
    }
    public static string Url(string code) => code switch
    {
        "TAI_KHOAN" => "/Permissions",
        "PHONG_TRO" => "/PhongTro",
        _ => "/Modules/" + Uri.EscapeDataString(code)
    };

    public async Task<int> UnprocessedRequestCountAsync()
    {
        if (RoleCode != "CHU_NHA") return 0;
        if (!int.TryParse(accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var accountId)) return 0;
        return await db.YeuCaus.AsNoTracking()
            .Where(x => TrangThaiYeuCau.ChuaXuLy(x.TrangThai))
            .Join(db.ToaNhas.AsNoTracking().Where(x => x.ChuNhaId == accountId && x.DangHoatDong),
                request => request.ToaNhaId, building => building.Id, (_, _) => 1)
            .CountAsync();
    }
}

