using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_PhongTro.Models;

// In-app notification. Large booking products keep this channel always on and treat
// email as an optional extra, so a tenant sees the change as soon as they sign in.
[Table("thong_bao")]
public class ThongBao
{
    [Key]
    public int Id { get; set; }

    public int NguoiNhanId { get; set; }

    [Required, MaxLength(40)]
    public string Loai { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string TieuDe { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string NoiDung { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? DuongDan { get; set; }

    public bool DaDoc { get; set; }
    public DateTime NgayTao { get; set; }
}

public static class LoaiThongBao
{
    public const string YeuCauXacNhan = "YEU_CAU_XAC_NHAN";
    public const string YeuCauDoiLich = "YEU_CAU_DOI_LICH";
    public const string YeuCauTuChoi = "YEU_CAU_TU_CHOI";
    public const string YeuCauDuyet = "YEU_CAU_DUYET";
}
