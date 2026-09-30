using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using QL_PhongTro.Models;

namespace QL_PhongTro.ViewModels;

public sealed class YeuCauThueIndexViewModel
{
    public string? TrangThai { get; set; }
    public string? Loai { get; set; }
    public string? ToaNhaId { get; set; }
    public bool LaChuNha { get; set; }
    public IReadOnlyList<SelectListItem> ToaNhas { get; set; } = [];
    public IReadOnlyList<SelectListItem> TrangThaiOptions { get; set; } = [];
    public IReadOnlyList<SelectListItem> LoaiOptions { get; set; } = [];
    public IReadOnlyList<YeuCauThueListItem> YeuCaus { get; set; } = [];
}

public sealed class YeuCauThueListItem
{
    public int Id { get; set; }
    public string MaYeuCau { get; set; } = string.Empty;
    public string TenPhong { get; set; } = string.Empty;
    public string TenToaNha { get; set; } = string.Empty;
    public string TenKhach { get; set; } = string.Empty;
    public string SoDienThoaiKhach { get; set; } = string.Empty;
    public string LoaiYeuCau { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public DateTime? LichHen { get; set; }
    public DateTime NgayTao { get; set; }
    public bool DaDoiLich { get; set; }
}

public sealed class YeuCauThueDetailViewModel
{
    public YeuCauThue YeuCau { get; set; } = new();
    public string TenPhong { get; set; } = string.Empty;
    public string TenToaNha { get; set; } = string.Empty;
    public string DiaChiToaNha { get; set; } = string.Empty;
    public string TenKhach { get; set; } = string.Empty;
    public string SoDienThoaiKhach { get; set; } = string.Empty;
    public string TenChuNha { get; set; } = string.Empty;
    public bool LaChuNha { get; set; }

    // Filled only for a landlord on a request still waiting for a decision.
    public bool HienThiFormXacNhan { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? LichHenMoi { get; set; }

    public IReadOnlyList<SelectListItem> LyDoOptions { get; set; } = [];
}

public sealed class ThongBaoIndexViewModel
{
    public IReadOnlyList<ThongBao> DanhSach { get; set; } = [];
}
