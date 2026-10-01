namespace QL_PhongTro.ViewModels;

public class TimTinViewModel
{
    public string? QuanHuyen { get; set; }
    public List<string> QuanHuyens { get; set; } = [];
    public List<TinTimKiem> TinDangs { get; set; } = [];
    public bool SchemaReady { get; set; }
}

public class TinTimKiem
{
    public string TieuDe { get; set; } = "";
    public string DiaChi { get; set; } = "";
    public string? QuanHuyen { get; set; }
    public long GiaThue { get; set; }
    public decimal DienTich { get; set; }
}
