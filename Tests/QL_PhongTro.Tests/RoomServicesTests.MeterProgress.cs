using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Models;
using QL_PhongTro.Services;
using Xunit;

namespace QL_PhongTro.Tests;

public sealed partial class RoomServicesTests
{
    [Fact]
    public async Task MeterProgressCountsLeasedRoomsAndRequiresBothMonthlyReadings()
    {
        using var db = Context();
        var fixture = await MonthlyFixture(db);
        await db.Database.ExecuteSqlRawAsync("DELETE FROM chi_so_dien_nuoc WHERE tu_ngay='2026-10-01'");
        var octoberStart = new DateOnly(2026, 10, 1);
        var octoberEnd = new DateOnly(2026, 10, 31);
        db.ChiSoDienNuocs.AddRange(
            new ChiSoDienNuoc { HopDongId = fixture.A, DichVuId = fixture.Dien, TuNgay = octoberStart, DenNgay = octoberEnd, ChiSoDau = 1, ChiSoCuoi = 2, NguoiNhapId = 1, NgayNhap = DateTime.UtcNow },
            new ChiSoDienNuoc { HopDongId = fixture.A, DichVuId = fixture.Nuoc, TuNgay = octoberStart, DenNgay = octoberEnd, ChiSoDau = 1, ChiSoCuoi = 2, NguoiNhapId = 1, NgayNhap = DateTime.UtcNow },
            new ChiSoDienNuoc { HopDongId = fixture.B, DichVuId = fixture.Dien, TuNgay = octoberStart, DenNgay = octoberEnd, ChiSoDau = 1, ChiSoCuoi = 2, NguoiNhapId = 1, NgayNhap = DateTime.UtcNow },
            new ChiSoDienNuoc { HopDongId = fixture.B, DichVuId = fixture.Nuoc, TuNgay = octoberStart, DenNgay = octoberEnd, ChiSoDau = 1, ChiSoCuoi = 2, NguoiNhapId = 1, NgayNhap = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var service = new TienDoChiSoService(db);
        var october = Assert.Single((await service.XemAsync(1, 2026, 10, default)).ToaNhas);
        Assert.Equal((2, 2, 0), (october.TongPhongDangThue, october.SoPhongDaChot, october.SoPhongConThieu));

        db.ChangeTracker.Clear();
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM chi_so_dien_nuoc WHERE hop_dong_id={fixture.B} AND dich_vu_id={fixture.Nuoc}");
        var partlySaved = Assert.Single((await service.XemAsync(1, 2026, 10, default)).ToaNhas);
        Assert.Equal((2, 1, 1), (partlySaved.TongPhongDangThue, partlySaved.SoPhongDaChot, partlySaved.SoPhongConThieu));

        db.ChangeTracker.Clear();
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM chi_so_dien_nuoc WHERE hop_dong_id={fixture.A} AND dich_vu_id={fixture.Nuoc}");
        var noneSaved = Assert.Single((await service.XemAsync(1, 2026, 10, default)).ToaNhas);
        Assert.Equal((2, 0, 2), (noneSaved.TongPhongDangThue, noneSaved.SoPhongDaChot, noneSaved.SoPhongConThieu));
    }

    [Fact]
    public async Task MeterProgressUsesSelectedMonthAndContractDateOverlapRatherThanCurrentRoomStatus()
    {
        using var db = Context();
        var fixture = await MonthlyFixture(db);
        await db.Database.ExecuteSqlRawAsync("DELETE FROM chi_so_dien_nuoc WHERE tu_ngay='2026-10-01'");
        var futureRoom = await AddRoom(db, "FUTURE");
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE phong_tro SET trang_thai='TRONG' WHERE id={fixture.B};
            UPDATE hop_dong SET trang_thai='DA_KET_THUC', ngay_tra_phong='2026-10-20' WHERE id={fixture.B};
            INSERT INTO hop_dong(id,ma_hop_dong,phong_id,khach_dung_ten_id,trang_thai,nguoi_lap_id,ngay_tao)
            VALUES({futureRoom.Id},'HD-FUTURE',{futureRoom.Id},1,'DANG_HIEU_LUC',1,'2026-01-01');
            """);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO ky_hop_dong(hop_dong_id,so_thu_tu,ngay_bat_dau,ngay_ket_thuc,so_thang,gia_thue,nguoi_lap_id,ngay_tao)
            VALUES({futureRoom.Id},1,'2026-11-01','2027-10-31',12,1000000,1,'2026-01-01');
            UPDATE ky_hop_dong SET ngay_ket_thuc='2026-10-20',so_thang=10 WHERE hop_dong_id={fixture.B};
            """);

        var service = new TienDoChiSoService(db);
        var october = Assert.Single((await service.XemAsync(1, 2026, 10, default)).ToaNhas);
        Assert.Equal((2, 0, 2), (october.TongPhongDangThue, october.SoPhongDaChot, october.SoPhongConThieu));

        var november = Assert.Single((await service.XemAsync(1, 2026, 11, default)).ToaNhas);
        Assert.Equal((2, 0, 2), (november.TongPhongDangThue, november.SoPhongDaChot, november.SoPhongConThieu));
    }

    [Fact]
    public async Task MeterProgressReturnsOnlyBuildingsOwnedByTheRequestingLandlord()
    {
        using var db = Context();
        await MonthlyFixture(db);

        var ownerBuildings = await new TienDoChiSoService(db).XemAsync(1, 2026, 10, default);
        Assert.All(ownerBuildings.ToaNhas, x => Assert.Equal(1, x.ToaNhaId));

        var otherOwnerBuildings = await new TienDoChiSoService(db).XemAsync(2, 2026, 10, default);
        Assert.All(otherOwnerBuildings.ToaNhas, x => Assert.Equal(2, x.ToaNhaId));
    }

    [Fact]
    public async Task MissingRoomDetailsMatchProgressAndIncludeAssignedOrUnassignedManager()
    {
        using var db = Context();
        var fixture = await MonthlyFixture(db);
        await AddRoom(db, "VACANT-NO-CONTRACT");
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM chi_so_dien_nuoc WHERE hop_dong_id={fixture.B} AND dich_vu_id={fixture.Nuoc}");
        await db.Database.ExecuteSqlRawAsync(
            "UPDATE tai_khoan SET vai_tro='QUAN_LY' WHERE id=2; UPDATE toa_nha SET quan_ly_id=2 WHERE id=1;");

        var service = new TienDoChiSoService(db);
        var progress = Assert.Single((await service.XemAsync(1, 2026, 10, default)).ToaNhas);
        Assert.Equal(1, progress.SoPhongConThieu);
        var details = await service.PhongConThieuAsync(1, 1, 2026, 10, default);
        var room = Assert.Single(details.Phongs);
        Assert.Equal(fixture.B, room.PhongId);
        Assert.Equal("DEMO-B", room.MaPhong);
        Assert.Equal("Other", room.TenQuanLy);
        Assert.DoesNotContain(details.Phongs, x => x.MaPhong == "VACANT-NO-CONTRACT");

        await db.Database.ExecuteSqlRawAsync("UPDATE toa_nha SET quan_ly_id=NULL WHERE id=1;");
        var unassigned = Assert.Single((await service.PhongConThieuAsync(1, 1, 2026, 10, default)).Phongs);
        Assert.Null(unassigned.TenQuanLy);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.PhongConThieuAsync(2, 1, 2026, 10, default));
    }

