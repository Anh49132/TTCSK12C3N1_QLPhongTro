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
        var ids = rows.Select(x => x.HopDong.Id).ToList();
        var periods = await db.KyHopDongs.AsNoTracking().Where(x => ids.Contains(x.HopDongId)).OrderByDescending(x => x.NgayBatDau).ToListAsync(ct);
        ViewData["HomNay"] = HomNay;
        return View(rows.Select(x => x with { Ky = periods.FirstOrDefault(k => k.HopDongId == x.HopDong.Id) }).ToList());
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
        bool luuNhap = vm.Intent == "NHAP";
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
        contract.TienCoc = vm.TienCoc!.Value; contract.NgayChot = vm.NgayChot!.Value; contract.TrangThai = luuNhap ? "NHAP" : "DANG_HIEU_LUC";
        await db.SaveChangesAsync(ct);
        var period = await db.KyHopDongs.SingleOrDefaultAsync(k => k.HopDongId == contract.Id && k.SoThuTu == 1, ct);
        if (period is null) { period = new KyHopDongThamChieu { HopDongId = contract.Id, NguoiLapId = AccountId, NgayTao = now }; db.KyHopDongs.Add(period); }
        period.NgayBatDau = vm.NgayBatDau!.Value; period.NgayKetThuc = vm.NgayKetThuc!.Value;
        period.GiaThue = vm.GiaThue!.Value; period.SoThang = vm.SoThang!.Value;
        var meter = await db.HopDongChiSoDauKys.SingleOrDefaultAsync(m => m.HopDongId == contract.Id, ct);
        if (meter is null) { meter = new HopDongChiSoDauKy { HopDongId = contract.Id }; db.HopDongChiSoDauKys.Add(meter); }
        meter.NgayBanGiao = vm.NgayBatDau.Value;
        meter.ChiSoDien = vm.ChiSoDien!.Value;
        meter.ChiSoNuoc = vm.ChiSoNuoc!.Value;
        meter.NguoiNhapId = AccountId;
        meter.NgayNhap = now;
        if (!luuNhap)
        {
            room.TrangThai = "DANG_THUE";
            room.PhienBan++;
            // Hợp đồng có hiệu lực: mọi tin đăng của phòng rời khỏi trang công khai cùng lúc với phòng.
            await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE tin_dang SET trang_thai='DA_CHO_THUE' WHERE phong_id={room.Id} AND trang_thai<>'DA_CHO_THUE'", ct);
        }
        await db.SaveChangesAsync(ct);
        await sqliteTransaction.CommitAsync(ct);
        if (luuNhap)
        {
            TempData["ContractSuccess"] = $"Đã lưu hợp đồng {code} ở trạng thái nháp. Mở chi tiết hợp đồng và bấm Kích hoạt khi sẵn sàng cho thuê.";
            return RedirectToAction(nameof(Details), new { id = contract.Id });
        }
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

    [HttpPost, ValidateAntiForgeryToken, ModuleAccess("HOP_DONG", write: true)]
    public async Task<IActionResult> KichHoat(int id, CancellationToken ct)
    {
        if (!await db.HopDongs.AnyAsync(h => h.Id == id, ct)) return NotFound();
        if (!await OwnContract(id, ct)) return Forbid();
        try
        {
            // BEGIN IMMEDIATE serializes writers so two owners cannot activate one room at once.
            await db.Database.OpenConnectionAsync(ct);
            await using var sqliteTransaction = ((SqliteConnection)db.Database.GetDbConnection()).BeginTransaction(deferred: false);
            await using var transaction = await db.Database.UseTransactionAsync(sqliteTransaction, ct);
            if (!await OwnContract(id, ct)) return Forbid();
            var hopDong = await db.HopDongs.SingleAsync(h => h.Id == id, ct);
            var room = await db.PhongTros.SingleAsync(p => p.Id == hopDong.PhongId, ct);
            if (hopDong.TrangThai == "DANG_HIEU_LUC")
                ModelState.AddModelError("", "Hợp đồng đã hiệu lực. Phòng và tin đăng đã được cập nhật trước đó.");
            else if (hopDong.TrangThai is not ("NHAP" or "CHO_HIEU_LUC"))
                ModelState.AddModelError("", "Chỉ kích hoạt được hợp đồng ở trạng thái nháp hoặc chờ hiệu lực.");
            if (room.TrangThai == "NGUNG_CHO_THUE")
                ModelState.AddModelError("", "Phòng đang ngừng cho thuê, không thể kích hoạt hợp đồng.");
            var period = await db.KyHopDongs.AsNoTracking().Where(k => k.HopDongId == id).OrderByDescending(k => k.NgayBatDau).FirstOrDefaultAsync(ct);
            if (period is not null)
            {
                var conflicts = await new HopDongService(db).ChongLanAsync(room.Id, period.NgayBatDau, period.NgayKetThuc, ct);
                if (conflicts.Any(c => c.Id != id))
                    ModelState.AddModelError("", $"Phòng {room.MaPhong} đã có hợp đồng hiệu lực khác. Không thể kích hoạt.");
            }
            if (!ModelState.IsValid) return View("Details", await DetailModel(id, null, ct));
            // Hợp đồng có hiệu lực: phòng chuyển Đang thuê và mọi tin đăng của phòng chuyển
            // Đã cho thuê trong cùng một giao dịch, không để phòng đã thuê mà tin vẫn hiển thị.
            hopDong.TrangThai = "DANG_HIEU_LUC";
            room.TrangThai = "DANG_THUE";
            room.PhienBan++;
            await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE tin_dang SET trang_thai='DA_CHO_THUE' WHERE phong_id={room.Id} AND trang_thai<>'DA_CHO_THUE'", ct);
            await db.SaveChangesAsync(ct);
            await sqliteTransaction.CommitAsync(ct);
            TempData["ContractSuccess"] = $"Hợp đồng {hopDong.MaHopDong} đã hiệu lực. Phòng {room.MaPhong}: Đang thuê; tin đăng: Đã cho thuê.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex) when (ex is DbUpdateException or SqliteException)
        {
            db.ChangeTracker.Clear();
            ModelState.AddModelError("", "Không thể kích hoạt hợp đồng. Không có thay đổi nào được lưu. Vui lòng tải lại trang và thử lại.");
            return View("Details", await DetailModel(id, null, ct));
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

    private async Task<bool> OwnContract(int id, CancellationToken ct) => await
        (from h in db.HopDongs join p in db.PhongTros on h.PhongId equals p.Id
         join b in db.ToaNhas on p.ToaNhaId equals b.Id
         join a in db.TaiKhoans on b.ChuNhaId equals a.Id
         where h.Id == id && a.Id == AccountId && a.VaiTro == "CHU_NHA" && a.DangHoatDong && !a.IsDeleted
         select h.Id).AnyAsync(ct);

    private async Task<HopDongDetailsViewModel> DetailModel(int id, NguoiOGhepInput? input, CancellationToken ct)
    {
        var h = await db.HopDongs.AsNoTracking().SingleAsync(x => x.Id == id, ct);
        var p = await db.PhongTros.AsNoTracking().SingleAsync(x => x.Id == h.PhongId, ct);
        var signer = await db.KhachThues.AsNoTracking().SingleOrDefaultAsync(x => x.Id == h.KhachDungTenId, ct);
        var model = new HopDongDetailsViewModel { HopDong = h, Phong = p.MaPhong, GioiHan = p.SoNguoiToiDa, DungTen = signer,
            HomNay = HomNay, ChoThem = signer != null && h.TrangThai is "CHO_HIEU_LUC" or "DANG_HIEU_LUC",
            Input = input ?? new() { NgayVao = HomNay, PhienBanPhong = p.PhienBan },
            ChuyenDi = new() { NgayRa = HomNay, PhienBanPhong = p.PhienBan } };
        model.ToaNha = await db.ToaNhas.Where(x => x.Id == p.ToaNhaId).Select(x => x.TenToaNha).SingleAsync(ct);
        model.Ky = await db.KyHopDongs.AsNoTracking().Where(x => x.HopDongId == id).OrderByDescending(x => x.NgayBatDau).FirstOrDefaultAsync(ct);
        model.ChiSo = await db.HopDongChiSoDauKys.AsNoTracking().SingleOrDefaultAsync(x => x.HopDongId == id, ct);
        var people = await (from g in db.NguoiOGheps.AsNoTracking() join k in db.KhachThues on g.KhachThueId equals k.Id
            where g.HopDongId == id orderby g.NgayVao, g.Id
            select new NguoiOGhepRow(k.HoTen, k.SoDienThoai, k.SoGiayTo, g.NgayVao) { Id = g.Id, NgayRa = g.NgayRa }).ToListAsync(ct);
        if (h.TrangThai != "DANG_HIEU_LUC" || !await db.KyHopDongs.AnyAsync(k => k.HopDongId == id && k.NgayBatDau <= HomNay && k.NgayKetThuc >= HomNay, ct))
        {
            static string? Mask(string? value) => value == null ? null : new string('*', Math.Max(0, value.Length - 4)) + value[^Math.Min(4, value.Length)..];
            if (signer != null) { signer.SoDienThoai = Mask(signer.SoDienThoai); signer.SoGiayTo = Mask(signer.SoGiayTo); }
            people = people.Select(x => x with { SoDienThoai = Mask(x.SoDienThoai), SoGiayTo = Mask(x.SoGiayTo) }).ToList();
        }
        model.Nguois = people.Where(x => x.NgayVao <= HomNay && (x.NgayRa == null || x.NgayRa >= HomNay)).ToList();
        model.DaChuyenDi = people.Where(x => x.NgayRa < HomNay).OrderByDescending(x => x.NgayRa).ToList();
        model.SapVao = people.Where(x => x.NgayVao > HomNay).ToList();
        return model;
    }

    [HttpGet]
    public async Task<IActionResult> LichSuNguoiO(int phongId, DateOnly? tuNgay, DateOnly? denNgay, CancellationToken ct)
    {
        if (!await db.PhongTros.AnyAsync(p => p.Id == phongId, ct)) return NotFound();
        var service = new LichSuNguoiOService(db);
        var room = await service.PhongAsync(AccountId, phongId, ct);
        if (room == null) return Forbid();
        var model = new LichSuNguoiOViewModel { PhongId = phongId, Phong = room.Value.Phong, ToaNha = room.Value.ToaNha,
            TuNgay = tuNgay ?? new DateOnly(HomNay.Year, HomNay.Month, 1), DenNgay = denNgay ?? HomNay };
        if (model.DenNgay < model.TuNgay) ModelState.AddModelError(nameof(model.DenNgay), "Đến ngày không được trước từ ngày.");
        if (ModelState.IsValid)
        {
            var history = await service.LayAsync(AccountId, phongId, model.TuNgay.Value, model.DenNgay.Value, HomNay, ct);
            model.Nguois = history.Rows; model.HopDongThieuDuLieu = history.Missing; model.DaLoc = true;
        }
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        if (!await db.HopDongs.AnyAsync(x => x.Id == id, ct)) return NotFound();
        if (!await OwnContract(id, ct)) return Forbid();
        return View(await DetailModel(id, null, ct));
    }

    [HttpPost, ValidateAntiForgeryToken, ModuleAccess("HOP_DONG", write: true)]
    public async Task<IActionResult> ChuyenDi(int id, int nguoiId, [Bind(Prefix = "ChuyenDi")] ChuyenDiInput input, CancellationToken ct)
    {
        if (!await db.HopDongs.AnyAsync(h => h.Id == id, ct)) return NotFound();
        if (!await OwnContract(id, ct)) return Forbid();
        if (!await db.NguoiOGheps.AnyAsync(g => g.Id == nguoiId && g.HopDongId == id, ct)) return Forbid();
        async Task<IActionResult> Invalid()
        {
            var model = await DetailModel(id, null, ct);
            model.ChuyenDi = input; model.ChuyenDiNguoiId = nguoiId;
            return View("Details", model);
        }
        if (input.NgayRa == null) ModelState.AddModelError("ChuyenDi.NgayRa", "Chọn ngày chuyển đi.");
        if (input.NgayRa?.Year > 9998) ModelState.AddModelError("ChuyenDi.NgayRa", "Ngày chuyển đi ngoài phạm vi hỗ trợ.");
        if (!ModelState.IsValid) return await Invalid();
        try
        {
            await db.Database.OpenConnectionAsync(ct);
            await using var sqlite = ((SqliteConnection)db.Database.GetDbConnection()).BeginTransaction(deferred: false);
            await using var transaction = await db.Database.UseTransactionAsync(sqlite, ct);
            if (!await OwnContract(id, ct)) return Forbid();
            var h = await db.HopDongs.SingleAsync(x => x.Id == id, ct);
            var room = await db.PhongTros.SingleAsync(x => x.Id == h.PhongId, ct);
            var stay = await db.NguoiOGheps.SingleOrDefaultAsync(g => g.Id == nguoiId && g.HopDongId == id, ct);
            if (stay == null) return Forbid();
            if (h.TrangThai is not ("DANG_HIEU_LUC" or "CHO_HIEU_LUC"))
                ModelState.AddModelError("", "Chỉ ghi nhận chuyển đi trong hợp đồng đang hoặc chờ hiệu lực.");
            if (room.PhienBan != input.PhienBanPhong)
                ModelState.AddModelError("", "Dữ liệu phòng đã thay đổi. Vui lòng tải lại trang trước khi lưu.");
            if (stay.NgayRa != null) ModelState.AddModelError("", "Người ở ghép đã được ghi ngày chuyển đi. Không thể ghi lại.");
            if (input.NgayRa < stay.NgayVao) ModelState.AddModelError("ChuyenDi.NgayRa", "Ngày chuyển đi không được trước ngày bắt đầu ở cùng.");
            if (!ModelState.IsValid) return await Invalid();
            var day = input.NgayRa!.Value;
            var next = new DateOnly(day.Year, day.Month, 1).AddMonths(1);
            using var probe = db.Database.GetDbConnection().CreateCommand();
            probe.Transaction = sqlite;
            probe.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='hoa_don'";
            var invoicesReady = Convert.ToInt32(await probe.ExecuteScalarAsync(ct)) == 1;
            // Contracts also work on databases where the optional invoice module is absent.
            var issued = invoicesReady ? await db.HoaDons.AsNoTracking().Where(x => x.HopDongId == id && x.TrangThai != "DA_HUY"
                && (x.Nam > next.Year || (x.Nam == next.Year && x.Thang >= next.Month)))
                .OrderBy(x => x.Nam).ThenBy(x => x.Thang).Select(x => new { x.Nam, x.Thang }).ToListAsync(ct) : [];
            stay.NgayRa = day; room.PhienBan++;
            await db.SaveChangesAsync(ct);
            await sqlite.CommitAsync(ct);
            TempData["ContractSuccess"] = $"Đã ghi ngày chuyển đi {day:dd/MM/yyyy}. Ngày chuyển đi vẫn tính là ngày còn ở; giảm khoản khoán từ kỳ {next:MM/yyyy}.";
            if (issued.Count > 0)
                TempData["ContractWarning"] = "Hóa đơn các kỳ " + string.Join(", ", issued.Select(x => $"{x.Thang:00}/{x.Nam}"))
                    + " đã lập được giữ nguyên. Số người giảm chỉ áp dụng cho hóa đơn chưa lập; hãy kiểm tra các hóa đơn này.";
            return RedirectToAction(nameof(Details), new { id, tab = "people" });
        }
        catch (Exception ex) when (ex is DbUpdateException or SqliteException)
        {
            db.ChangeTracker.Clear();
            ModelState.AddModelError("", "Không thể ghi nhận chuyển đi. Không có thay đổi nào được lưu. Vui lòng tải lại và thử lại.");
            return await Invalid();
        }
    }

    [HttpPost, ValidateAntiForgeryToken, ModuleAccess("HOP_DONG", write: true)]
    public async Task<IActionResult> ThemNguoi(int id, [Bind(Prefix = "Input")] NguoiOGhepInput input, CancellationToken ct)
    {
        if (!await db.HopDongs.AnyAsync(x => x.Id == id, ct)) return NotFound();
        if (!await OwnContract(id, ct)) return Forbid();
        input.HoTen = input.HoTen?.Trim() ?? "";
        input.SoDienThoai = input.SoDienThoai?.Trim() ?? "";
        input.SoGiayTo = input.SoGiayTo?.Trim() ?? "";
        var errors = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        System.ComponentModel.DataAnnotations.Validator.TryValidateObject(input, new(input), errors, true);
        foreach (var error in errors) ModelState.AddModelError("Input." + error.MemberNames.FirstOrDefault(), error.ErrorMessage!);
        async Task<IActionResult> Invalid() => View("Details", await DetailModel(id, input, ct));
        if (!ModelState.IsValid) return await Invalid();
        try
        {
            await db.Database.OpenConnectionAsync(ct);
            await using var sqlite = ((SqliteConnection)db.Database.GetDbConnection()).BeginTransaction(deferred: false);
            await using var transaction = await db.Database.UseTransactionAsync(sqlite, ct);
            if (!await OwnContract(id, ct)) return Forbid();
            var h = await db.HopDongs.SingleAsync(x => x.Id == id, ct);
            var room = await db.PhongTros.SingleAsync(x => x.Id == h.PhongId, ct);
            var signer = await db.KhachThues.SingleOrDefaultAsync(x => x.Id == h.KhachDungTenId, ct);
            if (signer is null || h.TrangThai is not ("DANG_HIEU_LUC" or "CHO_HIEU_LUC"))
                ModelState.AddModelError("", "Chỉ thêm người vào hợp đồng đã chốt có một người đứng tên.");
            if (room.PhienBan != input.PhienBanPhong)
                ModelState.AddModelError("", "Dữ liệu phòng đã thay đổi. Vui lòng tải lại trang trước khi lưu.");
            var day = input.NgayVao!.Value;
            if (!await db.KyHopDongs.AnyAsync(k => k.HopDongId == id && k.NgayBatDau <= day && k.NgayKetThuc >= day, ct))
                ModelState.AddModelError("Input.NgayVao", "Ngày bắt đầu ở cùng phải nằm trong kỳ hợp đồng.");
            // New stay has no end date: check every future arrival as well as the requested start.
            var stays = await db.NguoiOGheps.Where(g => g.HopDongId == id && (g.NgayRa == null || g.NgayRa >= day)).ToListAsync(ct);
            var peak = stays.Select(g => g.NgayVao).Where(d => d >= day).Append(day)
                .Max(d => 1 + stays.Count(g => g.NgayVao <= d && (g.NgayRa == null || g.NgayRa >= d)));
            if (peak + 1 > room.SoNguoiToiDa)
                ModelState.AddModelError("", $"Phòng {room.MaPhong} tối đa {room.SoNguoiToiDa} người (bao gồm người đứng tên). Không thể thêm người ở ghép.");
            if (signer?.SoGiayTo == input.SoGiayTo || await
                (from g in db.NguoiOGheps join k in db.KhachThues on g.KhachThueId equals k.Id
                 where g.HopDongId == id && k.SoGiayTo == input.SoGiayTo && (g.NgayRa == null || g.NgayRa >= day)
                 select g.Id).AnyAsync(ct))
                ModelState.AddModelError("Input.SoGiayTo", "Căn cước trùng người đứng tên hoặc người ở ghép có khoảng ở chồng lấn.");
            if (!ModelState.IsValid) return await Invalid();
            // Do not link or modify a sensitive profile owned by an unrelated account.
            var profile = new KhachThue { HoTen = input.HoTen, SoDienThoai = input.SoDienThoai,
                SoGiayTo = input.SoGiayTo, NgayTao = (clock ?? new SystemTimeProvider()).UtcNow };
            db.KhachThues.Add(profile);
            await db.SaveChangesAsync(ct);
            db.NguoiOGheps.Add(new() { HopDongId = id, KhachThueId = profile.Id, NgayVao = day });
            room.PhienBan++;
            await db.SaveChangesAsync(ct);
            await sqlite.CommitAsync(ct);
            TempData["ContractSuccess"] = "Đã thêm người ở ghép. Người đứng tên tiếp tục chịu trách nhiệm thanh toán và nhận lại tiền cọc.";
            return RedirectToAction(nameof(Details), new { id, tab = "people" });
        }
        catch (Exception ex) when (ex is DbUpdateException or SqliteException)
        {
            db.ChangeTracker.Clear();
            ModelState.AddModelError("", "Không thể lưu do dữ liệu đang thay đổi. Vui lòng tải lại trang và thử lại.");
            return await Invalid();
        }
    }
}
