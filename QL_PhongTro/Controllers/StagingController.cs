using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Data;
using QL_PhongTro.ViewModels;

namespace QL_PhongTro.Controllers;

[Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class StagingController(AppDbContext db, IWebHostEnvironment environment) : Controller
{
    [HttpGet("/Staging")]
    public async Task<IActionResult> Index()
    {
        if (!environment.IsStaging()) return NotFound();
        if (User.FindFirstValue(ClaimTypes.Role) != "ADMIN") return Forbid();

        var connection = db.Database.GetDbConnection();
        await db.Database.OpenConnectionAsync();
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT
                    (SELECT COUNT(*) FROM toa_nha),
                    (SELECT COUNT(*) FROM phong_tro),
                    (SELECT COUNT(*) FROM hop_dong),
                    (SELECT COUNT(*) FROM hoa_don),
                    (SELECT COUNT(*) FROM (SELECT nam, thang FROM hoa_don GROUP BY nam, thang)),
                    (SELECT COUNT(*) FROM hoa_don i WHERE (SELECT COALESCE(SUM(p.so_tien),0) FROM thanh_toan p WHERE p.hoa_don_id=i.id AND p.trang_thai='DA_XAC_NHAN') >= i.tong_tien),
                    (SELECT COUNT(*) FROM hoa_don i WHERE (SELECT COALESCE(SUM(p.so_tien),0) FROM thanh_toan p WHERE p.hoa_don_id=i.id AND p.trang_thai='DA_XAC_NHAN') > 0 AND (SELECT COALESCE(SUM(p.so_tien),0) FROM thanh_toan p WHERE p.hoa_don_id=i.id AND p.trang_thai='DA_XAC_NHAN') < i.tong_tien),
                    (SELECT COUNT(*) FROM hoa_don i WHERE (SELECT COALESCE(SUM(p.so_tien),0) FROM thanh_toan p WHERE p.hoa_don_id=i.id AND p.trang_thai='DA_XAC_NHAN') < i.tong_tien AND date(i.han_thanh_toan) < date('now'))
                """;
            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) throw new InvalidOperationException("Staging summary query returned no result.");
            return View(new StagingOverviewViewModel
            {
                BuildingCount = reader.GetInt32(0),
                RoomCount = reader.GetInt32(1),
                ContractCount = reader.GetInt32(2),
                InvoiceCount = reader.GetInt32(3),
                InvoicePeriodCount = reader.GetInt32(4),
                PaidInFullCount = reader.GetInt32(5),
                PaidPartiallyCount = reader.GetInt32(6),
                OverdueCount = reader.GetInt32(7)
            });
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }
}