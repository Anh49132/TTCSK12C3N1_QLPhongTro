using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Data;
using QL_PhongTro.ViewModels;

namespace QL_PhongTro.Services;

public sealed class TienDoChiSoService(AppDbContext db)
{
    public async Task<TienDoChiSoViewModel> XemAsync(int ownerId, int year, int month, CancellationToken ct)
    {
        if (year is < 1900 or > 9998 || month is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "Kỳ theo dõi không hợp lệ.");

        if (!await db.TaiKhoans.AsNoTracking().AnyAsync(x => x.Id == ownerId
            && x.VaiTro == "CHU_NHA" && x.DangHoatDong && !x.IsDeleted, ct))
            throw new UnauthorizedAccessException();

        var buildings = await db.ToaNhas.AsNoTracking()
            .Where(x => x.ChuNhaId == ownerId)
            .OrderBy(x => x.TenToaNha)
            .Select(x => new { x.Id, x.TenToaNha })
            .ToListAsync(ct);
        var first = new DateOnly(year, month, 1);
        var last = first.AddMonths(1).AddDays(-1);

        var leasedRooms = await (
            from building in db.ToaNhas.AsNoTracking()
            join room in db.PhongTros.AsNoTracking() on building.Id equals room.ToaNhaId
            join contract in db.HopDongs.AsNoTracking() on room.Id equals contract.PhongId
            join period in db.KyHopDongs.AsNoTracking() on contract.Id equals period.HopDongId
            where building.ChuNhaId == ownerId
                && (contract.TrangThai == "DANG_HIEU_LUC" || contract.TrangThai == "DA_KET_THUC")
                && period.NgayBatDau <= last && period.NgayKetThuc >= first
                && (!contract.NgayTraPhong.HasValue || contract.NgayTraPhong.Value >= first)
            select new { building.Id, RoomId = room.Id, ContractId = contract.Id })
            .Distinct()
            .ToListAsync(ct);

        var serviceIds = await db.DichVus.AsNoTracking()
            .Where(x => x.MaDichVu == "DIEN" || x.MaDichVu == "NUOC")
            .Select(x => new { x.MaDichVu, x.Id })
            .ToListAsync(ct);
        var electricId = serviceIds.Where(x => x.MaDichVu == "DIEN").Select(x => (int?)x.Id).SingleOrDefault();
        var waterId = serviceIds.Where(x => x.MaDichVu == "NUOC").Select(x => (int?)x.Id).SingleOrDefault();

        var contractIds = leasedRooms.Select(x => x.ContractId).Distinct().ToList();
        var confirmedContracts = new HashSet<int>();
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
                    total - counts.Confirmed);
            }).ToList()
        };
    }
}
