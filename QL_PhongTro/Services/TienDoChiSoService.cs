using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Data;
using QL_PhongTro.ViewModels;

namespace QL_PhongTro.Services;

public sealed class TienDoChiSoService(AppDbContext db)
{
    public async Task<TienDoChiSoViewModel> XemAsync(int ownerId, int year, int month, CancellationToken ct)
    {
        ValidatePeriod(year, month);
        await EnsureLandlordAsync(ownerId, ct);

        var buildings = await db.ToaNhas.AsNoTracking()
            .Where(x => x.ChuNhaId == ownerId)
            .OrderBy(x => x.TenToaNha)
            .Select(x => new { x.Id, x.TenToaNha })
            .ToListAsync(ct);
        var first = new DateOnly(year, month, 1);
        var last = first.AddMonths(1).AddDays(-1);
        var lockedBuildings = await KyChiSoLockService.LayToaNhaDaKhoaAsync(
            db, buildings.Select(x => x.Id).ToArray(), year, month, ct);
        var leasedRooms = await GetLeasedRoomsAsync(ownerId, first, last, ct);
        var confirmedContracts = await GetConfirmedContractsAsync(leasedRooms, first, last, ct);

        var summaries = leasedRooms.GroupBy(x => x.Id).ToDictionary(
            group => group.Key,
            group =>
            {
                var roomGroups = group.GroupBy(x => x.RoomId);
                var total = roomGroups.Count();
                var confirmed = roomGroups.Count(room => room.All(x => confirmedContracts.Contains(x.ContractId)));
                return (Total: total, Confirmed: confirmed);
            });

        return new TienDoChiSoViewModel
        {
            Nam = year,
            Thang = month,
            ToaNhas = buildings.Select(building =>
            {
                var counts = summaries.GetValueOrDefault(building.Id);
                var total = counts.Total;
                return new TienDoChiSoToaNha(building.Id, building.TenToaNha, total, counts.Confirmed,
                    total - counts.Confirmed, lockedBuildings.Contains(building.Id));
            }).ToList()
        };
    }

    public async Task<PhongConThieuViewModel> PhongConThieuAsync(
        int ownerId, int buildingId, int year, int month, CancellationToken ct)
    {
        ValidatePeriod(year, month);
        await EnsureLandlordAsync(ownerId, ct);

        var building = await db.ToaNhas.AsNoTracking()
            .Where(x => x.Id == buildingId && x.ChuNhaId == ownerId)
            .Select(x => new { x.Id, x.TenToaNha })
            .SingleOrDefaultAsync(ct) ?? throw new UnauthorizedAccessException();

        var first = new DateOnly(year, month, 1);
        var last = first.AddMonths(1).AddDays(-1);
        var leasedRooms = await GetLeasedRoomsAsync(ownerId, first, last, ct);
        var confirmedContracts = await GetConfirmedContractsAsync(leasedRooms, first, last, ct);
        var missingRooms = leasedRooms
            .Where(x => x.Id == building.Id)
            .GroupBy(x => x.RoomId)
            .Where(room => room.Any(x => !confirmedContracts.Contains(x.ContractId)))
            .Select(room => room.First())
            .OrderBy(x => x.Floor)
            .ThenBy(x => x.RoomCode)
            .Select(x => new PhongConThieuItem(x.RoomId, x.RoomCode, x.Floor, x.ManagerName))
            .ToList();

        return new PhongConThieuViewModel(building.Id, building.TenToaNha, year, month, missingRooms);
    }

    private async Task EnsureLandlordAsync(int ownerId, CancellationToken ct)
    {
        if (!await db.TaiKhoans.AsNoTracking().AnyAsync(x => x.Id == ownerId
            && x.VaiTro == "CHU_NHA" && x.DangHoatDong && !x.IsDeleted, ct))
            throw new UnauthorizedAccessException();
    }

    private static void ValidatePeriod(int year, int month)
    {
        if (year is < 1900 or > 9998 || month is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "Kỳ theo dõi không hợp lệ.");
    }

    private Task<List<LeasedRoom>> GetLeasedRoomsAsync(
        int ownerId, DateOnly first, DateOnly last, CancellationToken ct) =>
        (from building in db.ToaNhas.AsNoTracking()
         join room in db.PhongTros.AsNoTracking() on building.Id equals room.ToaNhaId
         join contract in db.HopDongs.AsNoTracking() on room.Id equals contract.PhongId
         join period in db.KyHopDongs.AsNoTracking() on contract.Id equals period.HopDongId
         where building.ChuNhaId == ownerId
             && (contract.TrangThai == "DANG_HIEU_LUC" || contract.TrangThai == "DA_KET_THUC")
             && period.NgayBatDau <= last && period.NgayKetThuc >= first
             && (!contract.NgayTraPhong.HasValue || contract.NgayTraPhong.Value >= first)
         select new LeasedRoom(
             building.Id, room.Id, room.MaPhong, room.Tang, contract.Id,
             db.TaiKhoans.Where(account => account.Id == building.QuanLyId)
                 .Select(account => account.HoTen).FirstOrDefault()))
        .Distinct()
        .ToListAsync(ct);

    private async Task<HashSet<int>> GetConfirmedContractsAsync(
        List<LeasedRoom> leasedRooms, DateOnly first, DateOnly last, CancellationToken ct)
    {
        var serviceIds = await db.DichVus.AsNoTracking()
            .Where(x => x.MaDichVu == "DIEN" || x.MaDichVu == "NUOC")
            .Select(x => new { x.MaDichVu, x.Id })
            .ToListAsync(ct);
        var electricId = serviceIds.Where(x => x.MaDichVu == "DIEN").Select(x => (int?)x.Id).SingleOrDefault();
        var waterId = serviceIds.Where(x => x.MaDichVu == "NUOC").Select(x => (int?)x.Id).SingleOrDefault();
        var confirmedContracts = new HashSet<int>();
        var contractIds = leasedRooms.Select(x => x.ContractId).Distinct().ToList();

        if (electricId.HasValue && waterId.HasValue && contractIds.Count > 0)
        {
            var readings = await db.ChiSoDienNuocs.AsNoTracking()
                .Where(x => contractIds.Contains(x.HopDongId)
                    && (x.DichVuId == electricId.Value || x.DichVuId == waterId.Value)
                    && x.TuNgay <= first && x.DenNgay >= last)
                .Select(x => new { x.HopDongId, x.DichVuId })
                .ToListAsync(ct);
            var metersByContract = readings.GroupBy(x => x.HopDongId)
                .ToDictionary(x => x.Key, x => x.Select(r => r.DichVuId).ToHashSet());
            foreach (var contractId in contractIds)
                if (metersByContract.TryGetValue(contractId, out var meters)
                    && meters.Contains(electricId.Value) && meters.Contains(waterId.Value))
                    confirmedContracts.Add(contractId);
        }

        return confirmedContracts;
    }

    private sealed record LeasedRoom(
        int Id, int RoomId, string RoomCode, int Floor, int ContractId, string? ManagerName);
}
