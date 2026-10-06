using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using QL_PhongTro.Services;
using QL_PhongTro.Authorization;
using QL_PhongTro.Data;
using QL_PhongTro.Models;
using QL_PhongTro.ViewModels;

namespace QL_PhongTro.Controllers;

[Authorize, ModuleAccess("HOP_DONG")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class HopDongController(AppDbContext db, ITimeProvider? clock = null) : Controller
{
    private int AccountId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    private DateOnly HomNay => DateOnly.FromDateTime((clock ?? new SystemTimeProvider()).UtcNow.AddHours(7));
    private async Task<bool> IsAdmin(CancellationToken ct) => await db.TaiKhoans.AnyAsync(x => x.Id == AccountId && x.VaiTro == "ADMIN" && x.DangHoatDong && !x.IsDeleted, ct);
    private async Task<bool> ForeignRequest(int id, CancellationToken ct) => !await IsAdmin(ct) && await
        (from y in db.YeuCauThues join t in db.TinDangs on y.TinDangId equals t.Id
         join p in db.PhongTros on t.PhongId equals p.Id join b in db.ToaNhas on p.ToaNhaId equals b.Id
         where y.Id == id && b.ChuNhaId != AccountId select y.Id).AnyAsync(ct);

    private async Task<List<YeuCauHopDong>> Approved(CancellationToken ct)
    {
        var admin = await IsAdmin(ct);
        return await (from y in db.YeuCauThues.AsNoTracking()
                      join t in db.TinDangs on y.TinDangId equals t.Id
                      join p in db.PhongTros on t.PhongId equals p.Id
                      join b in db.ToaNhas on p.ToaNhaId equals b.Id
                      join k in db.KhachThues on y.KhachThueId equals k.Id
                      where y.TrangThai == "DA_DUYET" && (admin || b.ChuNhaId == AccountId)
                          && !db.HopDongs.Any(h => h.YeuCauThueId == y.Id && h.TrangThai != "NHAP")
                      orderby y.NgayTao descending
                      select new YeuCauHopDong { Id = y.Id, Ma = y.MaYeuCau, KhachId = k.Id,
                          Khach = k.HoTen, DienThoai = k.SoDienThoai, PhongId = p.Id,
                          Phong = p.MaPhong, ToaNha = b.TenToaNha, NgayMongMuon = y.NgayMongMuon,
                          GiaThue = p.GiaThue, TienCoc = p.GiaThue, NgayChot = b.NgayChotHangThang, PhienBanPhong = p.PhienBan }).ToListAsync(ct);
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var admin = await IsAdmin(ct);
        var rows = await (from h in db.HopDongs.AsNoTracking()
                          join p in db.PhongTros on h.PhongId equals p.Id
                          join b in db.ToaNhas on p.ToaNhaId equals b.Id
                          join k in db.KhachThues on h.KhachDungTenId equals k.Id into clients
                          from k in clients.DefaultIfEmpty()
                          where admin || b.ChuNhaId == AccountId
                              || (User.IsInRole("QUAN_LY") && b.QuanLyId == AccountId)
                              || (User.IsInRole("KHACH_THUE") && k != null && k.TaiKhoanId == AccountId)
                          orderby h.Id descending
                          select new HopDongDanhSach(h, p.MaPhong, b.TenToaNha, k == null ? null : k.HoTen)).ToListAsync(ct);
        return View(rows);
    }

    [HttpGet, ModuleAccess("HOP_DONG", write: true)]
    public async Task<IActionResult> Create(int? yeuCauId, CancellationToken ct)
    {
        var vm = new HopDongCreateViewModel { YeuCauId = yeuCauId, SoThang = 12, SoThangCoc = 1, HomNay = HomNay };
        vm.YeuCaus = await Approved(ct);
        vm.DaChon = vm.YeuCaus.SingleOrDefault(x => x.Id == yeuCauId);
        if (yeuCauId.HasValue && vm.DaChon is null)
            return await ForeignRequest(yeuCauId.Value, ct) ? Forbid() : NotFound();
        if (vm.DaChon is { } y)
        {
            vm.GiaThue = y.GiaThue; vm.TienCoc = y.TienCoc;
            vm.PhienBanPhong = y.PhienBanPhong;
            vm.NgayBatDau = y.NgayMongMuon; vm.NgayChot = y.NgayChot;
            var draft = await db.HopDongs.AsNoTracking().SingleOrDefaultAsync(h => h.YeuCauThueId == y.Id && h.TrangThai == "NHAP", ct);
            if (draft is not null)
            {
                var period = await db.KyHopDongs.AsNoTracking().FirstOrDefaultAsync(k => k.HopDongId == draft.Id && k.SoThuTu == 1, ct);
                vm.NgayChot = draft.NgayChot;
                // Preserve old drafts only when the deposit is an exact 0..3-month multiple of the current room price.
                if (y.GiaThue > 0 && draft.TienCoc >= 0 && draft.TienCoc % y.GiaThue == 0 && draft.TienCoc / y.GiaThue <= 3)
                    vm.SoThangCoc = (int)(draft.TienCoc / y.GiaThue);
                if (period is not null) { vm.NgayBatDau = period.NgayBatDau; vm.SoThang = period.SoThang; }
            }
            if (vm.NgayBatDau < vm.HomNay) vm.NgayBatDau = vm.HomNay;
            TinhTienCoc(vm);
        }
        if (vm.DaChon is not null && vm.NgayBatDau is { } start && vm.NgayKetThuc is { } end)
            vm.ChongLans = await new HopDongService(db).ChongLanAsync(vm.DaChon.PhongId, start, end, ct);
        return View(vm);
    }

    [HttpGet, ModuleAccess("HOP_DONG", write: true)]
    public async Task<IActionResult> KiemTra(int yeuCauId, DateOnly ngayBatDau, int soThang, CancellationToken ct)
    {
        var y = (await Approved(ct)).SingleOrDefault(x => x.Id == yeuCauId);
        if (y is null) return await ForeignRequest(yeuCauId, ct) ? Forbid() : NotFound();
        if (!ModelState.IsValid || soThang is < 1 or > 120 || ngayBatDau.Year > 9988 || ngayBatDau < HomNay) return BadRequest();
        var conflicts = await new HopDongService(db).ChongLanAsync(y.PhongId, ngayBatDau, HopDongCreateViewModel.TinhNgayKetThuc(ngayBatDau, soThang), ct);
        return Json(conflicts.Select(x => new { x.Ma, TuNgay = x.NgayBatDau.ToString("dd/MM/yyyy"), DenNgay = x.NgayKetThuc.ToString("dd/MM/yyyy") }));
    }

    [HttpPost, ValidateAntiForgeryToken, ModuleAccess("HOP_DONG", write: true)]
    public async Task<IActionResult> Create(HopDongCreateViewModel vm, CancellationToken ct)
    {
        // BEGIN IMMEDIATE serializes writers before checking overlaps and allocating a code.
        await db.Database.OpenConnectionAsync(ct);
        await using var sqliteTransaction = ((SqliteConnection)db.Database.GetDbConnection()).BeginTransaction(deferred: false);
        await using var transaction = await db.Database.UseTransactionAsync(sqliteTransaction, ct);
        vm.YeuCaus = await Approved(ct);
        vm.DaChon = vm.YeuCaus.SingleOrDefault(x => x.Id == vm.YeuCauId);
        vm.HomNay = HomNay;
        vm.GiaThue = vm.DaChon?.GiaThue;
        // Prices and amounts are display-only: ignore any values or validation errors submitted for them.
        ModelState.Remove(nameof(vm.GiaThue)); ModelState.Remove(nameof(vm.TienCoc));
        TinhTienCoc(vm);
        if (vm.DaChon is null && vm.YeuCauId.HasValue && await ForeignRequest(vm.YeuCauId.Value, ct)) return Forbid();
        if (vm.DaChon is null) ModelState.AddModelError(nameof(vm.YeuCauId), "Yêu cầu không được duyệt, không thuộc quyền quản lý hoặc đã lập hợp đồng.");
        // Revalidate independently of the browser and never bind a submitted end date.
        var errors = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        System.ComponentModel.DataAnnotations.Validator.TryValidateObject(vm, new(vm), errors, true);
        foreach (var error in errors) ModelState.AddModelError(error.MemberNames.FirstOrDefault() ?? "", error.ErrorMessage!);
        if (vm.NgayKetThuc is null) ModelState.AddModelError(nameof(vm.NgayBatDau), "Ngày bắt đầu hoặc kỳ hạn không hợp lệ.");
        if (vm.NgayBatDau < vm.HomNay)
            ModelState.AddModelError(nameof(vm.NgayBatDau), "Ngày bắt đầu không được trước ngày hiện tại.");
        if (vm.DaChon is not null && (vm.GiaThue is not > 0 || vm.TienCoc is null))
            ModelState.AddModelError("", "Giá phòng hoặc số tháng cọc không hợp lệ, không thể tính tiền cọc.");
        if (vm.DaChon is { } chosen && vm.PhienBanPhong != chosen.PhienBanPhong)
            ModelState.AddModelError("", "Thông tin phòng đã thay đổi. Vui lòng tải lại trang trước khi lưu.");
        foreach (var meter in new[] { (nameof(vm.ChiSoDien), vm.ChiSoDien), (nameof(vm.ChiSoNuoc), vm.ChiSoNuoc) })
            if (meter.Item2.HasValue && decimal.Round(meter.Item2.Value, 3) != meter.Item2.Value)
                ModelState.AddModelError(meter.Item1, "Chỉ số đầu kỳ chỉ được có tối đa 3 chữ số thập phân.");
        if (vm.DaChon is not null && vm.NgayBatDau is { } start && vm.NgayKetThuc is { } end)
        {
            vm.ChongLans = await new HopDongService(db).ChongLanAsync(vm.DaChon.PhongId, start, end, ct);
            if (vm.ChongLans.Count > 0) ModelState.AddModelError("", "Không thể lưu: phòng có hợp đồng hiệu lực chồng lấn. Xem thông tin hợp đồng đang vướng bên phải.");
        }
        if (!ModelState.IsValid) return View(vm);
        var y = vm.DaChon!;
        var room = await db.PhongTros.SingleAsync(p => p.Id == y.PhongId, ct);
        if (room.TrangThai == "NGUNG_CHO_THUE")
        {
            ModelState.AddModelError("", "Phòng đang ngừng cho thuê, không thể lập hợp đồng.");
            return View(vm);
        }
        var now = (clock ?? new SystemTimeProvider()).UtcNow;
        string code;
        try { code = await new HopDongService(db).SinhMaAsync(now.AddHours(7).Year, ct); }
        catch (ArgumentException ex) { ModelState.AddModelError("", ex.Message); return View(vm); }
        try
        {
        var contract = await db.HopDongs.SingleOrDefaultAsync(h => h.YeuCauThueId == y.Id && h.TrangThai == "NHAP", ct);
        if (contract is null)
        {
            contract = new HopDongThamChieu { YeuCauThueId = y.Id, NguoiLapId = AccountId, NgayTao = now };
            db.HopDongs.Add(contract);
        }
        contract.MaHopDong = code; contract.PhongId = y.PhongId; contract.KhachDungTenId = y.KhachId;
        contract.TienCoc = vm.TienCoc!.Value; contract.NgayChot = vm.NgayChot!.Value; contract.TrangThai = "DANG_HIEU_LUC";
        await db.SaveChangesAsync(ct);
        var period = await db.KyHopDongs.SingleOrDefaultAsync(k => k.HopDongId == contract.Id && k.SoThuTu == 1, ct);
        if (period is null) { period = new KyHopDongThamChieu { HopDongId = contract.Id, NguoiLapId = AccountId, NgayTao = now }; db.KyHopDongs.Add(period); }
        period.NgayBatDau = vm.NgayBatDau!.Value; period.NgayKetThuc = vm.NgayKetThuc!.Value;
        period.GiaThue = vm.GiaThue!.Value; period.SoThang = vm.SoThang!.Value;
        db.HopDongChiSoDauKys.Add(new HopDongChiSoDauKy { HopDongId = contract.Id, NgayBanGiao = vm.NgayBatDau.Value,
            ChiSoDien = vm.ChiSoDien!.Value, ChiSoNuoc = vm.ChiSoNuoc!.Value, NguoiNhapId = AccountId, NgayNhap = now });
        room.TrangThai = "DANG_THUE";
        room.PhienBan++;
        await db.SaveChangesAsync(ct);
        await sqliteTransaction.CommitAsync(ct);
        TempData["ContractSuccess"] = $"Đã tạo hợp đồng {code}, ghi chỉ số điện nước đầu kỳ. Phòng {y.Phong} — {y.ToaNha}: Đang thuê.";
        return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (ex is DbUpdateException or SqliteException)
        {
            await sqliteTransaction.RollbackAsync(ct);
            db.ChangeTracker.Clear();
            ModelState.AddModelError("", "Không thể lưu hợp đồng. Không có thay đổi nào được ghi nhận. Vui lòng tải lại trang và thử lại.");
            return View(vm);
        }
    }

    private static void TinhTienCoc(HopDongCreateViewModel vm)
    {
        vm.TienCoc = null;
        if (vm.GiaThue is > 0 && vm.SoThangCoc is >= 0 and <= 3)
        {
            var amount = (decimal)vm.GiaThue.Value * vm.SoThangCoc.Value;
            if (amount <= long.MaxValue) vm.TienCoc = (long)amount;
        }
    }
}
