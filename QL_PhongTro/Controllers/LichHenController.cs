using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QL_PhongTro.Authorization;
using QL_PhongTro.Models;
using QL_PhongTro.Services;
using QL_PhongTro.ViewModels;

namespace QL_PhongTro.Controllers;

/// <summary>
/// S2-08 exposes one detail page, reached directly by URL. It deliberately has no Index
/// action: the landlord request list is S2-07 and the tenant list is S2-09, so a list here
/// would collide with both. The routes are written out in full for the same reason.
/// </summary>
[Authorize]
[ModuleAccess("YEU_CAU_THUE")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class LichHenController(LichHenService service) : Controller
{
    private int AccountId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    /// <summary>The time the user types is Vietnam time; the database keeps UTC.</summary>
    private static DateTime? ChuyenNhapSangUtc(DateTime? nhap) => nhap.HasValue ? nhap.Value.AddHours(-7) : null;

    private static DateTime? ChuyenUtcSangNhap(DateTime? utc) => utc.HasValue ? utc.Value.AddHours(7) : null;

    [HttpGet("/LichHen/ChiTiet/{id:int}")]
    public async Task<IActionResult> ChiTiet(int id, CancellationToken ct)
    {
        if (await service.DocAsync(id, ct) is not { } yeuCau) return NotFound();
        var laChuNha = await service.ChuNhaCuaYeuCauAsync(AccountId, id, ct);
        if (!laChuNha && !await service.KhachChuYeuCauAsync(AccountId, id, ct)) return Forbid();

        var view = new LichHenChiTietViewModel
        {
            YeuCau = yeuCau,
            LaChuNha = laChuNha,
            HienThiFormXacNhan = laChuNha && yeuCau.TrangThai == LichHenTrangThai.Moi,
            LichHenNhap = ChuyenUtcSangNhap(yeuCau.LichHen)
        };
        return View(view);
    }

    /// <summary>
    /// Advisory clash lookup behind the warning on the confirm form. It is deliberately a
    /// separate route rather than a filter on ChiTiet: the warning has to be re-checked every
    /// time the landlord moves the picker, and a failing lookup must never block confirming.
    /// </summary>
    [HttpGet("/LichHen/LichTrung")]
    public async Task<IActionResult> LichTrung(int id, DateTime? lichHen, CancellationToken ct)
    {
        if (await service.DocAsync(id, ct) is not { } yeuCau) return NotFound();
        if (!await service.ChuNhaCuaYeuCauAsync(AccountId, id, ct)) return Forbid();
        if (lichHen is not { } nhap) return Json(Array.Empty<LichHenService.LichTrung>());
        var list = await service.TimLichTrungAsync(yeuCau.PhongId, ChuyenNhapSangUtc(nhap)!.Value, id, ct);
        return Json(list.Select(x => new
        {
            x.YeuCauId, x.MaYeuCau, x.TenKhach, x.TenPhong,
            LichHenHienThoi = LichHenService.HienThoiGio(x.LichHen)
        }));
    }

    /// <summary>Parameter names match the input on the detail form so model binding works.</summary>
    [HttpPost("/LichHen/XacNhanLich"), ValidateAntiForgeryToken, ModuleAccess("YEU_CAU_THUE", write: true)]
    public async Task<IActionResult> XacNhanLich(int id, DateTime? LichHenNhap, CancellationToken ct)
    {
        try
        {
            if (LichHenNhap is not { } nhap) throw new InvalidOperationException("Hãy chọn ngày giờ hẹn.");
            await service.XacNhanLichAsync(id, AccountId, ChuyenNhapSangUtc(nhap)!.Value, ct);
            TempData["LichHenOk"] = "Đã xác nhận lịch hẹn.";
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception ex) when (ex is InvalidOperationException or LichHenConflictException)
        {
            TempData["LichHenError"] = ex.Message;
        }
        return RedirectToAction(nameof(ChiTiet), new { id });
    }
}
