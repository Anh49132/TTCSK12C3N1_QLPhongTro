using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QL_PhongTro.Services;

namespace QL_PhongTro.Controllers;

[Authorize(Roles = "KHACH_THUE")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class YeuCauThueController(YeuCauThueService service) : Controller
{
    private int? TaiKhoanId =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (TaiKhoanId is not int taiKhoanId) return Forbid();
        return View(await service.DanhSachCuaKhachAsync(taiKhoanId));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        if (TaiKhoanId is not int taiKhoanId) return Forbid();
        var result = await service.HuyCuaKhachAsync(taiKhoanId, id);
        switch (result)
        {
            case KetQuaHuyYeuCauThue.DaHuy:
                TempData["Success"] = "Đã huỷ yêu cầu.";
                break;
            case KetQuaHuyYeuCauThue.KhongTimThay:
                return NotFound();
            case KetQuaHuyYeuCauThue.KhongTheHuy:
                TempData["Error"] = "Yêu cầu đã ở trạng thái không thể huỷ.";
                break;
            case KetQuaHuyYeuCauThue.DaThayDoi:
                TempData["Error"] = "Yêu cầu vừa thay đổi; tải lại danh sách để kiểm tra trạng thái mới.";
                break;
        }
        return RedirectToAction(nameof(Index));
    }
}
