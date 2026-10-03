namespace QL_PhongTro.ViewModels;

public sealed record YeuCauThueItemViewModel(
    int Id,
    string MaYeuCau,
    string MaPhong,
    DateTime NgayTao,
    string TrangThai,
    string TrangThaiHienThi,
    DateTime? LichHen,
    string? LyDoTuChoi);
