namespace QL_PhongTro.ViewModels;

// This DTO contains only the identity number the viewer is allowed to receive.
public class XemHoSoViewModel
{
    public int Id { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string CanCuocHienThi { get; set; } = string.Empty;
    public bool XemDayDu { get; set; }
    public bool LaChuHoSo { get; set; }
    public DateOnly? NgaySinh { get; set; }
    public string? QueQuan { get; set; }
    public string? NgheNghiep { get; set; }
    public bool CoAnhMatTruoc { get; set; }
    public bool CoAnhMatSau { get; set; }
}
