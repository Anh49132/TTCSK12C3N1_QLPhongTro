using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using QL_PhongTro.Models;
using QL_PhongTro.Services;

namespace QL_PhongTro.ViewModels;

public class DongDichVuInput
{
    public int DichVuId { get; set; }
    public bool Chon { get; set; }
    public decimal? ChiSoDau { get; set; }
    public decimal? ChiSoCuoi { get; set; }
    // Optimistic check against the price actually displayed to the user.
    public int CauHinhId { get; set; }
    public long DonGiaDaXem { get; set; }
}
public class HoaDonGanDayViewModel
{
    public int Id { get; set; }
    public string MaHoaDon { get; set; } = "";
    public string MaPhong { get; set; } = "";
    public string TenToaNha { get; set; } = "";
    public int Thang { get; set; }
    public int Nam { get; set; }
    public DateOnly NgayChot { get; set; }
    public long TongTien { get; set; }
}

public class LapHoaDonDichVuViewModel
{
    public int ToaNhaId { get; set; }
    [Required(ErrorMessage = "Hãy chọn hợp đồng.")] public int? HopDongId { get; set; }
    [Required(ErrorMessage = "Hãy chọn ngày áp dụng đơn giá."), DataType(DataType.Date)] public DateOnly? NgayApDung { get; set; }
    [Range(1, 10000, ErrorMessage = "Số người phải lớn hơn 0.")] public int SoNguoi { get; set; } = 1;
    public List<DongDichVuInput> Dong { get; set; } = [];
    [ValidateNever] public List<SelectListItem> HopDongs { get; set; } = [];
    [ValidateNever] public List<DonGiaDichVu> DonGias { get; set; } = [];
    [ValidateNever] public List<HoaDonGanDayViewModel> DaPhatHanh { get; set; } = [];
    [ValidateNever] public bool SanSang { get; set; }
}
