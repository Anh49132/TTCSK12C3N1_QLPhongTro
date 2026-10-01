using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_PhongTro.Models;

[Table("tin_dang")]
public class TinDang
{
    [Column("id")] public int Id { get; set; }
    [Column("phong_id")] public int PhongId { get; set; }
    [Column("nguoi_dang_id")] public int NguoiDangId { get; set; }
    [Column("tin_goc_id")] public int? TinGocId { get; set; }
    [Column("tieu_de"), Required, MaxLength(200)] public string TieuDe { get; set; } = "";
    [Column("noi_dung")] public string? NoiDung { get; set; }
    [Column("ngay_dang")] public DateTime? NgayDang { get; set; }
    [Column("ngay_het_han")] public DateTime? NgayHetHan { get; set; }
    [Column("trang_thai"), Required] public string TrangThai { get; set; } = "NHAP";
    [Column("ngay_tao")] public DateTime NgayTao { get; set; }
}
