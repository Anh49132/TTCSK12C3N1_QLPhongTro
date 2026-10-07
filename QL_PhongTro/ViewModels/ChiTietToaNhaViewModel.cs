using QL_PhongTro.Models;
namespace QL_PhongTro.ViewModels;
public class ChiTietToaNhaViewModel
{
    public ToaNha ToaNha { get; set; } = null!;
    public string? QuanLy { get; set; }
    public string? Anh { get; set; }
    public int TongPhong { get; set; }
    public int DangThue { get; set; }
    public int Trong { get; set; }
    public int DatCoc { get; set; }
    public decimal? DienTichMin { get; set; }
    public decimal? DienTichMax { get; set; }
    public List<int> Tangs { get; set; } = [];
    public List<PhongChiTietToaNhaRow> Phongs { get; set; } = [];
    public string? TuKhoa { get; set; }
    public int? Tang { get; set; }
    public string? TrangThai { get; set; }
    public int Trang { get; set; } = 1;
    public int TongKetQua { get; set; }
    public int TongTrang => Math.Max(1, (TongKetQua + 7) / 8);
}
public record PhongChiTietToaNhaRow(PhongTro Phong, string? Khach, int? HopDongId, string? MaHopDong, DateOnly? HanHopDong);
