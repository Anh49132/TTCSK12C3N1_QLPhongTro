using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Data;
using QL_PhongTro.Models;
using QL_PhongTro.Services;

internal static class PrepareDemo
{
    public static async Task Run(string root, string database)
    {
        root = Path.GetFullPath(root); database = Path.GetFullPath(database);
        var allowed = Path.Combine(root, "data", "s308-demo") + Path.DirectorySeparatorChar;
        if (!database.StartsWith(allowed, StringComparison.OrdinalIgnoreCase) || !File.Exists(database))
            throw new InvalidOperationException("Only an existing copy under data/s308-demo is allowed.");
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite("Data Source=" + database).Options;
        using var read = new AppDbContext(options);
        var actor = await read.TaiKhoans.Where(x => x.Email == "owner.demo@demo.local").Select(x => x.Id).SingleAsync();
        using var db = new AppDbContext(options, new HttpContextAccessor { HttpContext = new DefaultHttpContext {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, actor.ToString()),
                new Claim(ClaimTypes.Role, "CHU_NHA")], "demo")) } });
        var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
        var first = new DateOnly(today.Year, today.Month, 1); var last = first.AddMonths(1).AddDays(-1);
        var contracts = await (from h in db.HopDongs join p in db.PhongTros on h.PhongId equals p.Id
            join b in db.ToaNhas on p.ToaNhaId equals b.Id
            where b.ChuNhaId == actor && h.TrangThai == "DANG_HIEU_LUC" select new { h.Id, p.ToaNhaId }).ToListAsync();
        var meters = await db.DichVus.Where(x => x.MaDichVu == "DIEN" || x.MaDichVu == "NUOC").ToListAsync();
        // Sprint 2 water is billed per person. This disposable copy needs a new meter-price period for the monthly preview.
        foreach (var meter in meters) {
            var configs = await db.CauHinhDichVus.Where(x => x.DichVuId == meter.Id && x.TuNgay < first
                && (x.DenNgay == null || x.DenNgay >= first) && x.CachTinh != "THEO_CHI_SO").ToListAsync();
            foreach (var old in configs) {
                var previousEnd = old.DenNgay; old.DenNgay = first.AddDays(-1);
                db.CauHinhDichVus.Add(new() { ToaNhaId = old.ToaNhaId, PhongId = old.PhongId, DichVuId = meter.Id,
                    CachTinh = "THEO_CHI_SO", DonViTinh = meter.MaDichVu == "DIEN" ? "kWh" : "m³", DonGia = old.DonGia,
                    TuNgay = first, DenNgay = previousEnd, NguoiTaoId = actor, NgayTao = DateTime.UtcNow });
            }
        }
        await db.SaveChangesAsync();
        foreach (var contract in contracts)
            foreach (var meter in meters)
                if (!await db.ChiSoDienNuocs.AnyAsync(x => x.HopDongId == contract.Id && x.DichVuId == meter.Id && x.TuNgay == first && x.DenNgay == last))
                    db.ChiSoDienNuocs.Add(new() { HopDongId = contract.Id, DichVuId = meter.Id, TuNgay = first, DenNgay = last,
                        ChiSoDau = meter.MaDichVu == "DIEN" ? 100 : 10, ChiSoCuoi = meter.MaDichVu == "DIEN" ? 120 : 12,
                        NguoiNhapId = actor, NgayNhap = DateTime.UtcNow, DaXacNhanBatThuong = true });
        await db.SaveChangesAsync();
        await PrepareTenantInvoiceAsync(db, actor, today);
        var existingDraftPath = Path.Combine(Path.GetDirectoryName(database)!, "draft-id.txt");
        if (File.Exists(existingDraftPath)
            && int.TryParse((await File.ReadAllTextAsync(existingDraftPath)).Trim(), out var existingDraftId)
            && await db.HoaDons.AnyAsync(x => x.Id == existingDraftId && x.TrangThai == "NHAP"))
        {
            Console.WriteLine($"Prepared draft already exists: {existingDraftId}");
            return;
        }
        var svc = new HoaDonDichVuService(db, new DichVuService(db));
        foreach (var building in contracts.Select(x => x.ToaNhaId).Distinct()) {
            var preview = await svc.XemThangAsync(actor, building, today.Year, today.Month);
            foreach (var skipped in preview.BoQua) Console.WriteLine(skipped.MaPhong + ": " + skipped.LyDo);
            if (preview.DuKien.Count == 0) continue;
            var id = await svc.TaoNhapThangAsync(actor, building, preview.DuKien[0].HoaDon.HopDongId, today.Year, today.Month);
            await File.WriteAllTextAsync(Path.Combine(Path.GetDirectoryName(database)!, "draft-id.txt"), id.ToString());
            Console.WriteLine("Prepared draft ID " + id + " on copy only: " + database);
            return;
        }
        throw new InvalidOperationException("No eligible invoice in the copied demo.");
    }

    private static async Task PrepareTenantInvoiceAsync(AppDbContext db, int ownerId, DateOnly today)
    {
        var directory = Path.GetDirectoryName(db.Database.GetDbConnection().DataSource)
            ?? throw new InvalidOperationException("Demo database has no parent directory.");
        var codePath = Path.Combine(directory, "tenant-invoice-code.txt");
        if (File.Exists(codePath))
        {
            var existingCode = (await File.ReadAllTextAsync(codePath)).Trim();
            if (existingCode.Length > 0 && await db.HoaDons.AnyAsync(x => x.MaHoaDon == existingCode && x.TrangThai == "DA_PHAT_HANH"))
            {
                Console.WriteLine("Tenant invoice demo already prepared: " + existingCode);
                return;
            }
        }

        var contracts = await (from contract in db.HopDongs.AsNoTracking()
            join room in db.PhongTros.AsNoTracking() on contract.PhongId equals room.Id
            join building in db.ToaNhas.AsNoTracking() on room.ToaNhaId equals building.Id
            join tenant in db.KhachThues.AsNoTracking() on contract.KhachDungTenId equals tenant.Id
            join account in db.TaiKhoans.AsNoTracking() on tenant.TaiKhoanId equals account.Id
            where building.ChuNhaId == ownerId && contract.TrangThai == "DANG_HIEU_LUC"
                && account.VaiTro == "KHACH_THUE" && account.DangHoatDong && !account.IsDeleted
            orderby room.MaPhong
            select new { contract.Id, room.MaPhong, room.ToaNhaId })
            .ToListAsync();
        var selected = default((int Id, string MaPhong, int ToaNhaId, KyHopDongThamChieu Term, DateOnly First, DateOnly Last)?);
        var previousMonth = new DateOnly(today.Year, today.Month, 1).AddMonths(-1);
        foreach (var contract in contracts)
        {
            var terms = await db.KyHopDongs.AsNoTracking().Where(x => x.HopDongId == contract.Id).OrderBy(x => x.NgayBatDau).ToListAsync();
            for (var offset = 0; offset < 24 && selected is null; offset++)
            {
                var first = previousMonth.AddMonths(-offset);
                var last = first.AddMonths(1).AddDays(-1);
                var term = terms.SingleOrDefault(x => x.NgayBatDau <= first && x.NgayKetThuc >= last);
                if (term is null || await db.HoaDons.AnyAsync(x => x.HopDongId == contract.Id && x.Nam == first.Year && x.Thang == first.Month))
                    continue;
                selected = (contract.Id, contract.MaPhong, contract.ToaNhaId, term, first, last);
            }
            if (selected is not null) break;
        }
        if (selected is not { } choice)
            throw new InvalidOperationException("No tenant contract has a free full-month period for the invoice demo.");

        var meterServices = await db.DichVus.AsNoTracking()
            .Where(x => x.MaDichVu == "DIEN" || x.MaDichVu == "NUOC")
            .ToDictionaryAsync(x => x.MaDichVu);
        if (!meterServices.TryGetValue("DIEN", out var electricity) || !meterServices.TryGetValue("NUOC", out var water))
            throw new InvalidOperationException("The demo copy must contain the electricity and water service catalog.");
        var monthCode = choice.First.ToString("yyyyMM");
        var lines = new List<ChiTietHoaDon>
        {
            new()
            {
                SoThuTu = 1, LoaiKhoan = "TIEN_PHONG", TenKhoan = "Tiền phòng", DonViTinh = "tháng",
                KyHopDongId = choice.Term.Id, SoLuong = 1, DonGia = choice.Term.GiaThue, ThanhTien = choice.Term.GiaThue
            },
            new()
            {
                SoThuTu = 2, LoaiKhoan = "DICH_VU", DichVuId = electricity.Id, TenKhoan = "Điện",
                CachTinhApDung = CachTinhDichVu.TheoChiSo, DonViTinh = "kWh", ChiSoDau = 100.125m,
                ChiSoCuoi = 120.25m, SoLuong = 20.125m, DonGia = 3500, ThanhTien = 70438
            },
            new()
            {
                SoThuTu = 3, LoaiKhoan = "DICH_VU", DichVuId = water.Id, TenKhoan = "Nước",
                CachTinhApDung = CachTinhDichVu.TheoChiSo, DonViTinh = "m³", ChiSoDau = 10.1m,
                ChiSoCuoi = 12.25m, SoLuong = 2.15m, DonGia = 5000, ThanhTien = 10750
            },
            new()
            {
                SoThuTu = 4, LoaiKhoan = "DICH_VU", TenKhoan = "Internet",
                CachTinhApDung = CachTinhDichVu.CoDinh, DonViTinh = "phòng", SoLuong = 1,
                DonGia = 100000, ThanhTien = 100000
            },
            new()
            {
                SoThuTu = 5, LoaiKhoan = "DICH_VU", TenKhoan = "Phí dịch vụ",
                CachTinhApDung = CachTinhDichVu.CoDinh, DonViTinh = "phòng", SoLuong = 1,
                DonGia = 50000, ThanhTien = 50000
            }
        };
        var issueDate = today;
        var dueDate = issueDate.AddDays(7);
        var bill = new HoaDon
        {
            MaHoaDon = $"HD-DEMO-{monthCode}-{Guid.NewGuid().ToString("N")[..8]}",
            HopDongId = choice.Id, Thang = choice.First.Month, Nam = choice.First.Year,
            TuNgay = choice.First, DenNgay = choice.Last, NgayChot = choice.Last,
            SoNguoiTinhPhi = 1, LoaiHoaDon = "DINH_KY", NgayLap = DateTime.UtcNow,
            HanThanhToan = dueDate, TongTien = lines.Sum(x => x.ThanhTien),
            TrangThai = "NHAP", NguoiLapId = ownerId, ChiTiet = lines
        };
        db.HoaDons.Add(bill);
        await db.SaveChangesAsync();
        await new HoaDonDichVuService(db, new DichVuService(db)).PhatHanhNhapAsync(ownerId, new()
        {
            Id = bill.Id, PhienBan = bill.PhienBan, NgayPhatHanh = issueDate,
            HanThanhToan = dueDate, XacNhan = true
        });
        await File.WriteAllTextAsync(codePath, bill.MaHoaDon);
        Console.WriteLine($"Tenant invoice demo ready for room {choice.MaPhong}, period {choice.First:MM/yyyy}: {bill.MaHoaDon}");
        Console.WriteLine($"Open /ThongBao/ChiTiet?maHoaDon={Uri.EscapeDataString(bill.MaHoaDon)} as the tenant account.");
        Console.WriteLine("Demo-only prices: electricity 3,500 đ/kWh, water 5,000 đ/m³, internet 100,000 đ/room, service fee 50,000 đ/room.");
    }
}
