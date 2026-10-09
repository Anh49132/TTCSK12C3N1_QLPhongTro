namespace QL_PhongTro.ViewModels;

public sealed class HomeViewModel
{
    public bool RegistrationEnabled { get; set; }
    public List<string> QuanHuyens { get; set; } = [];
    public List<HomeTinDangItemViewModel> TinMoiNhat { get; set; } = [];
    public HomeDashboardViewModel Dashboard { get; set; } = new();
}

public sealed class HomeDashboardViewModel
{
    public List<HomeRoomItem> Rooms { get; set; } = [];
    public List<HomeRequestItem> Requests { get; set; } = [];
    public List<HomeRoleItem> Roles { get; set; } = [];
    public List<QL_PhongTro.Models.NhatKyHoatDong> Activities { get; set; } = [];
    public int TongTaiKhoan { get; set; }
    public int TongChuNha { get; set; }
    public int TongToaNha { get; set; }
    public int TongPhong { get; set; }
    public int PhongTrong { get; set; }
    public int PhongDangThue { get; set; }
    public int YeuCauChoXuLy { get; set; }
    public int YeuCauCuaToi { get; set; }
    public int PhongDatCoc { get; set; }
    public List<HomeBuildingItem> Buildings { get; set; } = [];
    public List<HomeMeterAlert> MeterAlerts { get; set; } = [];
}

public sealed record HomeRoomItem(int Id, string Code, string Building, string Status, long Rent);
public sealed record HomeRequestItem(int Id, string Code, string Room, string Name, string Status, DateTime Created);
public sealed record HomeRoleItem(string Name, int Count);
public sealed record HomeBuildingItem(int Id, string Name, string Address, int Rooms, int Occupied);
public sealed record HomeMeterAlert(int ToaNhaId, string TenToaNha, int Nam, int Thang, int SoPhongConThieu);

public sealed class HomeTinDangItemViewModel
{
    public int Id { get; init; }
    public string TieuDe { get; init; } = string.Empty;
    public long GiaThue { get; init; }
    public decimal DienTich { get; init; }
    public int SoNguoiToiDa { get; init; }
    public string? TenToaNha { get; init; }
    public string? PhuongXa { get; init; }
    public string? QuanHuyen { get; init; }
    public string? DiaChi { get; init; }
    public string? AnhDaiDien { get; init; }
}
