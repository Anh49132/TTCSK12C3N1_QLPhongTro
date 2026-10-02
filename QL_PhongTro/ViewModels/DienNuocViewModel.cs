using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using QL_PhongTro.Models;

namespace QL_PhongTro.ViewModels;

public class DienNuocViewModel
{
    public int ToaNhaId { get; set; }
    public string MaDichVu { get; set; } = "DIEN";
    public int PhienBan { get; set; }
    [Required(ErrorMessage = "Hãy chọn cách tính.")]
    public string? CachTinh { get; set; }
    [Required(ErrorMessage = "Hãy nhập đơn giá."), Range(typeof(long), "1", "9223372036854775807", ErrorMessage = "Đơn giá điện/nước phải lớn hơn 0.")]
    public long? DonGia { get; set; }
    [Required(ErrorMessage = "Kỳ áp dụng không hợp lệ.")]
    public DateOnly? KyApDung { get; set; }
    [ValidateNever] public List<CauHinhDichVu> LichSu { get; set; } = [];
}
