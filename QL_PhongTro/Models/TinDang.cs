using System.ComponentModel.DataAnnotations.Schema;

namespace QL_PhongTro.Models;

[Table("tin_dang")]
public class TinDang
{
    [Column("id")] public int Id { get; set; }
    [Column("phong_id")] public int PhongId { get; set; }
    [Column("tieu_de")] public string TieuDe { get; set; } = "";
    [Column("ngay_het_han")] public DateTime? NgayHetHan { get; set; }
    [Column("trang_thai")] public string TrangThai { get; set; } = "";
}
