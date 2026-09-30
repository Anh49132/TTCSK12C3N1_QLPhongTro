using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_PhongTro.Models;

// Append-only business history of a request, separate from the admin activity log.
// Tenants read this table; nhat_ky_hoat_dong stays for the administrator.
[Table("yeu_cau_thue_lich_su")]
public class YeuCauThueLichSu
{
    [Key]
    public int Id { get; set; }

    public int YeuCauThueId { get; set; }

    [MaxLength(25)]
    public string? TrangThaiCu { get; set; }

    [Required, MaxLength(25)]
    public string TrangThaiMoi { get; set; } = string.Empty;

    [Required, MaxLength(40)]
    public string HanhDong { get; set; } = string.Empty;

    public int? NguoiThucHienId { get; set; }

    [MaxLength(100)]
    public string? TenNguoiThucHien { get; set; }

    [MaxLength(20)]
    public string? VaiTroLucThucHien { get; set; }

    public DateTime? LichHenCu { get; set; }
    public DateTime? LichHenMoi { get; set; }

    [MaxLength(30)]
    public string? LyDoTuChoi { get; set; }

    [MaxLength(500)]
    public string? GhiChuTuChoi { get; set; }

    public DateTime ThoiDiem { get; set; }
}

public static class HanhDongYeuCau
{
    public const string TaoYeuCau = "TAO_YEU_CAU";
    public const string XacNhan = "XAC_NHAN";
    public const string DoiLich = "DOI_LICH";
    public const string TuChoi = "TU_CHOI";
    public const string DuyetThueNgay = "DUYET_THUE_NGAY";

    public static string Label(string hanhDong) => hanhDong switch
    {
        TaoYeuCau => "Khách tạo yêu cầu",
        XacNhan => "Chủ nhà xác nhận lịch hẹn",
        DoiLich => "Chủ nhà đổi lịch hẹn",
        TuChoi => "Chủ nhà từ chối",
        DuyetThueNgay => "Chủ nhà duyệt thuê ngay",
        _ => hanhDong
    };
}
