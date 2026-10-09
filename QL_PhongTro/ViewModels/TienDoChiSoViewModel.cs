using System.ComponentModel.DataAnnotations;

namespace QL_PhongTro.ViewModels;

public sealed class TienDoChiSoViewModel
{
    [Range(1900, 9998, ErrorMessage = "Năm theo dõi phải từ 1900 đến 9998.")]
    public int Nam { get; set; }

    [Range(1, 12, ErrorMessage = "Tháng theo dõi không hợp lệ.")]
    public int Thang { get; set; }

    public List<TienDoChiSoToaNha> ToaNhas { get; set; } = [];
}

public sealed record TienDoChiSoToaNha(
    int ToaNhaId,
    string TenToaNha,
    int TongPhongDangThue,
    int SoPhongDaChot,
    int SoPhongConThieu);
