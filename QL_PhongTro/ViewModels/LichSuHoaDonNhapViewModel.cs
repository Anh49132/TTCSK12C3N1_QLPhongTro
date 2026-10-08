namespace QL_PhongTro.ViewModels;

public sealed record SnapshotHoaDonNhap(long TongTien, List<DongLichSuHoaDon> Dong);
public sealed record DongLichSuHoaDon(int SoThuTu, string LoaiKhoan, string TenKhoan,
    decimal? ChiSoDau, decimal? ChiSoCuoi, decimal SoLuong, long DonGia, long ThanhTien, string? GhiChu);
public sealed record LichSuHoaDonNhapViewModel(long Id, string NguoiSua, DateTime ThoiDiem,
    SnapshotHoaDonNhap? Truoc, SnapshotHoaDonNhap? Sau);
