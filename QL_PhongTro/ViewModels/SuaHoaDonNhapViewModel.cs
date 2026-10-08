using System.ComponentModel.DataAnnotations;
namespace QL_PhongTro.ViewModels;

public sealed class SuaHoaDonNhapViewModel
{
    public int Id { get; set; }
    public int PhienBan { get; set; }
    public List<ChiSoNhapInput> ChiSo { get; set; } = [];
    public List<KhoanNhapInput> Khoan { get; set; } = [];
}
public sealed class ChiSoNhapInput
{
    public int Id { get; set; }
    public decimal ChiSoDau { get; set; }
    public decimal ChiSoCuoi { get; set; }
}
public sealed class KhoanNhapInput
{
    public string LoaiKhoan { get; set; } = "PHAT_SINH";
    public string? TenKhoan { get; set; } = "";
    public long? SoTien { get; set; }
    public string? GhiChu { get; set; } = "";
}
