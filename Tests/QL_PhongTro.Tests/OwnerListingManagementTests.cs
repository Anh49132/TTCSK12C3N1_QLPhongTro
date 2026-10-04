using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Data;
using QL_PhongTro.Services;
using QL_PhongTro.ViewModels;
using Xunit;

namespace QL_PhongTro.Tests;

public sealed partial class PermissionTests
{
    private static string AntiForgery(string html) => WebUtility.HtmlDecode(
        Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);

    [Fact]
    public async Task ChuNha_DangLaiVaGoTinCuaPhongMinh()
    {
        var listingId = CreatePublicListing("TRONG", "TAM_AN", includePhoto: false);
        var (roomId, _) = GetListingRoomAndBuilding(listingId);
        using var owner = await Login("CHU_NHA");

        var manage = WebUtility.HtmlDecode(await owner.GetStringAsync("/TinDang/QuanLy"));
        Assert.Contains("Quản lý tin đăng", manage);
        Assert.Contains("Đăng lại", manage);

        var createHtml = await owner.GetStringAsync($"/TinDang/Tao?phongId={roomId}");
        var publish = await owner.PostAsync("/TinDang/Tao", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = AntiForgery(createHtml),
            ["PhongId"] = roomId.ToString(),
            ["TieuDe"] = "Tin chủ nhà vừa đăng lại",
            ["NoiDung"] = "Mô tả mới"
        }));
        Assert.Equal(HttpStatusCode.Redirect, publish.StatusCode);
        Assert.Equal("DANG_HIEN_THI", Scalar("SELECT trang_thai FROM tin_dang WHERE id=$id", ("$id", listingId)));
        Assert.Equal("Tin chủ nhà vừa đăng lại", Scalar("SELECT tieu_de FROM tin_dang WHERE id=$id", ("$id", listingId)));

        manage = WebUtility.HtmlDecode(await owner.GetStringAsync("/TinDang/QuanLy"));
        var remove = await owner.PostAsync($"/TinDang/Go/{listingId}", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = AntiForgery(manage)
        }));
        Assert.Equal(HttpStatusCode.Redirect, remove.StatusCode);
        Assert.Equal("/TinDang/QuanLy", remove.Headers.Location?.OriginalString);
        Assert.Equal("TAM_AN", Scalar("SELECT trang_thai FROM tin_dang WHERE id=$id", ("$id", listingId)));

        using var guest = Client();
        Assert.Equal(HttpStatusCode.NotFound, (await guest.GetAsync($"/TinDang/ChiTiet/{listingId}")).StatusCode);
    }

    [Fact]
    public async Task ChuNha_KhongGoDuocTinKhongThuocToaNhaMinh()
    {
        var listingId = CreatePublicListing("TRONG", "DANG_HIEN_THI", includePhoto: false);
        Execute("UPDATE toa_nha SET chu_nha_id=$other WHERE id=(SELECT p.toa_nha_id FROM phong_tro p JOIN tin_dang t ON t.phong_id=p.id WHERE t.id=$id)",
            ("$other", accounts["ADMIN"]), ("$id", listingId));
        using var owner = await Login("CHU_NHA");
        var html = await owner.GetStringAsync("/TinDang/QuanLy");

        var response = await owner.PostAsync($"/TinDang/Go/{listingId}", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = AntiForgery(html)
        }));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("DANG_HIEN_THI", Scalar("SELECT trang_thai FROM tin_dang WHERE id=$id", ("$id", listingId)));
    }

    [Fact]
    public async Task YeuCauDaHuy_KhongChanKhachGuiYeuCauMoi()
    {
        var listingId = CreatePublicListing("TRONG", "DANG_HIEN_THI", includePhoto: false);
        RentalRequestSchema.Initialize(database);
        Execute("UPDATE tai_khoan SET email_confirmed=1,must_change_password=0 WHERE id=$id", ("$id", accounts["KHACH_THUE"]));
        Execute("INSERT INTO khach_thue(tai_khoan_id,ho_ten,ngay_tao) VALUES($account,'Khách test',$now)",
            ("$account", accounts["KHACH_THUE"]), ("$now", DateTime.UtcNow.ToString("O")));
        var tenantId = Convert.ToInt32(Scalar("SELECT id FROM khach_thue WHERE tai_khoan_id=$account", ("$account", accounts["KHACH_THUE"])));
        Execute("""
            INSERT INTO yeu_cau_thue(ma_yeu_cau,tin_dang_id,khach_thue_id,loai_yeu_cau,ngay_mong_muon,so_nguoi_du_kien,trang_thai,ngay_tao)
            VALUES('YC-CANCELLED',$listing,$tenant,'XEM_PHONG',$date,1,'DA_HUY',$now)
            """, ("$listing", listingId), ("$tenant", tenantId), ("$date", DateOnly.FromDateTime(DateTime.Today.AddDays(2)).ToString("yyyy-MM-dd")), ("$now", DateTime.UtcNow.ToString("O")));

        var http = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, accounts["KHACH_THUE"].ToString())], "test"))
        };
        await using var context = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>().UseSqlite("Data Source=" + database).Options,
            new HttpContextAccessor { HttpContext = http });
        var service = new YeuCauThueService(context, new SystemTimeProvider());
        Assert.Null(await service.FindOpenRequest(listingId, accounts["KHACH_THUE"]));

        var created = await service.Send(listingId, accounts["KHACH_THUE"], new GuiYeuCauViewModel
        {
            LoaiYeuCau = "XEM_PHONG",
            NgayMongMuon = service.Today.AddDays(2),
            SoNguoiDuKien = 1,
            LoiNhan = "Yêu cầu mới sau khi đã hủy"
        });

        Assert.NotNull(created);
        Assert.Equal(2L, Scalar("SELECT COUNT(*) FROM yeu_cau_thue WHERE tin_dang_id=$listing AND khach_thue_id=$tenant", ("$listing", listingId), ("$tenant", tenantId)));
    }
}
