using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Authorization;
using QL_PhongTro.Data;

namespace QL_PhongTro.Controllers;

// Catalog only. No unimplemented business data or placeholder CRUD endpoints.
[ModuleAccess]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class ModulesController(AppDbContext db) : Controller
{
    [HttpGet("/Modules/{code}")]
    public async Task<IActionResult> Index(string code)
    {
        var module = await db.AppModules.AsNoTracking().SingleOrDefaultAsync(m => m.Code == code);
        if (module is null) return NotFound();
        if (code is "PHONG_TRO" or "TAI_KHOAN") return Redirect(PermissionService.Url(code));
        return View(module);
    }

    [HttpGet("/api/modules/{code}")]
    public async Task<IActionResult> Details(string code)
    {
        var module = await db.AppModules.AsNoTracking().SingleOrDefaultAsync(m => m.Code == code);
        return module is null ? NotFound() : Ok(new { module.Code, module.Name });
    }
}

