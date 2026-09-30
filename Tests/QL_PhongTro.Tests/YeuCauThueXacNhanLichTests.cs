using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Data;
using QL_PhongTro.Models;
using QL_PhongTro.Services;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace QL_PhongTro.Tests;

// S2-08 lát 1: xác nhận lịch hẹn. Each test owns a disposable copy of the repository
// database migrated to the current version, so a failed assertion never leaks into another.
public class YeuCauThueXacNhanLichTests : IDisposable
{
    private const string ChuNhaHoTen = "Chủ nhà S2-08";
    private const string ChuNhaKhacHoTen = "Chủ nhà khác S2-08";
    private const string KhachHoTen = "Khách S2-08";

    /// <summary>
    /// The audit interceptor resolves the acting account from the HTTP principal, so a
    /// service-level test has to present one. Token contents do not matter, only the id.
    /// </summary>
    private sealed class StubHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; }

        public void DangNhap(int accountId, string vaiTro)
        {
            var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, accountId.ToString()),
                new Claim(ClaimTypes.Role, vaiTro)
            ], "Test");
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        }
    }

    private readonly string _dbPath;
    private readonly AppDbContext _db;
    private readonly StubHttpContextAccessor _accessor = new();
    private readonly MockTimeProvider _clock = new();
    private readonly YeuCauThueService _service;

    private int _chuNhaId;
    private int _chuNhaKhacId;
    private int _khachAccountId;
    private int _khachId;
    private int _phongId;
    private int _toaNhaId;
    private DateTime _hienTai;

    public YeuCauThueXacNhanLichTests()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "QL_PhongTro", "Data")))
            directory = directory.Parent;
        var appPath = Path.Combine(directory?.FullName ?? throw new Exception("Repository not found"), "QL_PhongTro");

        _dbPath = Path.Combine(Path.GetTempPath(), $"s2_08_l1_{Guid.NewGuid():N}.sqlite");
        using (var original = new SqliteConnection("Data Source=" + Path.Combine(appPath, "Data", "local-dev.sqlite") + ";Mode=ReadOnly"))
        using (var copy = new SqliteConnection("Data Source=" + _dbPath))
        {
            original.Open();
            copy.Open();
            original.BackupDatabase(copy);
        }
        DatabaseUpdates.Update(_dbPath, Path.Combine(appPath, "Data", "permissions.seed.json"));

        _db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite("Data Source=" + _dbPath).Options, _accessor);
        _hienTai = new DateTime(2026, 3, 10, 3, 0, 0, DateTimeKind.Utc); // 10:00 Vietnam
        _clock.UtcNow = _hienTai;
        _service = new YeuCauThueService(_db, _clock);
        TaoDuLieu();
        _accessor.DangNhap(_chuNhaId, "CHU_NHA");
    }

    public void Dispose()
    {
        _db.Dispose();
        SqliteConnection.ClearAllPools();
        if (File.Exists(_dbPath)) File.Delete(_dbPath);
    }

    private long Insert(string sql, params (string Name, object? Value)[] parameters)
    {
        var connection = _db.Database.GetDbConnection();
        using var command = connection.CreateCommand();
        if (connection.State != System.Data.ConnectionState.Open) connection.Open();
        command.CommandText = sql;
        foreach (var (name, value) in parameters)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }
        command.ExecuteNonQuery();
        using var idCommand = connection.CreateCommand();
        idCommand.CommandText = "SELECT last_insert_rowid()";
        return Convert.ToInt64(idCommand.ExecuteScalar());
    }

    // Seed rows go in through raw SQL: creating an account is itself an audited write, and
    // there is no acting principal to audit it against yet.
    private int TaiKhoanMoi(string hoTen, string vaiTro, string email)
    {
        var now = _hienTai.ToString("yyyy-MM-dd HH:mm:ss");
        return (int)Insert(
            "INSERT INTO tai_khoan(ho_ten,email,so_dien_thoai,mat_khau,vai_tro,dang_hoat_dong,is_staff,is_superuser,"
                + "ngay_tao,ngay_cap_nhat,must_change_password,email_confirmed,is_deleted) "
                + "VALUES ($ten,$email,$sdt,'x',$vai_tro,1,0,0,$now,$now,0,1,0)",
            ("$ten", hoTen), ("$email", email), ("$sdt", "09" + Random.Shared.Next(10000000, 99999999)),
            ("$vai_tro", vaiTro), ("$now", now));
    }

    private void TaoDuLieu()
    {
        _chuNhaId = TaiKhoanMoi(ChuNhaHoTen, "CHU_NHA", "s2_08_l1_chu_nha@test.local");
        _chuNhaKhacId = TaiKhoanMoi(ChuNhaKhacHoTen, "CHU_NHA", "s2_08_l1_chu_nha_khac@test.local");
        _khachAccountId = TaiKhoanMoi(KhachHoTen, "KHACH_THUE", "s2_08_l1_khach@test.local");

        var now = _hienTai.ToString("yyyy-MM-dd HH:mm:ss");
        _toaNhaId = (int)Insert(
            "INSERT INTO toa_nha(chu_nha_id,ten_toa_nha,dia_chi,ngay_chot_hang_thang,dang_hoat_dong) "
                + "VALUES ($chu,'Tòa nhà S2-08','1 Đường S2-08',1,1)", ("$chu", _chuNhaId));
        Insert("INSERT INTO toa_nha(chu_nha_id,ten_toa_nha,dia_chi,ngay_chot_hang_thang,dang_hoat_dong) "
                + "VALUES ($chu,'Tòa nhà khác S2-08','2 Đường S2-08',1,1)", ("$chu", _chuNhaKhacId));

        _phongId = (int)Insert(
            "INSERT INTO phong_tro(toa_nha_id,ma_phong,tang,dien_tich,gia_thue,tien_coc_du_kien,so_nguoi_toi_da,"
                + "trang_thai,ngay_tao,phien_ban) VALUES ($toa,'P-S2-08',1,20,3000000,1000000,3,'TRONG',$now,0)",
            ("$toa", _toaNhaId), ("$now", now));

        _khachId = (int)Insert("INSERT INTO khach_thue(tai_khoan_id,ho_ten,ngay_tao) VALUES ($tk,$ten,$now)",
            ("$tk", _khachAccountId), ("$ten", KhachHoTen), ("$now", now));
    }

    private void DangNhap(int accountId, string vaiTro) => _accessor.DangNhap(accountId, vaiTro);

    private int TaoYeuCau(string trangThai = YeuCauThueTrangThai.Moi, DateTime? lichHenUtc = null)
    {
        var yeuCau = new YeuCauThue
        {
            MaYeuCau = "YC" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant(),
            PhongId = _phongId,
            KhachThueId = _khachId,
            LoaiYeuCau = YeuCauThueTrangThai.LoaiXemPhong,
            NgayMongMuon = new DateOnly(2026, 4, 1),
            SoNguoiDuKien = 2,
            TrangThai = trangThai,
            LichHen = lichHenUtc,
            NgayTao = _hienTai
        };
        _db.YeuCauThues.Add(yeuCau);
        _db.SaveChanges();
        return yeuCau.Id;
    }

    private async Task<YeuCauThue> LaiAsync(int id)
    {
        _db.ChangeTracker.Clear();
        return await _db.YeuCauThues.AsNoTracking().SingleAsync(x => x.Id == id);
    }

    [Fact]
    public async Task XacNhanLich_ChuyenSangDaHenLich_GhiLichSuVaThongBao()
    {
        var id = TaoYeuCau();
        var lichHen = _hienTai.AddDays(1);

        await _service.XacNhanLichAsync(id, _chuNhaId, lichHen, default);

        var yeuCau = await LaiAsync(id);
        Assert.Equal(YeuCauThueTrangThai.DaHenLich, yeuCau.TrangThai);
        Assert.Equal(lichHen, yeuCau.LichHen);
        Assert.Equal(_chuNhaId, yeuCau.NguoiXuLyId);
        Assert.Equal(_hienTai, yeuCau.NgayXuLy);
        Assert.Equal(1, yeuCau.PhienBan);

        var muc = Assert.Single(await _db.YeuCauThueLichSus.AsNoTracking().Where(x => x.YeuCauThueId == id).ToListAsync());
        Assert.Equal(HanhDongYeuCau.XacNhan, muc.HanhDong);
        Assert.Equal(YeuCauThueTrangThai.Moi, muc.TrangThaiCu);
        Assert.Equal(YeuCauThueTrangThai.DaHenLich, muc.TrangThaiMoi);
        Assert.Equal(lichHen, muc.LichHenMoi);
        Assert.Equal("CHU_NHA", muc.VaiTroLucThucHien);

        var thongBao = await _db.ThongBaos.AsNoTracking().SingleAsync();
        Assert.Equal(LoaiThongBao.YeuCauXacNhan, thongBao.Loai);
        Assert.Equal(_khachAccountId, thongBao.NguoiNhanId);
        Assert.Equal($"/YeuCauThue/Detail/{id}", thongBao.DuongDan);
    }

    [Fact]
    public async Task XacNhanLich_TuChoiLichHenTrongQuaKhong()
    {
        var id = TaoYeuCau();
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.XacNhanLichAsync(id, _chuNhaId, _hienTai.AddMinutes(-5), default));
        Assert.Equal(YeuCauThueTrangThai.Moi, (await LaiAsync(id)).TrangThai);
    }

    [Fact]
    public async Task XacNhanLich_KhongPhaiChuNhaPhong_ThenTuChoi()
    {
        var id = TaoYeuCau();
        // KHACH_THUE holds WRITE on YEU_CAU_THUE, so the service must check ownership itself.
        DangNhap(_khachAccountId, "KHACH_THUE");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.XacNhanLichAsync(id, _khachAccountId, _hienTai.AddDays(1), default));
        Assert.Equal(YeuCauThueTrangThai.Moi, (await LaiAsync(id)).TrangThai);
    }

    [Fact]
    public async Task XacNhanLich_ChuNhaCuaToaNhaKhac_ThenTuChoi()
    {
        var id = TaoYeuCau();
        DangNhap(_chuNhaKhacId, "CHU_NHA");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.XacNhanLichAsync(id, _chuNhaKhacId, _hienTai.AddDays(1), default));
    }

    [Fact]
    public async Task XacNhanLich_YeuCauDaCoLich_ThenTuChoi()
    {
        var id = TaoYeuCau(YeuCauThueTrangThai.DaHenLich, _hienTai.AddDays(1));
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.XacNhanLichAsync(id, _chuNhaId, _hienTai.AddDays(2), default));
    }

    [Fact]
    public async Task XacNhanLich_YeuCauKhongTonTai_ThenBaoKhongTimThay()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.XacNhanLichAsync(999999, _chuNhaId, _hienTai.AddDays(1), default));
    }

    [Fact]
    public async Task QuyenXem_ChuNhaVaKhachDuocXem_ChuNhaKhacKhong()
    {
        var id = TaoYeuCau();
        Assert.True(await _service.QuyenXemAsync(_chuNhaId, id, default));
        Assert.True(await _service.QuyenXemAsync(_khachAccountId, id, default));
        Assert.False(await _service.QuyenXemAsync(_chuNhaKhacId, id, default));
    }
}
