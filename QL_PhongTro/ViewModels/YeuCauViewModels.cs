using Microsoft.AspNetCore.Mvc.Rendering;
namespace QL_PhongTro.ViewModels;
public class YeuCauListItemViewModel
{
    public string MaYeuCau { get; set; } = ""; public string TenKhach { get; set; } = ""; public string SoDienThoai { get; set; } = "";
    public string MaPhong { get; set; } = ""; public string LoaiYeuCau { get; set; } = ""; public DateOnly? NgayMongMuon { get; set; }
    public string TrangThai { get; set; } = ""; public DateTime NgayTao { get; set; }
}
public class DanhSachYeuCauViewModel
{
    public IReadOnlyList<YeuCauListItemViewModel> YeuCaus { get; set; } = [];
    public string? TrangThai { get; set; }
    public int? ToaNhaId { get; set; }
    public IReadOnlyList<SelectListItem> ToaNhaOptions { get; set; } = [];
}
