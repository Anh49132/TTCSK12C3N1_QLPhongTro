using System.ComponentModel.DataAnnotations;
namespace QL_PhongTro.Models;
public static class TrangThaiYeuCau
{
    public const string Moi = "MOI", DaHenLich = "DA_HEN_LICH", DaDuyet = "DA_DUYET", TuChoi = "TU_CHOI", DaHuy = "DA_HUY";
    public static readonly IReadOnlyDictionary<string, string> Labels = new Dictionary<string, string>
    { [Moi] = "Mới", [DaHenLich] = "Đã hẹn lịch", [DaDuyet] = "Đã duyệt", [TuChoi] = "Từ chối", [DaHuy] = "Đã huỷ" };

    // A scheduled request has already been handled by the owner. Only a new request
    // belongs in the "chua xu ly" counter and can become overdue.
    public static bool ChuaXuLy(string trangThai) => trangThai == Moi;

    public static string LoaiLabel(string loaiYeuCau) => loaiYeuCau switch
    {
        LichHenTrangThai.LoaiXemPhong => "Xem phòng",
        LichHenTrangThai.LoaiThueNgay => "Thuê ngay",
        _ => loaiYeuCau
    };
}
public class YeuCau
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(30)] public string MaYeuCau { get; set; } = string.Empty;
    public int KhachThueId { get; set; }
    public int PhongId { get; set; }
    public int ToaNhaId { get; set; }
    [Required, MaxLength(50)] public string LoaiYeuCau { get; set; } = string.Empty;
    public DateOnly? NgayMongMuon { get; set; }
    [Required, MaxLength(25)] public string TrangThai { get; set; } = TrangThaiYeuCau.Moi;
    public DateTime NgayTao { get; set; }
}
