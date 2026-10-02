using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_PhongTro.Models;

[Table("yeu_cau_thue")]
public sealed class YeuCauThue
{
    [Key, Column("id")]
    public int Id { get; set; }

    [Required, MaxLength(30), Column("ma_yeu_cau")]
    public string MaYeuCau { get; set; } = "";

    [Column("khach_thue_id")]
    public int KhachThueId { get; set; }

    [Required, MaxLength(20), Column("loai_yeu_cau")]
    public string LoaiYeuCau { get; set; } = "";

    [Column("ngay_mong_muon")]
    public DateOnly NgayMongMuon { get; set; }

    [Column("so_nguoi_du_kien")]
    public int SoNguoiDuKien { get; set; }

    [Column("loi_nhan")]
    public string? LoiNhan { get; set; }

    [Column("lich_hen")]
    public DateTime? LichHen { get; set; }

    [Required, MaxLength(25), Column("trang_thai")]
    public string TrangThai { get; set; } = TrangThaiYeuCauThue.Moi;

    [Column("ly_do_tu_choi")]
    public string? LyDoTuChoi { get; set; }

    [Column("nguoi_xu_ly_id")]
    public int? NguoiXuLyId { get; set; }

    [Column("ngay_xu_ly")]
    public DateTime? NgayXuLy { get; set; }

    [Column("ngay_tao")]
    public DateTime NgayTao { get; set; }

    [Column("phien_ban")]
    public int PhienBan { get; set; }
}

public static class TrangThaiYeuCauThue
{
    public const string Moi = "MOI";
    public const string DaHenLich = "DA_HEN_LICH";
    public const string DaDuyet = "DA_DUYET";
    public const string TuChoi = "TU_CHOI";
    public const string DaHuy = "DA_HUY";

    public static bool CoTheHuy(string trangThai) => trangThai is Moi or DaHenLich;
}
