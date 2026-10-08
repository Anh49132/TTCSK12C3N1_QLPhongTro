using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Data;
using QL_PhongTro.Models;
using QL_PhongTro.Services;
using QL_PhongTro.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

if (args.Length == 3 && args[0] == "--prepare-demo") { await PrepareDemo.Run(args[1], args[2]); return; }
var root = Path.GetFullPath(args.Length > 0 ? args[0] : ".");
var folder = Path.Combine(Path.GetTempPath(), "s308-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(folder);
var path = Path.Combine(folder, "test.sqlite");
LocalDatabaseInitializer.Create(path, Path.Combine(root, "QL_PhongTro/Data/permissions.seed.json"));
RentalRequestSchema.Initialize(path);
DichVuSchemaInitializer.InitializeInvoices(path);
AppDbContext Context() => new(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(
    new SqliteConnectionStringBuilder { DataSource = path, ForeignKeys = true, Pooling = false }.ToString()).Options,
    new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(
        [new Claim(ClaimTypes.NameIdentifier, "1"), new Claim(ClaimTypes.Role, "CHU_NHA")], "test")) } });
void Check(bool condition, string name) { if (!condition) throw new Exception(name); Console.WriteLine("PASS: " + name); }
using (var setup = Context()) {
    await setup.Database.ExecuteSqlRawAsync("""
        INSERT INTO tai_khoan(id,ho_ten,email,so_dien_thoai,mat_khau,vai_tro,dang_hoat_dong,email_confirmed,ngay_tao,ngay_cap_nhat)
        VALUES(1,'Owner','owner@example.test','0900000001','unused','CHU_NHA',1,1,'2026-01-01','2026-01-01'),
              (2,'Other','other@example.test','0900000002','unused','CHU_NHA',1,1,'2026-01-01','2026-01-01'),
              (3,'Manager','manager@example.test','0900000003','unused','QUAN_LY',1,1,'2026-01-01','2026-01-01'),
              (4,'Tenant','tenant@example.test','0900000004','unused','KHACH_THUE',1,1,'2026-01-01','2026-01-01'),
              (5,'Admin','admin@example.test','0900000005','unused','ADMIN',1,1,'2026-01-01','2026-01-01');
        INSERT INTO toa_nha(id,chu_nha_id,ten_toa_nha,dia_chi) VALUES(1,1,'Synthetic building','Test');
        INSERT INTO phong_tro(id,toa_nha_id,ma_phong,tang,dien_tich,gia_thue,tien_coc_du_kien,so_nguoi_toi_da,trang_thai,ngay_tao)
        VALUES(1,1,'TEST',1,20,1000000,1000000,3,'DANG_THUE','2026-01-01');
        INSERT INTO khach_thue(id,ho_ten,ngay_tao) VALUES(1,'Synthetic tenant','2026-01-01');
        INSERT INTO hop_dong(id,ma_hop_dong,phong_id,khach_dung_ten_id,trang_thai,nguoi_lap_id,ngay_tao)
        VALUES(1,'HD-TEST',1,1,'DANG_HIEU_LUC',1,'2026-01-01');
        INSERT INTO ky_hop_dong(hop_dong_id,so_thu_tu,ngay_bat_dau,ngay_ket_thuc,so_thang,gia_thue,nguoi_lap_id,ngay_tao)
        VALUES(1,1,'2026-01-01','2028-12-31',36,1000000,1,'2026-01-01');
        """);
    var hash = BCrypt.Net.BCrypt.HashPassword("S308-Test-a1", 4);
    await setup.Database.ExecuteSqlInterpolatedAsync($"UPDATE tai_khoan SET mat_khau={hash}");
}
foreach (var (quantity, price, expected) in new (decimal, long, long)[] {
    (0, 3500, 0), (10, 3500, 35000), (.001m, 3500, 4), (.003m, 3500, 11),
    (1.234m, 3500, 4319), (12.345m, 3500, 43208), (2, 20000, 40000),
    (1, 1000000, 1000000), (.49m, 1, 0), (.5m, 1, 1) })
    Check(HoaDonDichVuService.ThanhTien(quantity, price) == expected, $"round {quantity} x {price} = {expected}");
