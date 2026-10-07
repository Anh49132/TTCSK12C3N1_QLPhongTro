namespace QL_PhongTro.ViewModels;

public sealed record ToaNhaGhiChiSo(int Id, string Ten);
public sealed record ChiSoThamChieu(decimal? GiaTri, bool LaBanGiao = false);
public sealed record PhongGhiChiSo(int PhongId, int HopDongId, string MaPhong, int Tang,
    ChiSoThamChieu Dien, ChiSoThamChieu Nuoc, bool DaChot);

public sealed class ChiSoDienNuocViewModel
{
    public List<ToaNhaGhiChiSo> ToaNhas { get; set; } = [];
    public int? ToaNhaId { get; set; }
    public DateOnly DauKy { get; set; }
    public List<PhongGhiChiSo> Phongs { get; set; } = [];
}
