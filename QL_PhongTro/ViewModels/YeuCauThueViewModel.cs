namespace QL_PhongTro.ViewModels;

public sealed record YeuCauThueItemViewModel(
    int Id,
    string MaYeuCau,
    string ThongTinPhong,
    DateTime NgayTao,
    DateTime? LichHen,
    string TrangThai,
    string TrangThaiHienThi,
    bool CoTheHuy);
