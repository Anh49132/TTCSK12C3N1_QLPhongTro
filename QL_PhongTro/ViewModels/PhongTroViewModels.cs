using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using QL_PhongTro.Models;

namespace QL_PhongTro.ViewModels;

public enum TrangThaiPhong
{
    [Display(Name = "Trống")]
    TRONG,

    [Display(Name = "Đã đặt cọc")]
    DA_DAT_COC,

    [Display(Name = "Đang thuê")]
    DANG_THUE,

    [Display(Name = "Ngừng cho thuê")]
    NGUNG_CHO_THUE
}

public class DanhSachPhongViewModel
{
    public int? ToaNhaId { get; set; }
    public string? TuKhoa { get; set; }
    public TrangThaiPhong? TrangThaiFilter { get; set; }
    public IReadOnlyList<SelectListItem> ToaNhaOptions { get; set; } = [];
    public IReadOnlyList<SelectListItem> TrangThaiOptions { get; set; } = [];
    public IReadOnlyDictionary<string, int> SoLuongTheoTrangThai { get; set; } = new Dictionary<string, int>();
    public IReadOnlyList<PhongTro> PhongTros { get; set; } = [];
}

public class TaoPhongViewModel
{
    [Required(ErrorMessage = "Vui lòng chọn tòa nhà.")]
    [Display(Name = "Tòa nhà")]
    public int? ToaNhaId { get; set; }

    [Required(ErrorMessage = "Mã phòng là bắt buộc.")]
    [StringLength(20, ErrorMessage = "Mã phòng không vượt quá 20 ký tự.")]
    [Display(Name = "Mã phòng")]
    public string MaPhong { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tầng là bắt buộc.")]
    [Display(Name = "Tầng")]
    public int? Tang { get; set; }

    [Required(ErrorMessage = "Diện tích là bắt buộc.")]
    [Range(0.01, 999999.99, ErrorMessage = "Diện tích phải lớn hơn 0 và tối đa 999999,99 m².")]
    [Display(Name = "Diện tích (m²)")]
    public decimal? DienTich { get; set; }

    [Required(ErrorMessage = "Giá thuê là bắt buộc.")]
    [Range(500000, long.MaxValue, ErrorMessage = "Giá thuê tối thiểu là 500.000 VND.")]
    [Display(Name = "Giá thuê mỗi tháng (VND)")]
    public long? GiaThue { get; set; }

    [Display(Name = "Giá thuê mỗi tháng (VND)")]
    public string? GiaThueDisplay { get; set; }

    [Required(ErrorMessage = "Số người ở tối đa là bắt buộc.")]
    [Range(1, int.MaxValue, ErrorMessage = "Số người ở tối đa phải lớn hơn 0.")]
    [Display(Name = "Số người ở tối đa")]
    public int? SoNguoiToiDa { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn trạng thái.")]
    [EnumDataType(typeof(TrangThaiPhong), ErrorMessage = "Trạng thái phòng không hợp lệ.")]
    [Display(Name = "Trạng thái")]
    public TrangThaiPhong? TrangThai { get; set; }

    public IReadOnlyList<SelectListItem> ToaNhaOptions { get; set; } = [];
    public IReadOnlyList<AnhPhongQuanLyViewModel> Anh { get; set; } = [];
}

public class TaoPhongHangLoatViewModel
{
    [Required(ErrorMessage = "Vui lòng chọn tòa nhà.")]
    [Display(Name = "Tòa nhà")]
    public int? ToaNhaId { get; set; }

    [Required(ErrorMessage = "Số tầng là bắt buộc.")]
    [Range(1, 99, ErrorMessage = "Số tầng phải từ 1 đến 99.")]
    [Display(Name = "Số tầng")]
    public int? SoTang { get; set; }

    [Required(ErrorMessage = "Số phòng mỗi tầng là bắt buộc.")]
    [Range(1, 99, ErrorMessage = "Số phòng mỗi tầng phải từ 1 đến 99.")]
    [Display(Name = "Số phòng mỗi tầng")]
    public int? SoPhongMoiTang { get; set; }

    [Required(ErrorMessage = "Diện tích là bắt buộc.")]
    [Range(0.01, 999999.99, ErrorMessage = "Diện tích phải lớn hơn 0 và tối đa 999999,99 m².")]
    [Display(Name = "Diện tích (m²)")]
    public decimal? DienTich { get; set; }

    [Required(ErrorMessage = "Giá thuê là bắt buộc.")]
    [Range(500000, long.MaxValue, ErrorMessage = "Giá thuê tối thiểu là 500.000 VND.")]
    [Display(Name = "Giá thuê mỗi tháng (VND)")]
    public long? GiaThue { get; set; }

    [Display(Name = "Giá thuê mỗi tháng (VND)")]
    public string? GiaThueDisplay { get; set; }

    [Required(ErrorMessage = "Số người ở tối đa là bắt buộc.")]
    [Range(1, int.MaxValue, ErrorMessage = "Số người ở tối đa phải lớn hơn 0.")]
    [Display(Name = "Số người ở tối đa")]
    public int? SoNguoiToiDa { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn trạng thái.")]
    [EnumDataType(typeof(TrangThaiPhong), ErrorMessage = "Trạng thái phòng không hợp lệ.")]
    [Display(Name = "Trạng thái ban đầu")]
    public TrangThaiPhong? TrangThai { get; set; }

    public IReadOnlyList<SelectListItem> ToaNhaOptions { get; set; } = [];
}

public class TaoToaNhaViewModel
{
    [Required(ErrorMessage = "Tên tòa nhà là bắt buộc.")]
    [StringLength(150)]
    [Display(Name = "Tên tòa nhà")]
    public string TenToaNha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Địa chỉ là bắt buộc.")]
    [Display(Name = "Địa chỉ")]
    public string DiaChi { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Phường/xã")]
    public string? PhuongXa { get; set; }

    [StringLength(100)]
    [Display(Name = "Quận/huyện")]
    public string? QuanHuyen { get; set; }

    [StringLength(100)]
    [Display(Name = "Tỉnh/thành phố")]
    public string? TinhThanh { get; set; }

    [Range(1, 99, ErrorMessage = "Số tầng phải từ 1 đến 99.")]
    [Display(Name = "Số tầng")]
    public int? SoTang { get; set; }

    [Display(Name = "Người quản lý")]
    public int? QuanLyId { get; set; }

    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }

    public IReadOnlyList<SelectListItem> QuanLyOptions { get; set; } = [];
}

public class DanhSachToaNhaViewModel
{
    public string? TuKhoa { get; set; }
    public IReadOnlyList<ToaNhaTongHopViewModel> ToaNhas { get; set; } = [];
}

public class ToaNhaTongHopViewModel
{
    public int Id { get; set; }
    public string TenToaNha { get; set; } = string.Empty;
    public string DiaChi { get; set; } = string.Empty;
    public string? QuanLy { get; set; }
    public int SoPhong { get; set; }
    public int SoPhongTrong { get; set; }
    public bool DangHoatDong { get; set; }
}
