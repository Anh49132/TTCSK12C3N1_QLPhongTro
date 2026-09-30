using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_PhongTro.Models;

[Table("yeu_cau_thue")]
public class YeuCauThue
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(30)]
    public string MaYeuCau { get; set; } = string.Empty;

    public int PhongId { get; set; }
    public int KhachThueId { get; set; }

    [Required, MaxLength(20)]
    public string LoaiYeuCau { get; set; } = YeuCauThueTrangThai.LoaiXemPhong;

    public DateOnly NgayMongMuon { get; set; }
    public int SoNguoiDuKien { get; set; }
    public string? LoiNhan { get; set; }

    public DateTime? LichHen { get; set; }
    public DateTime? LichHenCu { get; set; }
    public bool DaDoiLich { get; set; }

    [Required, MaxLength(25)]
    public string TrangThai { get; set; } = YeuCauThueTrangThai.Moi;

    [MaxLength(30)]
    public string? LyDoTuChoi { get; set; }

    [MaxLength(500)]
    public string? GhiChuTuChoi { get; set; }

    public int? NguoiXuLyId { get; set; }
    public DateTime? NgayXuLy { get; set; }
    public DateTime NgayTao { get; set; }
    public int PhienBan { get; set; }
}

public static class YeuCauThueTrangThai
{
    public const string Moi = "MOI";
    public const string DaHenLich = "DA_HEN_LICH";
    public const string DaDuyet = "DA_DUYET";
    public const string TuChoi = "TU_CHOI";
    public const string DaHuy = "DA_HUY";

    public const string LoaiXemPhong = "XEM_PHONG";
    public const string LoaiThueNgay = "THUE_NGAY";

    public static string Label(string trangThai) => trangThai switch
    {
        Moi => "Chờ xác nhận",
        DaHenLich => "Đã xác nhận",
        DaDuyet => "Đã duyệt",
        TuChoi => "Đã từ chối",
        DaHuy => "Đã huỷ",
        _ => trangThai
    };

    public static string LoaiLabel(string loai) => loai switch
    {
        LoaiXemPhong => "Xem phòng",
        LoaiThueNgay => "Thuê ngay",
        _ => loai
    };
}
