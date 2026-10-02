using System.Net;
using System.Globalization;
using System.Text.Json;
using QL_PhongTro.Data;
using QL_PhongTro.Models;
using QL_PhongTro.Services;
using Xunit;

namespace QL_PhongTro.Tests;

public sealed partial class PermissionTests
{
    [Fact]
    public async Task AnonymousGuestCanReadListingDetailsAndPhotos()
    {
        var listingId = CreatePublicListing("TRONG", "DANG_HIEN_THI", includePhoto: true);
        using var guest = Client();

        var page = await guest.GetAsync($"/TinDang/ChiTiet/{listingId}");
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        Assert.True(page.Headers.CacheControl?.NoStore);
        var html = WebUtility.HtmlDecode(await page.Content.ReadAsStringAsync());
        Assert.Contains("Phòng gần trung tâm, có cửa sổ", html);
        Assert.Contains("2.500.000", html);
        Assert.Contains("25,5 m²", html);
        Assert.Contains("2 người", html);
        Assert.Contains("1.000.000 đ", html);
        Assert.Contains("Phòng sáng, thoáng và có chỗ để xe.", html);
        Assert.Contains("/images/room-a-small.jpg", html);
        Assert.Contains("Chưa có đơn giá dịch vụ theo mức sử dụng.", html);
        Assert.Contains("Tin này chưa có khoản phí cố định hàng tháng.", html);

        var response = await guest.GetAsync($"/api/tin-dang/{listingId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.CacheControl?.NoStore);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = json.RootElement;
        Assert.Equal(listingId, data.GetProperty("id").GetInt32());
        Assert.Equal(2500000, data.GetProperty("giaThue").GetInt64());
        Assert.Equal(25.5m, data.GetProperty("dienTich").GetDecimal());
        Assert.Equal(2, data.GetProperty("soNguoiToiDa").GetInt32());
        Assert.Equal(1000000, data.GetProperty("tienCocDuKien").GetInt64());
        Assert.Equal("Phòng sáng, thoáng và có chỗ để xe.", data.GetProperty("moTa").GetString());
        Assert.Single(data.GetProperty("anh").EnumerateArray());
        Assert.Empty(data.GetProperty("dichVuTheoSuDung").EnumerateArray());
        Assert.Empty(data.GetProperty("khoanCoDinh").EnumerateArray());
    }

    [Fact]
    public async Task AnonymousGuestCanSeeUtilityRatesAndMultipleMonthlyFixedFees()
    {
        var listingId = CreatePublicListing("TRONG", "DANG_HIEN_THI", includePhoto: false);
        DichVuSchemaInitializer.Initialize(database);
        var (roomId, buildingId) = GetListingRoomAndBuilding(listingId);
        var ownerId = accounts["CHU_NHA"];

        var electricity = CreateService("DIEN", "Điện");
        var water = CreateService("NUOC", "Nước");
        var garbage = CreateService("RAC", "Rác");
        var parking = CreateService("GUI_XE", "Gửi xe");
        var internet = CreateService("INTERNET", "Internet");
        AddServicePrice(buildingId, electricity, ownerId, CachTinhDichVu.TheoChiSo, "kWh", 3500);
        AddServicePrice(buildingId, water, ownerId, CachTinhDichVu.TheoChiSo, "m³", 25000);
        AddServicePrice(buildingId, garbage, ownerId, CachTinhDichVu.TheoNguoi, "người/tháng", 20000);
        AddServicePrice(buildingId, parking, ownerId, CachTinhDichVu.CoDinh, "phòng/tháng", 100000);
        AddServicePrice(buildingId, internet, ownerId, CachTinhDichVu.CoDinh, "phòng/tháng", 150000);

        using var guest = Client();
        var html = WebUtility.HtmlDecode(await guest.GetStringAsync($"/TinDang/ChiTiet/{listingId}"));
        Assert.Contains("Điện", html);
        Assert.Contains("Theo chỉ số", html);
        Assert.Contains("kWh", html);
        Assert.Contains("3.500", html);
        Assert.Contains("Nước", html);
        Assert.Contains("m³", html);
        Assert.Contains("25.000", html);
        Assert.Contains("Rác", html);
        Assert.Contains("Gửi xe", html);
        Assert.Contains("100.000", html);
        Assert.Contains("Internet", html);
        Assert.Contains("150.000", html);

        using var response = await guest.GetAsync($"/api/tin-dang/{listingId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = json.RootElement;
        var usage = data.GetProperty("dichVuTheoSuDung").EnumerateArray().ToArray();
        var fixedFees = data.GetProperty("khoanCoDinh").EnumerateArray().ToArray();
        Assert.Equal(3, usage.Length);
        Assert.Equal(2, fixedFees.Length);
        Assert.Contains(usage, item => item.GetProperty("tenDichVu").GetString() == "Điện"
            && item.GetProperty("donViTinh").GetString() == "kWh"
            && item.GetProperty("donGia").GetInt64() == 3500);
        Assert.Contains(fixedFees, item => item.GetProperty("tenDichVu").GetString() == "Gửi xe"
            && item.GetProperty("donGia").GetInt64() == 100000);
    }

    [Fact]
    public async Task GuestPricingUsesRoomOverrideAndOmitsInvalidOrInactivePrices()
    {
        var listingId = CreatePublicListing("TRONG", "DANG_HIEN_THI", includePhoto: false);
        DichVuSchemaInitializer.Initialize(database);
        var (roomId, buildingId) = GetListingRoomAndBuilding(listingId);
        var ownerId = accounts["CHU_NHA"];

        var electricity = CreateService("DIEN", "Điện");
        AddServicePrice(buildingId, electricity, ownerId, CachTinhDichVu.TheoChiSo, "kWh", 3500);
        AddServicePrice(buildingId, electricity, ownerId, CachTinhDichVu.TheoChiSo, "kWh", 4200, roomId, active: false);

        var water = CreateService("NUOC", "Nước");
        AddServicePrice(buildingId, water, ownerId, CachTinhDichVu.TheoChiSo, "m³", 0);

        var malformed = CreateService("KHAC", "Phí lỗi");
        AddServicePrice(buildingId, malformed, ownerId, CachTinhDichVu.TheoChiSo, "", 3000, ignoreChecks: true);

        using var guest = Client();
        var html = WebUtility.HtmlDecode(await guest.GetStringAsync($"/TinDang/ChiTiet/{listingId}"));
        Assert.Contains("Chưa có đơn giá dịch vụ theo mức sử dụng.", html);
        Assert.Contains("Tin này chưa có khoản phí cố định hàng tháng.", html);
        Assert.DoesNotContain("3.500", html);
        Assert.DoesNotContain("4.200", html);
        Assert.DoesNotContain("Phí lỗi", html);

        using var response = await guest.GetAsync($"/api/tin-dang/{listingId}");
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Empty(json.RootElement.GetProperty("dichVuTheoSuDung").EnumerateArray());
        Assert.Empty(json.RootElement.GetProperty("khoanCoDinh").EnumerateArray());
    }

    [Fact]
    public async Task MissingOrUnavailableListingReturnsNotFoundAndNoPhotoHasAnEmptyState()
    {
        var noPhotoId = CreatePublicListing("TRONG", "DANG_HIEN_THI", includePhoto: false);
        var hiddenId = CreatePublicListing("TRONG", "TAM_AN", includePhoto: false);
        var rentedId = CreatePublicListing("DANG_THUE", "DANG_HIEN_THI", includePhoto: false);
        var expiredId = CreatePublicListing("TRONG", "DANG_HIEN_THI", includePhoto: false, expired: true);
        using var guest = Client();

        foreach (var id in new[] { 999999, hiddenId, rentedId, expiredId })
        {
            Assert.Equal(HttpStatusCode.NotFound, (await guest.GetAsync($"/TinDang/ChiTiet/{id}")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await guest.GetAsync($"/api/tin-dang/{id}")).StatusCode);
        }

        var page = await guest.GetStringAsync($"/TinDang/ChiTiet/{noPhotoId}");
        Assert.Contains("Tin này chưa có hình ảnh.", page);
        var response = await guest.GetAsync($"/api/tin-dang/{noPhotoId}");
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Empty(json.RootElement.GetProperty("anh").EnumerateArray());
    }

    private int CreatePublicListing(string roomStatus, string listingStatus, bool includePhoto, bool expired = false)
    {
        var key = Guid.NewGuid().ToString("N");
        Execute("INSERT INTO toa_nha(chu_nha_id,ten_toa_nha,dia_chi,dang_hoat_dong) VALUES($owner,$name,'12 Đường Hoa',1)",
            ("$owner", accounts["CHU_NHA"]), ("$name", "Nhà test " + key));
        var buildingId = Convert.ToInt32(Scalar("SELECT id FROM toa_nha WHERE ten_toa_nha=$name", ("$name", "Nhà test " + key)));
        Execute("""
            INSERT INTO phong_tro(toa_nha_id,ma_phong,tang,dien_tich,gia_thue,tien_coc_du_kien,so_nguoi_toi_da,trang_thai,mo_ta,ngay_tao,phien_ban)
            VALUES($building,'P01',1,25.5,2500000,1000000,2,$status,'Phòng sáng, thoáng và có chỗ để xe.',$created,0)
            """, ("$building", buildingId), ("$status", roomStatus), ("$created", DateTime.UtcNow.ToString("O")));
        var roomId = Convert.ToInt32(Scalar("SELECT id FROM phong_tro WHERE toa_nha_id=$building AND ma_phong='P01'", ("$building", buildingId)));
        var expiry = DateTime.UtcNow.AddDays(expired ? -1 : 30).ToString("O");
        Execute("""
            INSERT INTO tin_dang(phong_id,nguoi_dang_id,tieu_de,noi_dung,ngay_dang,ngay_het_han,trang_thai,ngay_tao)
            VALUES($room,$owner,'Phòng gần trung tâm, có cửa sổ','Phòng sáng, thoáng và có chỗ để xe.',$published,$expiry,$status,$created)
            """, ("$room", roomId), ("$owner", accounts["CHU_NHA"]), ("$published", DateTime.UtcNow.ToString("O")),
            ("$expiry", expiry), ("$status", listingStatus), ("$created", DateTime.UtcNow.ToString("O")));
        var listingId = Convert.ToInt32(Scalar("SELECT id FROM tin_dang WHERE phong_id=$room", ("$room", roomId)));

        if (includePhoto)
            Execute("INSERT INTO anh_phong(phong_id,duong_dan,duong_dan_anh_nho,thu_tu,mo_ta,ngay_tao) VALUES($room,'/images/room-a.jpg','/images/room-a-small.jpg',1,'Phòng có cửa sổ',$created)",
                ("$room", roomId), ("$created", DateTime.UtcNow.ToString("O")));

        return listingId;
    }

    private (int RoomId, int BuildingId) GetListingRoomAndBuilding(int listingId)
    {
        var roomId = Convert.ToInt32(Scalar("SELECT phong_id FROM tin_dang WHERE id=$id", ("$id", listingId)));
        var buildingId = Convert.ToInt32(Scalar("SELECT toa_nha_id FROM phong_tro WHERE id=$id", ("$id", roomId)));
        return (roomId, buildingId);
    }

    private int CreateService(string code, string name, bool active = true)
    {
        Execute("INSERT INTO dich_vu(ma_dich_vu,ten_dich_vu,dang_hoat_dong) VALUES($code,$name,$active)",
            ("$code", code), ("$name", name), ("$active", active));
        return Convert.ToInt32(Scalar("SELECT id FROM dich_vu WHERE ma_dich_vu=$code", ("$code", code)));
    }

    private void AddServicePrice(int buildingId, int serviceId, int ownerId, string method, string unit,
        long price, int? roomId = null, bool active = true, bool ignoreChecks = false)
    {
        var roomValue = roomId.HasValue ? (object)roomId.Value : DBNull.Value;
        var startDate = DichVuService.HomNay().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var sql = """
            INSERT INTO cau_hinh_dich_vu(toa_nha_id,phong_id,dich_vu_id,cach_tinh,don_vi_tinh,don_gia,tu_ngay,dang_ap_dung,da_chot_gia,nguoi_tao_id,ngay_tao)
            VALUES($building,$room,$service,$method,$unit,$price,$start,$active,1,$owner,$created)
            """;
        if (ignoreChecks)
            sql = "PRAGMA ignore_check_constraints=ON; " + sql;
        Execute(sql, ("$building", buildingId), ("$room", roomValue), ("$service", serviceId),
            ("$method", method), ("$unit", unit), ("$price", price), ("$start", startDate),
            ("$active", active), ("$owner", ownerId), ("$created", DateTime.UtcNow.ToString("O")));
    }
}