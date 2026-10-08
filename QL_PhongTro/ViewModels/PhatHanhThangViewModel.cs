using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using QL_PhongTro.Models;

namespace QL_PhongTro.ViewModels;

public sealed class PhatHanhThangViewModel
{
    public int ToaNhaId { get; set; }
    public int Thang { get; set; }
    public int Nam { get; set; }
    [ValidateNever] public bool SanSang { get; set; }
    [ValidateNever] public List<SelectListItem> ToaNhas { get; set; } = [];
    [ValidateNever] public int TongPhong { get; set; }
    [ValidateNever] public List<HoaDonThangDuKien> DuKien { get; set; } = [];
    [ValidateNever] public List<HoaDonThangDuKien> DaPhatHanh { get; set; } = [];
}

public sealed record HoaDonThangDuKien(string MaPhong, HoaDon HoaDon, List<ChiSoDienNuoc> ChiSo);
