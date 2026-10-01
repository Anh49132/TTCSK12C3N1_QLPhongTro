using System.Net;
using System.Text.Json;
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
}