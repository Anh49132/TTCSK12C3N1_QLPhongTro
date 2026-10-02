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
    public IReadOnlyList<DichVuTinChiTietViewModel> DichVuTheoSuDung { get; init; } = [];
    public IReadOnlyList<DichVuTinChiTietViewModel> KhoanCoDinh { get; init; } = [];
    public decimal TongChiPhiThangDau
    {
        get
        {
            var total = (decimal)GiaThue;
            foreach (var fee in KhoanCoDinh)
                total += fee.DonGia;
            return total;
        }
    }
}

public sealed class AnhPhongChiTietViewModel
{
    public string DuongDan { get; init; } = string.Empty;
    public string? DuongDanAnhNho { get; init; }
    public string? MoTa { get; init; }
}

public sealed class DichVuTinChiTietViewModel
{
    public string TenDichVu { get; init; } = string.Empty;
    public string CachTinh { get; init; } = string.Empty;
    public string DonViTinh { get; init; } = string.Empty;
    public long DonGia { get; init; }
}