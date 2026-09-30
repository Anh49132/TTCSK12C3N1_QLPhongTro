using QL_PhongTro.Models;
using QL_PhongTro.ViewModels;
using Xunit;

namespace QL_PhongTro.Tests;

public class AuditDisplayTests
{
    [Fact]
    public void RoomHistoryUsesVietnameseLabelsUnitsAndHistoricalName()
    {
        var result = AuditDisplay.Describe(new NhatKyHoatDong
        {
            LoaiDoiTuong = "phong_tro", DoiTuongId = 24, HanhDong = "TAO",
            DuLieuSau = """{"ma_phong":"304","dien_tich":20.5,"gia_thue":1500000,"trang_thai":"TRONG","loai_phong":null,"so_nguoi_toi_da":2}"""
        });
        Assert.Equal("Phòng “304”", result.Subject);
        Assert.Contains(result.Changes, x => x.Label == "Diện tích" && x.After == "20,5 m²");
        Assert.Contains(result.Changes, x => x.Label == "Giá thuê mỗi tháng" && x.After == "1.500.000 đ");
        Assert.Contains(result.Changes, x => x.Label == "Trạng thái" && x.After == "Trống");
        Assert.Contains(result.Changes, x => x.Label == "Loại phòng" && x.After == "Chưa có");
        Assert.Equal("Chủ nhà", AuditDisplay.Role("CHU_NHA"));
    }

    [Fact]
    public void UpdatesShowBeforeAfterWithoutInventingMissingHistory()
    {
        var result = AuditDisplay.Describe(new NhatKyHoatDong
        {
            LoaiDoiTuong = "phong_tro", DoiTuongId = 13, HanhDong = "SUA",
            DuLieuTruoc = """{"dien_tich":20}""", DuLieuSau = """{"dien_tich":21}"""
        });
        Assert.Equal("Phòng (mã 13)", result.Subject);
        Assert.Equal("Diện tích: 20 m² → 21 m²", result.Summary);
    }

    [Fact]
    public void DeletionKeepsHistoricalNameAndInvalidJsonDoesNotBreakPage()
    {
        var result = AuditDisplay.Describe(new NhatKyHoatDong
        {
            LoaiDoiTuong = "toa_nha", HanhDong = "XOA",
            DuLieuTruoc = """{"ten_toa_nha":"Tòa cũ","quan_ly_id":null,"dang_hoat_dong":false}"""
        });
        Assert.Equal("Tòa nhà “Tòa cũ”", result.Subject);
        Assert.Contains(result.Changes, x => x.Before == "Chưa phân công");
        Assert.Contains(result.Changes, x => x.Before == "Ngừng hoạt động");
        Assert.True(AuditDisplay.Describe(new NhatKyHoatDong { DuLieuTruoc = "bad json" }).Unreadable);
        Assert.True(AuditDisplay.Describe(new NhatKyHoatDong { DuLieuSau = "[]" }).Unreadable);
    }

    [Fact]
    public void DatesAndFreeTextAreNotMistakenForStatusCodes()
    {
        var result = AuditDisplay.Describe(new NhatKyHoatDong
        {
            LoaiDoiTuong = "hoa_don", HanhDong = "TAO",
            DuLieuSau = """{"tu_ngay":"2026-09-01","ngay_phat_hanh":"2026-09-30T18:30:00Z","nam":2026,"ten_khoan":"TRONG"}"""
        });
        Assert.Contains(result.Changes, x => x.After == "01/09/2026");
        Assert.Contains(result.Changes, x => x.After == "01/10/2026 01:30");
        Assert.Contains(result.Changes, x => x.Label == "Năm" && x.After == "2026");
        Assert.Contains(result.Changes, x => x.Label == "Tên khoản thu" && x.After == "TRONG");
    }
}
