using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using QL_PhongTro.Models;

namespace QL_PhongTro.ViewModels;

public class CauHinhDienNuocViewModel
{
    public int ToaNhaId { get; set; }
    [ValidateNever] public string TenToaNha { get; set; } = "";
    public CauHinhTienDichVuViewModel Dien { get; set; } = new();
    public CauHinhTienDichVuViewModel Nuoc { get; set; } = new();
}

public class CauHinhTienDichVuViewModel : IValidatableObject
{
    public string CachTinh { get; set; } = CachTinhDichVu.TheoChiSo;
    public long? DonGiaChiSo { get; set; }
    public long? TienMotNguoi { get; set; }
    public string TruongGiaApDung => CachTinh == CachTinhDichVu.TheoChiSo ? nameof(DonGiaChiSo) : nameof(TienMotNguoi);
    public long? GiaApDung => CachTinh == CachTinhDichVu.TheoChiSo ? DonGiaChiSo : TienMotNguoi;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CachTinh is not (CachTinhDichVu.TheoChiSo or CachTinhDichVu.TheoNguoi))
            yield return new ValidationResult("Hãy chọn cách tính hợp lệ.", [nameof(CachTinh)]);
        else if (!GiaApDung.HasValue)
            yield return new ValidationResult("Hãy nhập mức giá cho cách tính đã chọn.",
                [TruongGiaApDung]);
        else if (GiaApDung <= 0)
            yield return new ValidationResult("Mức giá phải là số nguyên đồng lớn hơn 0.",
                [TruongGiaApDung]);
    }
}
