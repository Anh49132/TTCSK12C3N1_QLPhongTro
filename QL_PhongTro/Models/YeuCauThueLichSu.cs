using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_PhongTro.Models;

// Append-only business history of a viewing request, kept apart from the administrator
// activity log. S2-06 owns yeu_cau_thue, so this table declares no foreign key to it:
// a request row is never deleted and the history has to survive any change to that
// table's shape. The link is a plain integer kept consistent by LichHenService.
[Table("yeu_cau_thue_lich_su")]
public class YeuCauThueLichSu
{
    [Column("id")] public int Id { get; set; }

    [Column("yeu_cau_thue_id")] public int? YeuCauThueId { get; set; }

    [Column("trang_thai_cu"), MaxLength(25)] public string? TrangThaiCu { get; set; }

    [Column("trang_thai_moi"), Required, MaxLength(25)] public string TrangThaiMoi { get; set; } = string.Empty;

    [Column("hanh_dong"), Required, MaxLength(40)] public string HanhDong { get; set; } = string.Empty;

    [Column("nguoi_thuc_hien_id")] public int? NguoiThucHienId { get; set; }

    [Column("ten_nguoi_thuc_hien"), MaxLength(100)] public string? TenNguoiThucHien { get; set; }

    [Column("vai_tro_luc_thuc_hien"), MaxLength(20)] public string? VaiTroLucThucHien { get; set; }

    [Column("lich_hen_cu")] public DateTime? LichHenCu { get; set; }

    [Column("lich_hen_moi")] public DateTime? LichHenMoi { get; set; }

    [Column("ly_do_tu_choi"), MaxLength(30)] public string? LyDoTuChoi { get; set; }

    [Column("ghi_chu_tu_choi"), MaxLength(500)] public string? GhiChuTuChoi { get; set; }

    [Column("thoi_diem"), Required] public DateTime ThoiDiem { get; set; }
}

public static class HanhDongYeuCau
{
    public const string XacNhan = "XAC_NHAN";

    public static string Label(string hanhDong) => hanhDong switch
    {
        XacNhan => "Chủ nhà xác nhận lịch hẹn",
        _ => hanhDong
    };
}