    [Fact]
    public async Task MeterProgressPageIsAvailableOnlyToLandlordsAndDisplaysSelectedPeriod()
    {
        using var db = Context();
        await MonthlyFixture(db);
        using var factory = BillingWeb();
        using var anonymous = factory.CreateClient(new() { AllowAutoRedirect = false });
        Assert.Equal(System.Net.HttpStatusCode.Redirect,
            (await anonymous.GetAsync("/TienDoChiSo?nam=2026&thang=10")).StatusCode);

        using var owner = factory.CreateClient(new() { AllowAutoRedirect = false });
        await Login(owner, "owner");
        var response = await owner.GetAsync("/TienDoChiSo?nam=2026&thang=10");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Kỳ 10/2026", html);
        Assert.Contains("Building A", html);
        Assert.Contains("name=\"thang\"", html);
        Assert.Contains("name=\"nam\"", html);

        await Login(owner, "tenant");
        Assert.Equal(System.Net.HttpStatusCode.Forbidden,
            (await owner.GetAsync("/TienDoChiSo?nam=2026&thang=10")).StatusCode);
    }

    [Fact]
    public async Task MissingRoomDetailsPageLinksFromProgressEnforcesBuildingScopeAndShowsEmptyState()
    {
        using var db = Context();
        var fixture = await MonthlyFixture(db);
        await AddRoom(db, "VACANT-NO-CONTRACT");
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM chi_so_dien_nuoc WHERE hop_dong_id={fixture.B} AND dich_vu_id={fixture.Nuoc}");
        await db.Database.ExecuteSqlRawAsync(
            "UPDATE tai_khoan SET vai_tro='QUAN_LY' WHERE id=2; UPDATE toa_nha SET quan_ly_id=2 WHERE id=1;");

        using var factory = BillingWeb();
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        await Login(client, "owner");
        var progressHtml = System.Net.WebUtility.HtmlDecode(
            await client.GetStringAsync("/TienDoChiSo?nam=2026&thang=10"));
        Assert.Contains("/TienDoChiSo/PhongConThieu?toaNhaId=1&nam=2026&thang=10", progressHtml);

        var detailsResponse = await client.GetAsync("/TienDoChiSo/PhongConThieu?toaNhaId=1&nam=2026&thang=10");
        Assert.Equal(System.Net.HttpStatusCode.OK, detailsResponse.StatusCode);
        var detailsHtml = System.Net.WebUtility.HtmlDecode(await detailsResponse.Content.ReadAsStringAsync());
        Assert.Contains("DEMO-B", detailsHtml);
        Assert.Contains("Other", detailsHtml);
        Assert.DoesNotContain("DEMO-A", detailsHtml);
        Assert.DoesNotContain("VACANT-NO-CONTRACT", detailsHtml);
        Assert.Equal(System.Net.HttpStatusCode.Forbidden,
            (await client.GetAsync("/TienDoChiSo/PhongConThieu?toaNhaId=2&nam=2026&thang=10")).StatusCode);

        await db.Database.ExecuteSqlRawAsync("UPDATE toa_nha SET quan_ly_id=NULL WHERE id=1;");
        var noManager = System.Net.WebUtility.HtmlDecode(
            await (await client.GetAsync("/TienDoChiSo/PhongConThieu?toaNhaId=1&nam=2026&thang=10"))
                .Content.ReadAsStringAsync());
        Assert.Contains("Chưa phân công", noManager);

        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO chi_so_dien_nuoc(hop_dong_id,dich_vu_id,tu_ngay,den_ngay,chi_so_dau,chi_so_cuoi,nguoi_nhap_id,ngay_nhap)
            VALUES({fixture.B},{fixture.Nuoc},'2026-10-01','2026-10-31','20','25',1,'2026-10-31');
            """);
        var empty = System.Net.WebUtility.HtmlDecode(
            await (await client.GetAsync("/TienDoChiSo/PhongConThieu?toaNhaId=1&nam=2026&thang=10"))
                .Content.ReadAsStringAsync());
        Assert.Contains("Tòa nhà không còn phòng nào thiếu chỉ số", empty);
        Assert.Contains("Danh sách phòng (0)", empty);
    }
}
