using QL_PhongTro.Models;

namespace QL_PhongTro.ViewModels;

public sealed class ChiTietHoaDonViewModel
{
    public required HoaDon HoaDon { get; init; }
    public int ToaNhaId { get; init; }
    public string MaPhong { get; init; } = "";
    public string TenToaNha { get; init; } = "";
    public string TenKhach { get; init; } = "";
    public bool XemTruoc { get; init; }
}