int id;
using (var db = Context()) {
    id = await new HoaDonDichVuService(db, new DichVuService(db)).PhatHanhAsync(1,
        new LapHoaDonDichVuViewModel { ToaNhaId = 1, HopDongId = 1, NgayApDung = new(2026, 10, 1) }, taoNhap: true);
    var invoice = await db.HoaDons.Include(x => x.ChiTiet).SingleAsync(x => x.Id == id);
    Check(invoice.TrangThai == "NHAP" && invoice.NgayPhatHanh == null && invoice.NguoiPhatHanhId == null, "new invoice remains draft");
    invoice.ChiTiet.Add(new ChiTietHoaDon { SoThuTu = 2, TenKhoan = "Synthetic meter", CachTinhApDung = "THEO_CHI_SO",
        ChiSoDau = 100, ChiSoCuoi = 110, SoLuong = 10, DonGia = 3500, ThanhTien = 35000 });
    invoice.TongTien += 35000;
    await db.SaveChangesAsync();
}
async Task<SuaHoaDonNhapViewModel> Input() {
    using var db = Context(); var invoice = await db.HoaDons.Include(x => x.ChiTiet).SingleAsync(x => x.Id == id);
    return new() { Id = id, PhienBan = invoice.PhienBan,
        ChiSo = invoice.ChiTiet.Where(x => x.ChiSoDau.HasValue).Select(x => new ChiSoNhapInput { Id = x.Id, ChiSoDau = 100, ChiSoCuoi = 120 }).ToList() };
}
async Task Save(SuaHoaDonNhapViewModel model, int actor = 1) {
    using var db = Context(); await new HoaDonDichVuService(db, new DichVuService(db)).LuuNhapAsync(actor, model);
}
async Task Reject(SuaHoaDonNhapViewModel model, string name, int actor = 1) {
    using var before = Context(); var original = await before.HoaDons.AsNoTracking().SingleAsync(x => x.Id == id);
    var count = await before.ChiTietHoaDons.CountAsync(x => x.HoaDonId == id);
    try { await Save(model, actor); throw new Exception("Unexpected successful save: " + name); }
    catch (Exception ex) when (ex is InvalidOperationException or UnauthorizedAccessException or OverflowException) { }
    using var after = Context(); var current = await after.HoaDons.AsNoTracking().SingleAsync(x => x.Id == id);
    Check(current.TongTien == original.TongTien && current.PhienBan == original.PhienBan
        && count == await after.ChiTietHoaDons.CountAsync(x => x.HoaDonId == id), name + " rollback");
}
var edit = await Input();
edit.Khoan = [new() { LoaiKhoan = "PHAT_SINH", TenKhoan = "Repair", SoTien = 50000, GhiChu = "Agreed repair" },
    new() { LoaiKhoan = "GIAM_TRU", TenKhoan = "Discount", SoTien = 20000, GhiChu = "Agreed discount" }];
await Save(edit);
using (var db = Context()) {
    var invoice = await db.HoaDons.Include(x => x.ChiTiet).SingleAsync(x => x.Id == id);
    Check(invoice.TongTien == 1100000 && invoice.PhienBan == 1 && invoice.TrangThai == "NHAP", "meter + surcharge - discount total");
    Check(invoice.ChiTiet.Where(x => x.LoaiKhoan == "GIAM_TRU").Single().ThanhTien == 20000, "discount stored positive");
}
await Reject(edit, "stale version");
await Reject(await Input(), "other owner", 2);
foreach (var type in new[] { "PHAT_SINH", "GIAM_TRU" }) {
    var invalid = await Input(); invalid.Khoan = [new() { LoaiKhoan = type, TenKhoan = "Missing note", SoTien = 1000 }];
    await Reject(invalid, type + " missing note");
}
var negative = await Input(); negative.ChiSo[0].ChiSoCuoi = 99; await Reject(negative, "end below start");
var excessive = await Input(); excessive.Khoan = [new() { LoaiKhoan = "GIAM_TRU", TenKhoan = "Too much", SoTien = 2000000, GhiChu = "Test" }];
await Reject(excessive, "negative total");
var overflow = await Input(); overflow.Khoan = [new() { LoaiKhoan = "PHAT_SINH", TenKhoan = "Overflow", SoTien = long.MaxValue, GhiChu = "Test" }];
await Reject(overflow, "overflow");
using (var db = Context()) {
    Check(!await db.HoaDons.AnyAsync(x => x.NgayPhatHanh != null), "draft never published");
    await db.Database.ExecuteSqlRawAsync("UPDATE hoa_don SET trang_thai='DA_PHAT_HANH' WHERE id={0}", id);
}
await Reject(await Input(), "issued invoice immutable");
using (var c = new SqliteConnection("Data Source=" + path)) {
    c.Open(); using var command = c.CreateCommand(); command.CommandText = "PRAGMA integrity_check";
    Check((string?)command.ExecuteScalar() == "ok", "integrity"); command.CommandText = "PRAGMA foreign_key_check";
    using var reader = command.ExecuteReader(); Check(!reader.Read(), "foreign keys");
}
Console.WriteLine("All S3-08 checks passed on fresh temporary DB: " + path);

