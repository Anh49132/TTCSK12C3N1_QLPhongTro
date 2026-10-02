namespace QL_PhongTro.Models;

// yeu_cau_thue belongs to S2-06 and its trang_thai column carries no CHECK constraint.
// S2-08 therefore treats this list as the single definition of the values it accepts,
// and re-checks the current state in LichHenService before every write.
public static class LichHenTrangThai
{
    public const string Moi = "MOI";
    public const string DaHenLich = "DA_HEN_LICH";
    public const string DaDuyet = "DA_DUYET";
    public const string TuChoi = "TU_CHOI";
    public const string DaHuy = "DA_HUY";

    public const string LoaiXemPhong = "XEM_PHONG";
    public const string LoaiThueNgay = "THUE_NGAY";

    public static string Label(string trangThai) => trangThai switch
    {
        Moi => "Chờ xác nhận",
        DaHenLich => "Đã xác nhận lịch hẹn",
        DaDuyet => "Đã duyệt",
        TuChoi => "Đã từ chối",
        DaHuy => "Đã huỷ",
        _ => trangThai
    };

    public static string LoaiLabel(string loai) => loai switch
    {
        LoaiXemPhong => "Xem phòng",
        LoaiThueNgay => "Thuê ngay",
        _ => loai
    };
}
