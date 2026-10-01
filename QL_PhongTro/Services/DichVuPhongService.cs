using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Data;
using QL_PhongTro.Models;
using QL_PhongTro.ViewModels;

namespace QL_PhongTro.Services;

public sealed class DichVuPhongService(AppDbContext db, DichVuService prices)
{
    // TODO(PO S2-01): Confirm snapshot-at-creation policy. Changing defaults never
    // backfills existing rooms. Keep this decision here for single and bulk creation.
    // Caller saves the room graph and audit in one transaction; no price is copied.
    public async Task GanMacDinhChoPhongMoiAsync(IReadOnlyCollection<PhongTro> rooms)
    {
        if (rooms.Any(x => db.Entry(x).State != EntityState.Added))
            throw new InvalidOperationException("Chỉ gán mặc định khi tạo phòng mới.");
        var buildingIds = rooms.Select(x => x.ToaNhaId).Distinct().ToArray();
        var defaults = await db.DichVuToaNhas.Where(x => buildingIds.Contains(x.ToaNhaId) && x.ApDungMacDinh).ToListAsync();
        foreach (var room in rooms)
            foreach (var service in defaults.Where(x => x.ToaNhaId == room.ToaNhaId))
                db.DichVuPhongs.Add(new DichVuPhong { Phong = room, DichVuToaNhaId = service.Id });
    }

    public async Task DatMacDinhAsync(int accountId, int buildingId, int serviceId, bool enabled)
    {
        await using var tx = await db.Database.BeginTransactionAsync();
        if (!await prices.SoHuuToaNhaAsync(accountId, buildingId)) throw new UnauthorizedAccessException();
        var catalog = await db.DichVuToaNhas.SingleOrDefaultAsync(x => x.ToaNhaId == buildingId && x.DichVuId == serviceId)
            ?? throw new InvalidOperationException("Không tìm thấy dịch vụ trong tòa nhà.");
        catalog.ApDungMacDinh = enabled;
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    private async Task<PhongTro> PhongAsync(int accountId, int roomId)
    {
        var room = await db.PhongTros.AsNoTracking().SingleOrDefaultAsync(x => x.Id == roomId);
        if (room is null || !await prices.SoHuuToaNhaAsync(accountId, room.ToaNhaId)) throw new UnauthorizedAccessException();
        return room;
    }

    public async Task DatDichVuAsync(int accountId, int roomId, int catalogId, bool enabled)
    {
        await using var tx = await db.Database.BeginTransactionAsync();
        var room = await PhongAsync(accountId, roomId);
        if (!await db.DichVuToaNhas.AnyAsync(x => x.Id == catalogId && x.ToaNhaId == room.ToaNhaId))
            throw new UnauthorizedAccessException();
        var existing = await db.DichVuPhongs.SingleOrDefaultAsync(x => x.PhongId == roomId && x.DichVuToaNhaId == catalogId);
        if (enabled && existing is null) db.DichVuPhongs.Add(new DichVuPhong { PhongId = roomId, DichVuToaNhaId = catalogId });
        if (!enabled && existing is not null) db.DichVuPhongs.Remove(existing);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task<DichVuPhongViewModel> XemAsync(int accountId, int roomId)
    {
        var room = await PhongAsync(accountId, roomId);
        var catalog = await db.DichVuToaNhas.AsNoTracking().Include(x => x.DichVu)
            .Where(x => x.ToaNhaId == room.ToaNhaId).OrderBy(x => x.DichVu.TenDichVu).ToListAsync();
        var selected = await db.DichVuPhongs.AsNoTracking().Where(x => x.PhongId == roomId)
            .Select(x => x.DichVuToaNhaId).ToListAsync();
        var model = new DichVuPhongViewModel { PhongId = room.Id, MaPhong = room.MaPhong, ToaNhaId = room.ToaNhaId };
        foreach (var item in catalog)
        {
            var price = await prices.LayDonGiaAsync(accountId, room.ToaNhaId, item.DichVuId, DichVuService.HomNay());
            model.DichVus.Add(new(item.Id, item.DichVu.TenDichVu, selected.Contains(item.Id), price));
        }
        return model;
    }
}