using (var db = Context()) {
    foreach (var code in new[] { "DIEN", "NUOC" }) {
        var service = new DichVu { MaDichVu = code, TenDichVu = code };
        var catalog = new DichVuToaNha { ToaNhaId = 1, DichVu = service, ApDungMacDinh = true };
        db.DichVuToaNhas.Add(catalog);
        db.CauHinhDichVus.Add(new() { ToaNhaId = 1, DichVu = service, CachTinh = "THEO_CHI_SO", DonViTinh = "unit",
            DonGia = 3500, TuNgay = new(2026, 1, 1), NguoiTaoId = 1, NgayTao = DateTime.UtcNow });
        db.DichVuPhongs.Add(new() { PhongId = 1, DichVuToaNha = catalog });
        await db.SaveChangesAsync();
        foreach (var date in new[] { new DateOnly(2026, 12, 1), new DateOnly(2027, 1, 1) })
            db.ChiSoDienNuocs.Add(new() { HopDongId = 1, DichVuId = service.Id, TuNgay = date, DenNgay = date.AddMonths(1).AddDays(-1),
                ChiSoDau = 100, ChiSoCuoi = 110, NguoiNhapId = 1, NgayNhap = DateTime.UtcNow });
        await db.SaveChangesAsync();
    }
    var svc = new HoaDonDichVuService(db, new DichVuService(db));
    var monthlyId = await svc.TaoNhapThangAsync(1, 1, 1, 2026, 12);
    Check((await db.HoaDons.FindAsync(monthlyId))!.TrangThai == "NHAP", "monthly single creates draft");
}
using (var db = Context()) {
    var draft = await db.HoaDons.Include(x => x.ChiTiet).SingleAsync(x => x.Thang == 12);
    await new HoaDonDichVuService(db, new DichVuService(db)).LuuNhapAsync(1, new() { Id = draft.Id, PhienBan = draft.PhienBan,
        ChiSo = draft.ChiTiet.Where(x => x.ChiSoDau.HasValue).Select(x => new ChiSoNhapInput { Id = x.Id, ChiSoDau = 100, ChiSoCuoi = 120 }).ToList() });
    Check(!await db.ChiSoDienNuocs.AnyAsync(x => x.ChiSoCuoi != 110 || x.DaKhoa), "editing draft preserves original meter readings");
}
using (var db = Context()) {
    var result = await new HoaDonDichVuService(db, new DichVuService(db)).PhatHanhDanhSachAsync(1, 1, 2027, 1, taoNhap: true);
    Check(result.SoNhap == 1 && result.SoDaPhatHanh == 0 && result.TongTien == 1070000, "monthly batch creates draft with correct total");
    Check(!await db.ChiSoDienNuocs.AnyAsync(x => x.DaKhoa), "draft creation leaves source meters unlocked");
}
using (var db = Context()) {
    var result = await new HoaDonDichVuService(db, new DichVuService(db)).PhatHanhDanhSachAsync(1, 1, 2027, 1, taoNhap: true);
    Check(result.SoNhap == 0 && result.SoDaCoHoaDon == 1, "monthly batch repeat does not duplicate draft");
}

