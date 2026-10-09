namespace QL_PhongTro.ViewModels;

public sealed record ChiTietHoaDonKhachPageViewModel(string MaHoaDon);

public sealed record ChiTietHoaDonKhachViewModel(
    string MaHoaDon,
    int Thang,
    int Nam,
    string MaPhong,
    IReadOnlyList<ChiTietHoaDonKhachLineResponse> ChiTiet);

public sealed record ChiTietHoaDonKhachLineViewModel(
    string TenKhoan,
    decimal? ChiSoDau,
    decimal? ChiSoCuoi,
    decimal SoLuong,
    string? DonViTinh,
    string LoaiKhoan,
    long DonGia,
    long ThanhTien);

public sealed record ChiTietHoaDonKhachLineResponse(
    string TenKhoan,
    string? ChiSoDau,
    string? ChiSoCuoi,
    string SoLuongTieuThu,
    string DonViTinh,
    string DonGia,
    string ThanhTien);
