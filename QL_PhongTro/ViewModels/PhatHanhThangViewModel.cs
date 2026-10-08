using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using QL_PhongTro.Models;

namespace QL_PhongTro.ViewModels;

public sealed class PhatHanhThangViewModel
{
    public int ToaNhaId { get; set; }
    public int Thang { get; set; }
    public int Nam { get; set; }
    public DateOnly NgayPhatHanh { get; set; }
    public DateOnly HanThanhToan { get; set; }
    [ValidateNever] public string ReviewToken { get; set; } = "";
    [ValidateNever] public bool SanSang { get; set; }
    [ValidateNever] public List<SelectListItem> ToaNhas { get; set; } = [];
    [ValidateNever] public int TongPhong { get; set; }
    [ValidateNever] public List<HoaDonThangDuKien> DuKien { get; set; } = [];
    [ValidateNever] public List<HoaDonThangDuKien> DaPhatHanh { get; set; } = [];
    [ValidateNever] public List<PhongHoaDonBoQua> BoQua { get; set; } = [];
    [ValidateNever] public List<KetQuaPhongHoaDon> KetQua { get; set; } = [];
}

public sealed record HoaDonThangDuKien(string MaPhong, HoaDon HoaDon, List<ChiSoDienNuoc> ChiSo)
{
    public int Tang { get; init; }
    public string TenKhach { get; init; } = "";
    public int PhienBanPhong { get; init; }
}
public sealed record PhongHoaDonBoQua(int HopDongId, string MaPhong, string LyDo);
public sealed record KetQuaPhongHoaDon(int HopDongId, string MaPhong, string TrangThai, string LyDo, int? HoaDonId = null);
public sealed record KetQuaPhatHanhThang(List<KetQuaPhongHoaDon> Phongs)
{
    public int SoDaPhatHanh => Phongs.Count(x => x.TrangThai == "DA_PHAT_HANH");
}
