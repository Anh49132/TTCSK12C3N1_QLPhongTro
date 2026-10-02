using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QL_PhongTro.Data;
using QL_PhongTro.Models;
using QL_PhongTro.Services;
using Xunit;

namespace QL_PhongTro.Tests;

public sealed class TenantRequestCancellationTests : IDisposable
{
    private readonly string folder = Path.Combine(Path.GetTempPath(), "s209-" + Guid.NewGuid().ToString("N"));
    private readonly string database;
    private readonly string appPath;
    private const string Password = "DemoPass123!";
    private static readonly DateTime Now = new(2026, 10, 2, 8, 30, 0, DateTimeKind.Utc);

    public TenantRequestCancellationTests()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "QL_PhongTro", "Data")))
            directory = directory.Parent;
        appPath = Path.Combine(directory?.FullName ?? throw new InvalidOperationException("Repository not found."), "QL_PhongTro");
        Directory.CreateDirectory(folder);
        database = Path.Combine(folder, "test.sqlite");
        LocalDatabaseInitializer.Create(database, Path.Combine(appPath, "Data", "permissions.seed.json"));

        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO tai_khoan(id,ho_ten,email,so_dien_thoai,mat_khau,vai_tro,dang_hoat_dong,email_confirmed,ngay_tao,ngay_cap_nhat)
            VALUES (1,'Tenant One','tenant1@example.test','0900000001',$hash,'KHACH_THUE',1,1,'2026-01-01','2026-01-01'),
                   (2,'Tenant Two','tenant2@example.test','0900000002',$hash,'KHACH_THUE',1,1,'2026-01-01','2026-01-01');
            INSERT INTO khach_thue(id,tai_khoan_id,ho_ten,ngay_tao)
            VALUES (1,1,'Tenant One','2026-01-01'), (2,2,'Tenant Two','2026-01-01');
            INSERT INTO yeu_cau_thue(id,ma_yeu_cau,khach_thue_id,loai_yeu_cau,ngay_mong_muon,so_nguoi_du_kien,trang_thai,ngay_tao)
            VALUES (1,'YC-001',1,'XEM_PHONG','2026-10-05',1,'MOI','2026-10-01T08:00:00Z'),
                   (2,'YC-002',1,'XEM_PHONG','2026-10-06',1,'DA_HEN_LICH','2026-10-01T09:00:00Z'),
                   (3,'YC-003',1,'XEM_PHONG','2026-10-07',1,'TU_CHOI','2026-10-01T10:00:00Z'),
                   (4,'YC-004',2,'XEM_PHONG','2026-10-08',1,'MOI','2026-10-01T11:00:00Z');
            """;
        command.Parameters.AddWithValue("$hash", BCrypt.Net.BCrypt.HashPassword(Password, 4));
        command.ExecuteNonQuery();
    }

    private SqliteConnection Open()
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = database,
            ForeignKeys = true,
            Pooling = false
        }.ToString());
        connection.Open();
        return connection;
    }

    private AppDbContext Context(int actorId = 1) => new(
        new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(new SqliteConnectionStringBuilder
            {
                DataSource = database,
                ForeignKeys = true,
                Pooling = false
            }.ToString()).Options,
        new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimTypes.NameIdentifier, actorId.ToString()), new Claim(ClaimTypes.Role, "KHACH_THUE")],
                    "test"))
            }
        });

    private YeuCauThueService Service(AppDbContext db) => new(db, new FixedTimeProvider(Now));

    [Theory]
    [InlineData(TrangThaiYeuCauThue.Moi)]
    [InlineData(TrangThaiYeuCauThue.DaHenLich)]
    public async Task TenantCanCancelNewOrScheduledRequestAndListShowsSavedStatus(string initialStatus)
    {
        using var db = Context();
        var request = await db.YeuCauThues.SingleAsync(x => x.Id == (initialStatus == TrangThaiYeuCauThue.Moi ? 1 : 2));

        var result = await Service(db).HuyCuaKhachAsync(1, request.Id);

        Assert.Equal(KetQuaHuyYeuCauThue.DaHuy, result);
        Assert.Equal(TrangThaiYeuCauThue.DaHuy, request.TrangThai);
        Assert.Equal(1, request.PhienBan);
        Assert.Equal(1, request.NguoiXuLyId);
        Assert.Equal(Now, request.NgayXuLy);
        var item = Assert.Single(await Service(db).DanhSachCuaKhachAsync(1), x => x.Id == request.Id);
        Assert.Equal("Đã huỷ", item.TrangThaiHienThi);
        Assert.False(item.CoTheHuy);
        var audit = await db.NhatKyHoatDongs.SingleAsync(x => x.LoaiDoiTuong == "yeu_cau_thue" && x.DoiTuongId == request.Id);
        Assert.Equal("DOI_TRANG_THAI", audit.HanhDong);
    }

    [Theory]
    [InlineData(TrangThaiYeuCauThue.TuChoi)]
    [InlineData(TrangThaiYeuCauThue.DaDuyet)]
    [InlineData(TrangThaiYeuCauThue.DaHuy)]
    public async Task RequestOutsideCancellableStatesIsNotChanged(string currentStatus)
    {
        using var db = Context();
        await db.YeuCauThues.Where(x => x.Id == 1).ExecuteUpdateAsync(update =>
            update.SetProperty(x => x.TrangThai, currentStatus));

        var result = await Service(db).HuyCuaKhachAsync(1, 1);

        Assert.Equal(KetQuaHuyYeuCauThue.KhongTheHuy, result);
        using var verify = Context();
        Assert.Equal(currentStatus, (await verify.YeuCauThues.SingleAsync(x => x.Id == 1)).TrangThai);
        Assert.Empty(await verify.NhatKyHoatDongs.Where(x => x.LoaiDoiTuong == "yeu_cau_thue").ToListAsync());
    }

    [Fact]
    public async Task TenantCannotCancelAnotherTenantsRequest()
    {
        using var db = Context(actorId: 2);

        var result = await Service(db).HuyCuaKhachAsync(2, 1);

        Assert.Equal(KetQuaHuyYeuCauThue.KhongTimThay, result);
        Assert.Equal(TrangThaiYeuCauThue.Moi, await db.YeuCauThues.Where(x => x.Id == 1).Select(x => x.TrangThai).SingleAsync());
    }

    [Fact]
    public async Task StatusChangedBeforeConfirmationCannotBeCancelled()
    {
        using (var connection = Open())
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "UPDATE yeu_cau_thue SET trang_thai='TU_CHOI', phien_ban=phien_ban+1 WHERE id=1";
            command.ExecuteNonQuery();
        }
        using var db = Context();

        var result = await Service(db).HuyCuaKhachAsync(1, 1);

        Assert.Equal(KetQuaHuyYeuCauThue.KhongTheHuy, result);
        Assert.Equal(TrangThaiYeuCauThue.TuChoi, await db.YeuCauThues.Where(x => x.Id == 1).Select(x => x.TrangThai).SingleAsync());
    }

    [Fact]
    public async Task HttpCancellationUpdatesListAndRemovesCancelAction()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseContentRoot(appPath);
            builder.UseEnvironment("Development");
            builder.UseSetting("DatabasePath", database);
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.ConfigureServices(services => services.AddDataProtection().UseEphemeralDataProtectionProvider());
        });
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var loginPage = await client.GetStringAsync("/Account/Login");
        var loginToken = WebUtility.HtmlDecode(Regex.Match(loginPage,
            "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
        Assert.NotEmpty(loginToken);
        var login = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["TaiKhoanDangNhap"] = "tenant1@example.test",
            ["MatKhau"] = Password,
            ["__RequestVerificationToken"] = loginToken
        }));
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);

        var listBefore = WebUtility.HtmlDecode(await client.GetStringAsync("/YeuCauThue"));
        Assert.Contains("data-cancel-form", listBefore);
        Assert.True(listBefore.Contains("Mới", StringComparison.Ordinal), listBefore);
        var cancelToken = WebUtility.HtmlDecode(Regex.Match(listBefore,
            "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
        var response = await client.PostAsync("/YeuCauThue/Cancel", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["id"] = "1",
            ["__RequestVerificationToken"] = cancelToken
        }));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        var listAfter = WebUtility.HtmlDecode(await client.GetStringAsync("/YeuCauThue"));
        Assert.Contains("Đã huỷ", listAfter);
        Assert.Contains("Đã huỷ yêu cầu", listAfter);
        Assert.DoesNotContain("YC-004", listAfter);
        var cancelledRow = Regex.Match(listAfter, "<tr data-request-id=\"1\">(?<row>.*?)</tr>", RegexOptions.Singleline);
        Assert.True(cancelledRow.Success);
        Assert.DoesNotContain("data-cancel-form", cancelledRow.Groups["row"].Value);
        using var verify = Context();
        Assert.Equal(TrangThaiYeuCauThue.DaHuy, await verify.YeuCauThues.Where(x => x.Id == 1).Select(x => x.TrangThai).SingleAsync());
    }

    [Fact]
    public async Task VersionNineUpdateCreatesRequestTableAndPreservesExistingRows()
    {
        using (var connection = Open())
        using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                DROP TABLE yeu_cau_thue;
                DELETE FROM app_schema_version WHERE version=9;
                """;
            command.ExecuteNonQuery();
        }

        DatabaseUpdates.Update(database, Path.Combine(appPath, "Data", "permissions.seed.json"));

        using var verifyConnection = Open();
        using var verifyCommand = verifyConnection.CreateCommand();
        verifyCommand.CommandText = "SELECT COUNT(*) FROM tai_khoan";
        Assert.Equal(2L, verifyCommand.ExecuteScalar());
        verifyCommand.CommandText = "SELECT MAX(version) FROM app_schema_version";
        Assert.Equal(9L, verifyCommand.ExecuteScalar());
        verifyCommand.CommandText = "PRAGMA integrity_check";
        Assert.Equal("ok", verifyCommand.ExecuteScalar());
        verifyCommand.CommandText = "PRAGMA foreign_key_check";
        using var reader = verifyCommand.ExecuteReader();
        Assert.False(reader.Read());
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        Directory.Delete(folder, true);
    }

    private sealed class FixedTimeProvider(DateTime utcNow) : ITimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }
}
