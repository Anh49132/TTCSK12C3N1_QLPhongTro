namespace QL_PhongTro.ViewModels;

public sealed class HomeViewModel
{
    public bool RegistrationEnabled { get; set; }
    public List<string> QuanHuyens { get; set; } = [];
    public List<HomeTinDangItemViewModel> TinMoiNhat { get; set; } = [];
}

public sealed class HomeTinDangItemViewModel
{
    public int Id { get; init; }
    public string TieuDe { get; init; } = string.Empty;
    public long GiaThue { get; init; }
    public decimal DienTich { get; init; }
    public int SoNguoiToiDa { get; init; }
    public string? TenToaNha { get; init; }
    public string? PhuongXa { get; init; }
    public string? QuanHuyen { get; init; }
    public string? DiaChi { get; init; }
    public string? AnhDaiDien { get; init; }
}
