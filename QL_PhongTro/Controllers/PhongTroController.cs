using System.Globalization;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Data;
using QL_PhongTro.Authorization;
using QL_PhongTro.Models;
using QL_PhongTro.Services;
using QL_PhongTro.ViewModels;

namespace QL_PhongTro.Controllers;

[Authorize(Roles = "CHU_NHA,QUAN_LY,ADMIN")]
[ModuleAccess("PHONG_TRO")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class PhongTroController(AppDbContext db, DichVuPhongService roomServices, RoomImageStore imageStore) : Controller
{
    [HttpGet]
    public async Task<IActionResult> ToaNha(string? tuKhoa)
    {
        var ownerId = CurrentAccountId();
        if (ownerId is null)
            return Forbid();

        var query = db.ToaNhas.AsNoTracking().Where(building => building.ChuNhaId == ownerId);
        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var keyword = tuKhoa.Trim();
            query = query.Where(building => building.TenToaNha.Contains(keyword) || building.DiaChi.Contains(keyword));
        }

        var buildings = await query.OrderBy(building => building.TenToaNha)
            .Select(building => new ToaNhaTongHopViewModel
            {
                Id = building.Id,
                TenToaNha = building.TenToaNha,
                DiaChi = building.DiaChi,
                QuanLy = db.TaiKhoans.Where(account => account.Id == building.QuanLyId).Select(account => account.HoTen).FirstOrDefault(),
                SoPhong = db.PhongTros.Count(room => room.ToaNhaId == building.Id),
                SoPhongTrong = db.PhongTros.Count(room => room.ToaNhaId == building.Id && room.TrangThai == TrangThaiPhong.TRONG.ToString()),
                DangHoatDong = building.DangHoatDong
            }).ToListAsync();

        return View(new DanhSachToaNhaViewModel { TuKhoa = tuKhoa, ToaNhas = buildings });
    }

    public async Task<IActionResult> Index(int? toaNhaId, TrangThaiPhong? trangThaiFilter)
    {
        var ownerId = CurrentAccountId();
        if (ownerId is null)
            return Forbid();

        var buildings = await db.ToaNhas.AsNoTracking()
            .Where(building => building.ChuNhaId == ownerId && building.DangHoatDong)
            .OrderBy(building => building.TenToaNha)
            .Select(building => new SelectListItem(building.TenToaNha, building.Id.ToString()))
            .ToListAsync();

        var selectedId = buildings.Any(building => building.Value == toaNhaId?.ToString())
            ? toaNhaId
            : null;

        var rooms = selectedId is null
            ? []
            : await GetRoomQuery(selectedId.Value, trangThaiFilter)
                .OrderBy(room => room.Tang)
                .ThenBy(room => room.MaPhong)
                .ToListAsync();

        var statusCounts = Enum.GetValues<TrangThaiPhong>()
            .ToDictionary(status => status.ToString(), _ => 0);
        if (selectedId is not null)
        {
            var counts = await db.PhongTros.AsNoTracking()
                .Where(room => room.ToaNhaId == selectedId)
                .GroupBy(room => room.TrangThai)
                .Select(group => new { Status = group.Key, Count = group.Count() })
                .ToListAsync();
            foreach (var count in counts)
                statusCounts[count.Status] = count.Count;
        }

        return View(new DanhSachPhongViewModel
        {
            ToaNhaId = selectedId,
            TrangThaiFilter = trangThaiFilter,
            ToaNhaOptions = buildings,
            TrangThaiOptions = GetStatusOptions(),
            SoLuongTheoTrangThai = statusCounts,
            PhongTros = rooms
        });
    }

    [HttpGet]
    [ModuleAccess("PHONG_TRO", write: true)]
    public async Task<IActionResult> Create(int? toaNhaId)
    {
        var ownerId = CurrentAccountId();
        if (ownerId is null)
            return Forbid();

        var options = await GetBuildingOptions(ownerId.Value);
        if (options.Count == 0)
        {
            TempData["RoomWarning"] = "Hãy khai báo tòa nhà trước khi thêm phòng.";
            return RedirectToAction(nameof(TaoToaNha));
        }

        var selectedBuildingId = options.Any(building => building.Value == toaNhaId?.ToString())
            ? toaNhaId
            : null;

        return View(new TaoPhongViewModel
        {
            ToaNhaId = selectedBuildingId,
            ToaNhaOptions = options
        });
    }

    [HttpGet]
    [ModuleAccess("PHONG_TRO", write: true)]
    public async Task<IActionResult> CreateBulk(int? toaNhaId)
    {
        var ownerId = CurrentAccountId();
        if (ownerId is null)
            return Forbid();

        var options = await GetBuildingOptions(ownerId.Value);
        if (options.Count == 0)
        {
            TempData["RoomWarning"] = "Hãy khai báo tòa nhà trước khi tạo phòng hàng loạt.";
            return RedirectToAction(nameof(TaoToaNha));
        }

        var selectedBuildingId = options.Any(building => building.Value == toaNhaId?.ToString())
            ? toaNhaId
            : null;

        return View(new TaoPhongHangLoatViewModel
        {
            ToaNhaId = selectedBuildingId,
            ToaNhaOptions = options
        });
    }

    [HttpPost]
    [ModuleAccess("PHONG_TRO", write: true)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaoPhongViewModel model)
    {
        var ownerId = CurrentAccountId();
        if (ownerId is null)
            return Forbid();

        if (model.TrangThai is not null && !Enum.IsDefined(model.TrangThai.Value))
            ModelState.AddModelError(nameof(model.TrangThai), "Trạng thái phòng không hợp lệ.");
        BindRentPrice(nameof(model.GiaThue), model.GiaThueDisplay, price => model.GiaThue = price);

        var building = model.ToaNhaId is null
            ? null
            : await db.ToaNhas.SingleOrDefaultAsync(item =>
                item.Id == model.ToaNhaId && item.ChuNhaId == ownerId && item.DangHoatDong);

        if (building is null)
            ModelState.AddModelError(nameof(model.ToaNhaId), "Tòa nhà không hợp lệ.");

        var roomCode = model.MaPhong?.Trim() ?? string.Empty;
        if (building is not null && roomCode.Length > 0 && await db.PhongTros.AsNoTracking()
                .AnyAsync(room => room.ToaNhaId == building.Id && room.MaPhong == roomCode))
            ModelState.AddModelError(nameof(model.MaPhong), "Mã phòng đã được sử dụng trong tòa nhà này.");

        if (!ModelState.IsValid)
        {
            model.ToaNhaOptions = await GetBuildingOptions(ownerId.Value);
            return View(model);
        }

        var room = new PhongTro
        {
            ToaNhaId = building!.Id,
            MaPhong = roomCode,
            Tang = model.Tang!.Value,
            DienTich = model.DienTich!.Value,
            GiaThue = model.GiaThue!.Value,
            SoNguoiToiDa = model.SoNguoiToiDa!.Value,
            TrangThai = model.TrangThai!.Value.ToString(),
            NgayTao = DateTime.UtcNow
        };

        db.PhongTros.Add(room);
        await roomServices.GanMacDinhChoPhongMoiAsync([room]);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsRoomCodeConflict(exception))
        {
            db.Entry(room).State = EntityState.Detached;
            ModelState.AddModelError(nameof(model.MaPhong), "Mã phòng đã được sử dụng trong tòa nhà này.");
            model.ToaNhaOptions = await GetBuildingOptions(ownerId.Value);
            return View(model);
        }

        TempData["Success"] = "Đã thêm phòng thành công.";
        return RedirectToAction(nameof(Index), new { toaNhaId = building.Id });
    }

    [HttpPost]
    [ModuleAccess("PHONG_TRO", write: true)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBulk(TaoPhongHangLoatViewModel model)
    {
        var ownerId = CurrentAccountId();
        if (ownerId is null)
            return Forbid();

        if (model.TrangThai is not null && !Enum.IsDefined(model.TrangThai.Value))
            ModelState.AddModelError(nameof(model.TrangThai), "Trạng thái phòng không hợp lệ.");
        BindRentPrice(nameof(model.GiaThue), model.GiaThueDisplay, price => model.GiaThue = price);

        var building = model.ToaNhaId is null
            ? null
            : await db.ToaNhas.SingleOrDefaultAsync(item =>
                item.Id == model.ToaNhaId && item.ChuNhaId == ownerId && item.DangHoatDong);

        if (building is null)
            ModelState.AddModelError(nameof(model.ToaNhaId), "Tòa nhà không hợp lệ.");

        if (!ModelState.IsValid)
        {
            model.ToaNhaOptions = await GetBuildingOptions(ownerId.Value);
            return View(model);
        }

        var roomCodes = Enumerable.Range(1, model.SoTang!.Value)
            .SelectMany(floor => Enumerable.Range(1, model.SoPhongMoiTang!.Value)
                .Select(roomNumber => $"{floor}{roomNumber:00}"))
            .ToList();

        var generatedCodeSet = roomCodes.ToHashSet(StringComparer.Ordinal);
        var existingCodes = await db.PhongTros.AsNoTracking()
            .Where(room => room.ToaNhaId == building!.Id)
            .Select(room => room.MaPhong)
            .ToListAsync();
        var existingCode = existingCodes.FirstOrDefault(generatedCodeSet.Contains);

        if (existingCode is not null)
        {
            ModelState.AddModelError(string.Empty,
                $"Không tạo phòng: mã {existingCode} đã tồn tại trong tòa nhà. Không có phòng nào trong lô được lưu.");
            model.ToaNhaOptions = await GetBuildingOptions(ownerId.Value);
            return View(model);
        }

        var createdAt = DateTime.UtcNow;
        var rooms = roomCodes.Select(code => new PhongTro
        {
            ToaNhaId = building!.Id,
            MaPhong = code,
            Tang = int.Parse(code[..^2]),
            DienTich = model.DienTich!.Value,
            GiaThue = model.GiaThue!.Value,
            SoNguoiToiDa = model.SoNguoiToiDa!.Value,
            TrangThai = model.TrangThai!.Value.ToString(),
            NgayTao = createdAt
        }).ToList();

        db.PhongTros.AddRange(rooms);
        await roomServices.GanMacDinhChoPhongMoiAsync(rooms);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsRoomCodeConflict(exception))
        {
            foreach (var room in rooms)
                db.Entry(room).State = EntityState.Detached;

            ModelState.AddModelError(string.Empty,
                "Không tạo phòng vì một mã vừa được sử dụng trong tòa nhà. Không có phòng nào trong lô được lưu.");
            model.ToaNhaOptions = await GetBuildingOptions(ownerId.Value);
            return View(model);
        }

        TempData["Success"] = $"Đã tạo {rooms.Count} phòng thành công.";
        return RedirectToAction(nameof(Index), new { toaNhaId = building!.Id });
    }

    [HttpGet]
    [ModuleAccess("PHONG_TRO", write: true)]
    public async Task<IActionResult> TaoToaNha()
    {
        if (CurrentAccountId() is null)
            return Forbid();

        return View(new TaoToaNhaViewModel { QuanLyOptions = await GetManagerOptions() });
    }

    [HttpPost]
    [ModuleAccess("PHONG_TRO", write: true)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TaoToaNha(TaoToaNhaViewModel model)
    {
        var ownerId = CurrentAccountId();
        if (ownerId is null)
            return Forbid();

        model.QuanLyOptions = await GetManagerOptions();
        ValidateManager(model);
        if (!ModelState.IsValid)
            return View(model);

        var building = new ToaNha
        {
            ChuNhaId = ownerId.Value,
            TenToaNha = model.TenToaNha.Trim(),
            DiaChi = model.DiaChi.Trim(),
            PhuongXa = model.PhuongXa?.Trim(),
            QuanHuyen = model.QuanHuyen?.Trim(),
            TinhThanh = model.TinhThanh?.Trim(),
            SoTang = model.SoTang,
            QuanLyId = model.QuanLyId,
            GhiChu = model.GhiChu?.Trim(),
            NgayChotHangThang = 1,
            DangHoatDong = true
        };

        db.ToaNhas.Add(building);
        await db.SaveChangesAsync();

        TempData["Success"] = "Đã thêm tòa nhà thành công.";
        return RedirectToAction(nameof(Create), new { toaNhaId = building.Id });
    }

    [HttpGet]
    [ModuleAccess("PHONG_TRO", write: true)]
    public async Task<IActionResult> SuaToaNha(int id)
    {
        var building = await GetOwnedBuilding(id);
        if (building is null)
            return NotFound();

        return View(new TaoToaNhaViewModel
        {
            TenToaNha = building.TenToaNha,
            DiaChi = building.DiaChi,
            PhuongXa = building.PhuongXa,
            QuanHuyen = building.QuanHuyen,
            TinhThanh = building.TinhThanh,
            SoTang = building.SoTang,
            QuanLyId = building.QuanLyId,
            GhiChu = building.GhiChu,
            QuanLyOptions = await GetManagerOptions()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ModuleAccess("PHONG_TRO", write: true)]
    public async Task<IActionResult> SuaToaNha(int id, TaoToaNhaViewModel model)
    {
        var building = await GetOwnedBuilding(id);
        if (building is null)
            return NotFound();

        model.QuanLyOptions = await GetManagerOptions();
        ValidateManager(model);
        if (!ModelState.IsValid)
            return View(model);

        building.TenToaNha = model.TenToaNha.Trim();
        building.DiaChi = model.DiaChi.Trim();
        building.PhuongXa = model.PhuongXa?.Trim();
        building.QuanHuyen = model.QuanHuyen?.Trim();
        building.TinhThanh = model.TinhThanh?.Trim();
        building.SoTang = model.SoTang;
        building.QuanLyId = model.QuanLyId;
        building.GhiChu = model.GhiChu?.Trim();
        await db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật tòa nhà thành công.";
        return RedirectToAction(nameof(ToaNha));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ModuleAccess("PHONG_TRO", write: true)]
    public async Task<IActionResult> XoaToaNha(int id)
    {
        var building = await GetOwnedBuilding(id);
        if (building is null)
            return NotFound();

        if (await db.PhongTros.AnyAsync(room => room.ToaNhaId == id))
        {
            TempData["Error"] = "Không thể xóa tòa nhà đang có phòng. Hãy chuyển sang ngừng hoạt động nếu không còn sử dụng.";
            return RedirectToAction(nameof(ToaNha));
        }

        db.ToaNhas.Remove(building);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Không thể xóa tòa nhà đang được sử dụng hoặc vừa thay đổi. Vui lòng tải lại danh sách.";
            return RedirectToAction(nameof(ToaNha));
        }
        TempData["Success"] = "Đã xóa tòa nhà thành công.";
        return RedirectToAction(nameof(ToaNha));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ModuleAccess("PHONG_TRO", write: true)]
    public async Task<IActionResult> NgungHoatDong(int id)
    {
        var building = await GetOwnedBuilding(id);
        if (building is null)
            return NotFound();

        building.DangHoatDong = false;
        await db.SaveChangesAsync();
        TempData["Success"] = "Đã chuyển tòa nhà sang ngừng hoạt động.";
        return RedirectToAction(nameof(ToaNha));
    }

    private int? CurrentAccountId() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var accountId) ? accountId : null;

    private void ValidateManager(TaoToaNhaViewModel model)
    {
        if (model.QuanLyId is not null && !model.QuanLyOptions.Any(x => x.Value == model.QuanLyId.ToString()))
            ModelState.AddModelError(nameof(model.QuanLyId), "Vui lòng chọn người quản lý đang hoạt động trong danh sách.");
    }

    [HttpGet, ModuleAccess("PHONG_TRO", write: true)]
    public async Task<IActionResult> Edit(int id)
    {
        var room = await GetOwnedRoom(id);
        if (room is null) return NotFound();
        ViewData["RoomId"] = id;
        return View("Create", new TaoPhongViewModel
        {
            ToaNhaId = room.ToaNhaId, MaPhong = room.MaPhong, Tang = room.Tang,
            DienTich = room.DienTich, GiaThue = room.GiaThue,
            GiaThueDisplay = room.GiaThue.ToString(CultureInfo.InvariantCulture),
            SoNguoiToiDa = room.SoNguoiToiDa, TrangThai = Enum.Parse<TrangThaiPhong>(room.TrangThai),
            ToaNhaOptions = await GetBuildingOptions(CurrentAccountId()!.Value),
            Anh = await GetRoomImagesAsync(id)
        });
    }

    [HttpPost, ValidateAntiForgeryToken, ModuleAccess("PHONG_TRO", write: true)]
    public async Task<IActionResult> Edit(int id, TaoPhongViewModel model)
    {
        var room = await GetOwnedRoom(id);
        if (room is null) return NotFound();
        ViewData["RoomId"] = id;
        model.ToaNhaOptions = await GetBuildingOptions(CurrentAccountId()!.Value);
        if (model.ToaNhaId != room.ToaNhaId)
            ModelState.AddModelError(nameof(model.ToaNhaId), "Không thể chuyển phòng sang tòa nhà khác.");
        BindRentPrice(nameof(model.GiaThue), model.GiaThueDisplay, price => model.GiaThue = price);
        var code = model.MaPhong?.Trim() ?? string.Empty;
        if (await db.PhongTros.AnyAsync(x => x.ToaNhaId == room.ToaNhaId && x.MaPhong == code && x.Id != id))
            ModelState.AddModelError(nameof(model.MaPhong), "Mã phòng đã được sử dụng trong tòa nhà này.");
        if (!ModelState.IsValid)
        {
            model.Anh = await GetRoomImagesAsync(id);
            return View("Create", model);
        }
        room.MaPhong = code;
        room.Tang = model.Tang!.Value;
        room.DienTich = model.DienTich!.Value;
        room.GiaThue = model.GiaThue!.Value;
        room.SoNguoiToiDa = model.SoNguoiToiDa!.Value;
        room.TrangThai = model.TrangThai!.Value.ToString();
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException ex)
        {
            ModelState.AddModelError(string.Empty, IsRoomCodeConflict(ex)
                ? "Mã phòng đã được sử dụng trong tòa nhà này."
                : "Không thể cập nhật phòng lúc này. Vui lòng thử lại.");
            model.Anh = await GetRoomImagesAsync(id);
            return View("Create", model);
        }
        TempData["Success"] = "Đã cập nhật phòng thành công.";
        return RedirectToAction(nameof(Index), new { toaNhaId = room.ToaNhaId });
    }

    [HttpPost, ValidateAntiForgeryToken, ModuleAccess("PHONG_TRO", write: true)]
    [RequestSizeLimit(6 * 1024 * 1024), RequestFormLimits(MultipartBodyLengthLimit = 6 * 1024 * 1024)]
    public async Task<IActionResult> UploadImage(int id, IFormFile? file, CancellationToken cancellationToken)
    {
        if (CurrentAccountId() is not { } ownerId)
            return Forbid();
        if (await GetOwnedRoom(id) is null)
            return NotFound();

        PreparedRoomImage prepared;
        try
        {
            prepared = await imageStore.PrepareAsync(file, cancellationToken);
        }
        catch (InvalidDataException exception)
        {
            return BadRequest(new { message = exception.Message });
        }

        await db.Database.OpenConnectionAsync(cancellationToken);
        using var sqliteTransaction = ((SqliteConnection)db.Database.GetDbConnection()).BeginTransaction(deferred: false);
        await using var transaction = await db.Database.UseTransactionAsync(sqliteTransaction);
        StoredRoomImage? stored = null;
        try
        {
            var roomIsOwned = await db.PhongTros.AsNoTracking().AnyAsync(room => room.Id == id
                && db.ToaNhas.Any(building => building.Id == room.ToaNhaId
                    && building.ChuNhaId == ownerId && building.DangHoatDong), cancellationToken);
            if (!roomIsOwned)
                return NotFound();

            var count = await db.AnhPhongs.CountAsync(image => image.PhongId == id, cancellationToken);
            if (count >= RoomImageStore.MaxImagesPerRoom)
                return Conflict(new { message = "Phòng đã đủ 8 ảnh. Không thể tải thêm." });

            var nextOrder = (await db.AnhPhongs.Where(image => image.PhongId == id)
                .Select(image => (int?)image.ThuTu).MaxAsync(cancellationToken) ?? 0) + 1;
            stored = await imageStore.SaveAsync(id, prepared, cancellationToken);
            var image = new AnhPhong
            {
                PhongId = id,
                DuongDan = stored.OriginalPath,
                DuongDanAnhNho = stored.ThumbnailPath,
                ThuTu = nextOrder,
                NgayTao = DateTime.UtcNow
            };
            db.AnhPhongs.Add(image);
            await db.SaveChangesAsync(cancellationToken);
            await transaction!.CommitAsync(cancellationToken);
            return Ok(new
            {
                id = image.Id,
                order = image.ThuTu,
                originalPath = image.DuongDan,
                thumbnailPath = image.DuongDanAnhNho,
                count = count + 1
            });
        }
        catch (DbUpdateException)
        {
            try { await transaction!.RollbackAsync(CancellationToken.None); } catch { }
            imageStore.Delete(stored);
            return Conflict(new { message = "Không thể lưu ảnh vào phòng lúc này. Vui lòng thử lại." });
        }
        catch
        {
            try { await transaction!.RollbackAsync(CancellationToken.None); } catch { }
            imageStore.Delete(stored);
            throw;
        }
    }

    [HttpPost, ValidateAntiForgeryToken, ModuleAccess("PHONG_TRO", write: true)]
    public async Task<IActionResult> Delete(int id)
    {
        var room = await GetOwnedRoom(id);
        if (room is null) return NotFound();
        if (room.TrangThai is "DANG_THUE" or "DA_DAT_COC")
        {
            TempData["Error"] = "Không thể xóa phòng đang thuê hoặc đã đặt cọc.";
            return RedirectToAction(nameof(Index), new { toaNhaId = room.ToaNhaId });
        }
        db.PhongTros.Remove(room);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Không thể xóa phòng đang được hợp đồng hoặc dữ liệu khác sử dụng. Vui lòng tải lại danh sách.";
            return RedirectToAction(nameof(Index), new { toaNhaId = room.ToaNhaId });
        }
        TempData["Success"] = "Đã xóa phòng thành công.";
        return RedirectToAction(nameof(Index), new { toaNhaId = room.ToaNhaId });
    }

    private Task<PhongTro?> GetOwnedRoom(int id) => db.PhongTros.SingleOrDefaultAsync(room =>
        room.Id == id && db.ToaNhas.Any(building => building.Id == room.ToaNhaId &&
            building.ChuNhaId == CurrentAccountId() && building.DangHoatDong));

    private Task<List<AnhPhongQuanLyViewModel>> GetRoomImagesAsync(int roomId) => db.AnhPhongs.AsNoTracking()
        .Where(image => image.PhongId == roomId)
        .OrderBy(image => image.ThuTu)
        .Select(image => new AnhPhongQuanLyViewModel
        {
            Id = image.Id,
            DuongDan = image.DuongDan,
            DuongDanAnhNho = image.DuongDanAnhNho,
            ThuTu = image.ThuTu
        }).ToListAsync();

    private IQueryable<PhongTro> GetRoomQuery(int buildingId, TrangThaiPhong? status)
    {
        var query = db.PhongTros.AsNoTracking().Where(room => room.ToaNhaId == buildingId);
        if (status is not null && Enum.IsDefined(status.Value))
            query = query.Where(room => room.TrangThai == status.Value.ToString());

        return query;
    }

    private void BindRentPrice(string propertyName, string? displayValue, Action<long?> setPrice)
    {
        ModelState.Remove(propertyName);
        if (!TryParseRentPrice(displayValue, out var price))
        {
            setPrice(null);
            ModelState.AddModelError(propertyName, "Giá thuê phải là số nguyên từ 500.000 VND trở lên.");
            return;
        }

        setPrice(price);
        if (price < 500000)
            ModelState.AddModelError(propertyName, "Giá thuê tối thiểu là 500.000 VND.");
    }

    private static bool TryParseRentPrice(string? input, out long price)
    {
        price = 0;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var value = input.Trim();
        if (!Regex.IsMatch(value, @"^\d+$") && !Regex.IsMatch(value, @"^\d{1,3}(?:\.\d{3})+$"))
            return false;

        return long.TryParse(value.Replace(".", string.Empty), NumberStyles.None, CultureInfo.InvariantCulture, out price);
    }

    private static bool IsRoomCodeConflict(DbUpdateException exception) =>
        exception.GetBaseException() is SqliteException
        {
            SqliteErrorCode: 19,
            SqliteExtendedErrorCode: 2067
        };

    private static IReadOnlyList<SelectListItem> GetStatusOptions() =>
        Enum.GetValues<TrangThaiPhong>()
            .Select(status => new SelectListItem(StatusLabel(status), status.ToString()))
            .ToList();

    public static string StatusLabel(TrangThaiPhong status) => status switch
    {
        TrangThaiPhong.TRONG => "Trống",
        TrangThaiPhong.DA_DAT_COC => "Đã đặt cọc",
        TrangThaiPhong.DANG_THUE => "Đang thuê",
        TrangThaiPhong.NGUNG_CHO_THUE => "Ngừng cho thuê",
        _ => status.ToString()
    };

    private Task<List<SelectListItem>> GetBuildingOptions(int ownerId) => db.ToaNhas.AsNoTracking()
        .Where(building => building.ChuNhaId == ownerId && building.DangHoatDong)
        .OrderBy(building => building.TenToaNha)
        .Select(building => new SelectListItem(building.TenToaNha, building.Id.ToString()))
        .ToListAsync();

    private Task<ToaNha?> GetOwnedBuilding(int id) => db.ToaNhas
        .SingleOrDefaultAsync(building => building.Id == id && building.ChuNhaId == CurrentAccountId());

    private Task<List<SelectListItem>> GetManagerOptions() => db.TaiKhoans.AsNoTracking()
        .Where(account => account.VaiTro == "QUAN_LY" && account.DangHoatDong)
        .OrderBy(account => account.HoTen)
        .Select(account => new SelectListItem(account.HoTen, account.Id.ToString()))
        .ToListAsync();
}
