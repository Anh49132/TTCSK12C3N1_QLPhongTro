using System.Net;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Data;
using QL_PhongTro.Models;
using QL_PhongTro.Services;
using Xunit;

namespace QL_PhongTro.Tests;

public sealed partial class RoomServicesTests
{
    private async Task<int> SupplementalAsync(AppDbContext db, int room, string name, long price, string type = CachTinhDichVu.CoDinh)
    {
        var catalog = await AddService(db, name, price, type: type);
        await Rooms(db).DatDichVuAsync(1, room, catalog, true);
        await db.Database.ExecuteSqlRawAsync("UPDATE cau_hinh_dich_vu SET tu_ngay='2026-01-01'");
        return (await db.DichVuToaNhas.FindAsync(catalog))!.DichVuId;
    }

    [Fact]
    public async Task MonthlyMultipleFixedServicesAndUnassignedService()
    {
        using var db = Context(); var f = await MonthlyFixture(db);
        var internet = await SupplementalAsync(db, f.A, "Internet", 150000);
        var trash = await SupplementalAsync(db, f.A, "Rác", 50000);
        await AddService(db, "Dịch vụ không chọn", 900000);
        var service = new HoaDonDichVuService(db, new(db));
        var preview = await service.XemThangAsync(1, 1, 2026, 10);
        Assert.Equal(1480000, preview.DuKien.Single(x => x.HoaDon.HopDongId == f.A).HoaDon.TongTien);
        Assert.Equal(1280000, preview.DuKien.Single(x => x.HoaDon.HopDongId == f.B).HoaDon.TongTien);
        await service.PhatHanhThangAsync(1, 1, 2026, 10);
        var invoice = await db.HoaDons.Include(x => x.ChiTiet).SingleAsync(x => x.HopDongId == f.A);
        Assert.Equal(5, invoice.ChiTiet.Count);
        Assert.Equal(150000, invoice.ChiTiet.Single(x => x.DichVuId == internet).ThanhTien);
        Assert.Equal(50000, invoice.ChiTiet.Single(x => x.DichVuId == trash).ThanhTien);
        Assert.All(invoice.ChiTiet.Where(x => x.CachTinhApDung == CachTinhDichVu.CoDinh), x => Assert.Equal(1m, x.SoLuong));
        Assert.Equal(invoice.ChiTiet.Sum(x => x.ThanhTien), invoice.TongTien);
    }

    [Theory]
    [InlineData(0, 1, 1360000)]
    [InlineData(1, 2, 1440000)]
    [InlineData(3, 4, 1600000)]
    public async Task MonthlyPeopleFeesUseSignerAndRoommates(int roommates, int people, long expected)
    {
        using var db = Context(); var f = await MonthlyFixture(db);
        await db.Database.ExecuteSqlRawAsync("UPDATE phong_tro SET so_nguoi_toi_da=10");
        for (var i = 0; i < roommates; i++) await AddStay(db, f.A, new(2026, 9, 15));
        var fee = await SupplementalAsync(db, f.A, "Vệ sinh theo người", 80000, CachTinhDichVu.TheoNguoi);
        await new HoaDonDichVuService(db, new(db)).PhatHanhThangAsync(1, 1, 2026, 10);
        var invoice = await db.HoaDons.Include(x => x.ChiTiet).SingleAsync(x => x.HopDongId == f.A);
        Assert.Equal(people, invoice.SoNguoiTinhPhi);
        Assert.Equal((decimal)people, invoice.ChiTiet.Single(x => x.DichVuId == fee).SoLuong);
        Assert.Equal(80000L * people, invoice.ChiTiet.Single(x => x.DichVuId == fee).ThanhTien);
        Assert.Equal(expected, invoice.TongTien);
    }

    [Fact]
    public async Task MonthlyAllAmountsSnapshotSurvivesChanges()
    {
        using var db = Context(); var f = await MonthlyFixture(db);
        var internet = await SupplementalAsync(db, f.A, "Internet", 150000);
        await SupplementalAsync(db, f.A, "Rác", 50000);
        var fee = await SupplementalAsync(db, f.A, "Vệ sinh theo người", 80000, CachTinhDichVu.TheoNguoi);
        await AddStay(db, f.A, new(2026, 9, 30));
        await new HoaDonDichVuService(db, new(db)).PhatHanhThangAsync(1, 1, 2026, 10);
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE cau_hinh_dich_vu SET don_gia=999999 WHERE dich_vu_id={internet} OR dich_vu_id={fee}");
        await db.Database.ExecuteSqlRawAsync("UPDATE phong_tro SET so_nguoi_toi_da=10");
        await AddStay(db, f.A, new(2026, 9, 10));
        var invoice = await db.HoaDons.AsNoTracking().Include(x => x.ChiTiet).SingleAsync(x => x.HopDongId == f.A);
        Assert.Equal(1640000, invoice.TongTien);
        Assert.Equal(2, invoice.SoNguoiTinhPhi);
        Assert.Equal(6, invoice.ChiTiet.Count);
        Assert.Equal(150000, invoice.ChiTiet.Single(x => x.DichVuId == internet).ThanhTien);
        Assert.Equal(160000, invoice.ChiTiet.Single(x => x.DichVuId == fee).ThanhTien);
        using var factory = BillingWeb(); using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        await Login(client, "owner");
        var html = WebUtility.HtmlDecode(await client.GetStringAsync($"/HoaDonDichVu/Details/{invoice.Id}"));
        Assert.Contains("Khoán theo đầu người", html); Assert.Contains("Cố định theo phòng", html);
        Assert.Contains("1.640.000 đ", html); Assert.Contains("2 người", html); Assert.DoesNotContain("999.999", html);
    }

