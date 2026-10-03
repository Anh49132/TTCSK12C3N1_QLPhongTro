using System.ComponentModel.DataAnnotations;
using QL_PhongTro.Services;

namespace QL_PhongTro.ViewModels;

/// <summary>
/// Backs the single detail page of S2-08. There is no list view model on purpose:
/// the landlord request list belongs to S2-07, and S2-09 may add its own tenant list.
/// </summary>
public sealed class LichHenChiTietViewModel
{
    public required LichHenYeuCau YeuCau { get; init; }

    public bool LaChuNha { get; init; }

    /// <summary>Only a landlord sees the confirm form, and only while the request still waits.</summary>
    public bool HienThiFormXacNhan { get; init; }

    /// <summary>
    /// A landlord may also walk away from a slot they already confirmed, so the reject form
    /// stays visible in DA_HEN_LICH and disappears once the request is closed.
    /// </summary>
    public bool HienThiFormTuChoi { get; init; }

    /// <summary>Wall-clock time the landlord typed, kept in Vietnam time for the input control.</summary>
    [DataType(DataType.DateTime)]
    public DateTime? LichHenNhap { get; set; }

    [Required(ErrorMessage = "Hãy chọn lý do từ chối.")]
    [StringLength(30, ErrorMessage = "Lý do từ chối không hợp lệ.")]
    public string? LyDoTuChoi { get; set; }

    [StringLength(LichHenService.GhiChuToiDa, ErrorMessage = "Ghi chú không được dài quá 500 ký tự.")]
    public string? GhiChuTuChoi { get; set; }
}
