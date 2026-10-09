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
}
