namespace QL_PhongTro.ViewModels;

public sealed class TinDangChiTietViewModel
{
    public int Id { get; init; }
    public string TieuDe { get; init; } = string.Empty;
    public string? MoTa { get; init; }
    public long GiaThue { get; init; }
    public decimal DienTich { get; init; }
    public int SoNguoiToiDa { get; init; }
    public long TienCocDuKien { get; init; }
    public string DiaChi { get; init; } = string.Empty;
    public string? PhuongXa { get; init; }
    public string? QuanHuyen { get; init; }
    public string? TinhThanh { get; init; }
    public IReadOnlyList<AnhPhongChiTietViewModel> Anh { get; init; } = [];
}

public sealed class AnhPhongChiTietViewModel
{
    public string DuongDan { get; init; } = string.Empty;
    public string? DuongDanAnhNho { get; init; }
    public string? MoTa { get; init; }
}