// Exercise the real MVC binding, authorization and antiforgery on this same disposable fixture.
int webId;
using (var db = Context()) {
    webId = await new HoaDonDichVuService(db, new DichVuService(db)).PhatHanhAsync(1,
        new() { ToaNhaId = 1, HopDongId = 1, NgayApDung = new(2026, 11, 1) }, taoNhap: true);
}
var listener = new TcpListener(IPAddress.Loopback, 0); listener.Start();
var port = ((IPEndPoint)listener.LocalEndpoint).Port; listener.Stop();
var url = "http://127.0.0.1:" + port;
var start = new ProcessStartInfo("dotnet") { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
foreach (var arg in new[] { Path.Combine(AppContext.BaseDirectory, "QL_PhongTro.dll"), "--contentRoot", Path.Combine(root, "QL_PhongTro"),
    "--environment", "Development", "--urls", url, "--DatabasePath", path, "--DataProtectionKeysPath", Path.Combine(folder, "keys"),
    "--PasswordReset:PickupDirectory", Path.Combine(folder, "mail"), "--Logging:EventLog:LogLevel:Default", "None", "--Logging:LogLevel:Default", "Warning" }) start.ArgumentList.Add(arg);
using var server = Process.Start(start)!;
var output = server.StandardOutput.ReadToEndAsync(); var errors = server.StandardError.ReadToEndAsync();
HttpClient Client() => new(new HttpClientHandler { AllowAutoRedirect = false, CookieContainer = new CookieContainer() }) { BaseAddress = new Uri(url) };
string Token(string page) => WebUtility.HtmlDecode(Regex.Match(page, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
async Task Login(HttpClient client, string email) {
    var page = await client.GetStringAsync("/Account/Login");
    var result = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string> {
        ["TaiKhoanDangNhap"] = email, ["MatKhau"] = "S308-Test-a1", ["__RequestVerificationToken"] = Token(page) }));
    Check(result.StatusCode == HttpStatusCode.Redirect, "HTTP login " + email);
}
try {
    using var owner = Client();
    var ready = false;
    for (var attempt = 0; attempt < 100; attempt++) {
        try { if ((await owner.GetAsync("/Account/Login")).IsSuccessStatusCode) { ready = true; break; } } catch (HttpRequestException) { }
        if (server.HasExited) throw new Exception(await errors);
        await Task.Delay(100);
    }
    Check(ready, "HTTP server ready");
    await Login(owner, "owner@example.test");
    var page = await owner.GetStringAsync("/HoaDonDichVu/Details/" + webId);
    Check(page.Contains("Sửa hóa đơn Nháp"), "draft editor rendered");
    var fields = new Dictionary<string, string> { ["Id"] = webId.ToString(), ["PhienBan"] = "0",
        ["Khoan[0].LoaiKhoan"] = "PHAT_SINH", ["Khoan[0].TenKhoan"] = "HTTP charge", ["Khoan[0].SoTien"] = "10000", ["Khoan[0].GhiChu"] = "HTTP note",
        ["Khoan[1].LoaiKhoan"] = "GIAM_TRU", ["Khoan[1].TenKhoan"] = "", ["Khoan[1].SoTien"] = "", ["Khoan[1].GhiChu"] = "",
        ["__RequestVerificationToken"] = Token(page) };
    var saved = await owner.PostAsync("/HoaDonDichVu/SaveDraft", new FormUrlEncodedContent(fields));
    Check(saved.StatusCode == HttpStatusCode.Redirect, "HTTP save redirect");
    using (var db = Context()) Check((await db.HoaDons.FindAsync(webId))!.TongTien == 1010000, "HTTP binding accepts empty optional adjustment");
    fields["PhienBan"] = "1"; fields["Khoan[0].GhiChu"] = "";
    var missingNote = await owner.PostAsync("/HoaDonDichVu/SaveDraft", new FormUrlEncodedContent(fields));
    using (var db = Context()) Check((await db.HoaDons.FindAsync(webId))!.PhienBan == 1, "HTTP missing note not saved");
    fields.Remove("__RequestVerificationToken");
    var csrf = await owner.PostAsync("/HoaDonDichVu/SaveDraft", new FormUrlEncodedContent(fields));
    Check(csrf.StatusCode == HttpStatusCode.BadRequest, "HTTP missing CSRF rejected");
    using var other = Client(); await Login(other, "other@example.test");
    Check((await other.GetAsync("/HoaDonDichVu/Details/" + webId)).StatusCode == HttpStatusCode.Forbidden, "HTTP other owner cannot view");
    var home = await other.GetStringAsync("/"); fields["__RequestVerificationToken"] = Token(home);
    Check((await other.PostAsync("/HoaDonDichVu/SaveDraft", new FormUrlEncodedContent(fields))).StatusCode == HttpStatusCode.Forbidden, "HTTP other owner cannot save");
    foreach (var email in new[] { "manager@example.test", "tenant@example.test", "admin@example.test" }) {
        using var denied = Client(); await Login(denied, email);
        Check((await denied.PostAsync("/HoaDonDichVu/SaveDraft", new FormUrlEncodedContent(fields))).StatusCode == HttpStatusCode.Forbidden,
            "HTTP write forbidden for " + email);
    }
    Check(!Directory.Exists(Path.Combine(folder, "mail")) || !Directory.EnumerateFiles(Path.Combine(folder, "mail"), "*", SearchOption.AllDirectories).Any(), "draft sends no email");
    Console.WriteLine("All S3-08 HTTP checks passed.");
} finally {
    if (!server.HasExited) server.Kill(entireProcessTree: true);
    await server.WaitForExitAsync();
}