    [Fact]
    public async Task MonthlyPeopleArrivalAndDepartureRulesAndRoomPrice()
    {
        using var db = Context(); var f = await MonthlyFixture(db);
        await db.Database.ExecuteSqlRawAsync("UPDATE phong_tro SET so_nguoi_toi_da=10");
        await AddStay(db, f.A, new(2026, 9, 1), new(2026, 10, 1)); // billed for departure month
        await AddStay(db, f.A, new(2026, 10, 1)); // billed starting next month
        await AddStay(db, f.A, new(2026, 8, 1), new(2026, 9, 30)); // no October fee
        var fee = await SupplementalAsync(db, f.A, "Vệ sinh theo người", 80000, CachTinhDichVu.TheoNguoi);
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE dich_vu_phong SET don_gia_rieng=90000 WHERE phong_id={f.A} AND dich_vu_toa_nha_id=(SELECT id FROM dich_vu_toa_nha WHERE dich_vu_id={fee} AND toa_nha_id=1)");
        var model = await new HoaDonDichVuService(db, new(db)).XemThangAsync(1, 1, 2026, 10);
        var invoice = model.DuKien.Single(x => x.HoaDon.HopDongId == f.A).HoaDon;
        Assert.Equal(2, invoice.SoNguoiTinhPhi); Assert.Equal(180000, invoice.ChiTiet.Single(x => x.DichVuId == fee).ThanhTien);
        Assert.Equal(1460000, invoice.TongTien);
    }

    [Fact]
    public async Task MonthlyStoppedAndContractExcludedServicesAreNotCharged()
    {
        using var db = Context(); var f = await MonthlyFixture(db);
        var included = await SupplementalAsync(db, f.A, "Internet", 150000);
        var excluded = await SupplementalAsync(db, f.A, "Ngoài hợp đồng", 50000);
        var stopped = await SupplementalAsync(db, f.A, "Đã ngừng", 90000);
        await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO ngung_dich_vu_phong(dich_vu_phong_id,yeu_cau_luc_utc,ngung_tu_ky) SELECT id,'2026-09-01','2026-10-01' FROM dich_vu_phong WHERE phong_id={f.A} AND dich_vu_toa_nha_id=(SELECT id FROM dich_vu_toa_nha WHERE dich_vu_id={stopped} AND toa_nha_id=1)");
        var service = new HoaDonDichVuService(db, new(db));
        foreach (var id in new[] { f.Dien, f.Nuoc, included, stopped }) await service.GanVaoHopDongAsync(1, f.A, id, new(2026,10,1));
        var invoice = (await service.XemThangAsync(1,1,2026,10)).DuKien.Single(x=>x.HoaDon.HopDongId==f.A).HoaDon;
        Assert.DoesNotContain(invoice.ChiTiet,x=>x.DichVuId==excluded || x.DichVuId==stopped);
        Assert.Equal(1430000,invoice.TongTien);
    }

    [Fact]
    public async Task MonthlyPreviewCanPublishOnlySelectedRoomAndProtectsOwnership()
    {
        using var db = Context(); var f = await MonthlyFixture(db);
        await SupplementalAsync(db, f.A, "Internet", 150000);
        using var factory = BillingWeb(); using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        await Login(client, "owner");
        var url = $"/HoaDonDichVu/Preview?toaNhaId=1&hopDongId={f.A}&nam=2026&thang=10";
        var html = WebUtility.HtmlDecode(await client.GetStringAsync(url));
        Assert.Contains("Internet", html); Assert.Contains("1.430.000 đ", html); Assert.DoesNotContain("mi-steps",html);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/HoaDonDichVu/Preview?toaNhaId=2&hopDongId={f.A}&nam=2026&thang=10")).StatusCode);
        var token = System.Text.RegularExpressions.Regex.Match(html,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;
        Assert.NotEmpty(token);
        var response = await client.PostAsync("/HoaDonDichVu/IssueMonthly", new FormUrlEncodedContent(new Dictionary<string,string>
        { ["toaNhaId"]="1",["nam"]="2026",["thang"]="10",["hopDongId"]=f.A.ToString(),["__RequestVerificationToken"]=token }));
        Assert.Equal(HttpStatusCode.Redirect,response.StatusCode);
        using var verify = Context(); var invoice = await verify.HoaDons.SingleAsync();
        Assert.Equal(f.A,invoice.HopDongId); Assert.Equal(1430000,invoice.TongTien);
        Assert.False(await verify.ChiSoDienNuocs.AnyAsync(x=>x.HopDongId==f.B && x.DaKhoa));
        await Login(client,"other"); Assert.Equal(HttpStatusCode.Forbidden,(await client.GetAsync(url)).StatusCode);
    }
}
