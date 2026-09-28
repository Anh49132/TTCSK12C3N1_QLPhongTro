using System.Diagnostics;
using System.Globalization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro.Models;

namespace QL_PhongTro.Data;

public static class StagingDemoSeeder
{
    private const int SchemaVersion = 1;
    private const int ContractCount = 20;
    private const int InvoicePeriodCount = 3;
    private const long MonthlyRent = 3_000_000;
    private static readonly string[] Roles = ["KHACH_THUE", "CHU_NHA", "QUAN_LY", "ADMIN"];

    public static async Task ResetAsync(string databasePath, string permissionSeedPath, string demoPassword)
    {
        var stopwatch = Stopwatch.StartNew();
        databasePath = Path.GetFullPath(databasePath);
        Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);

        var existingTables = ReadUserTables(databasePath);
        var isNewDatabase = existingTables.Count == 0;
        if (!isNewDatabase && !existingTables.Contains("staging_seed_meta"))
            throw new InvalidOperationException("The selected SQLite file is not a tagged staging database; refusing to erase it.");
        if (!isNewDatabase && ReadSchemaVersion(databasePath) != SchemaVersion)
            throw new InvalidOperationException("Unsupported staging schema version; no data was changed.");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(new SqliteConnectionStringBuilder
            {
                DataSource = databasePath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                ForeignKeys = true
            }.ToString())
            .Options;
        await using (var db = new AppDbContext(options))
            await db.Database.EnsureCreatedAsync();

        if (isNewDatabase)
        {
            PermissionSchemaInitializer.Initialize(databasePath, permissionSeedPath);
            CreateStagingSchema(databasePath);
        }
        else
        {
            ValidateStagingSchema(databasePath);
        }
        if (!ReadUserTables(databasePath).Contains("account_session_version"))
            PasswordSchemaInitializer.Initialize(databasePath);

        using var connection = Open(databasePath);
        using var transaction = connection.BeginTransaction();
        foreach (var table in new[]
        {
            "password_reset_token", "password_reset_request", "account_session_version",
            "thanh_toan", "chi_tiet_hoa_don", "hoa_don", "ky_hop_dong", "hop_dong",
            "phong_tro", "toa_nha", "khach_thue", "tai_khoan"
        })
        {
            if (ReadUserTables(databasePath).Contains(table))
                Execute(connection, transaction, "DELETE FROM " + table);
        }
        Execute(connection, transaction, "DELETE FROM sqlite_sequence WHERE name IN ('tai_khoan','khach_thue','toa_nha','phong_tro','hop_dong','ky_hop_dong','hoa_don','chi_tiet_hoa_don','thanh_toan')");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(demoPassword);
        var accountIds = InsertAccounts(connection, transaction, passwordHash);
        var profileIds = InsertTenantProfiles(connection, transaction, accountIds);
        var buildingIds = InsertBuildings(connection, transaction, accountIds);
        var roomIds = InsertRooms(connection, transaction, buildingIds);
        InsertContractsAndInvoices(connection, transaction, accountIds, profileIds, roomIds);
        AssertCounts(connection, transaction);
        transaction.Commit();
        stopwatch.Stop();

        if (stopwatch.Elapsed >= TimeSpan.FromMinutes(2))
            throw new InvalidOperationException("Staging seed completed but exceeded the 2-minute acceptance target.");

        Console.WriteLine("Staging data reset complete.");
        Console.WriteLine("Buildings: 2; rooms: 30; contracts: 20; invoice periods: 3; invoices: 60.");
        Console.WriteLine("Invoice payment states: paid in full 20; paid partially 20; overdue 20.");
        Console.WriteLine("Demo accounts: " + string.Join(", ", Roles.Select(role => "demo-" + role.ToLowerInvariant() + "@staging.test")));
        Console.WriteLine("Shared password is the configured Staging:DemoPassword secret.");
        Console.WriteLine("Elapsed: " + stopwatch.Elapsed.TotalSeconds.ToString("F2", CultureInfo.InvariantCulture) + " seconds.");
    }

    private static HashSet<string> ReadUserTables(string path)
    {
        if (!File.Exists(path)) return [];
        using var connection = Open(path);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'";
        using var reader = command.ExecuteReader();
        var tables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (reader.Read()) tables.Add(reader.GetString(0));
        return tables;
    }

    private static int ReadSchemaVersion(string path)
    {
        using var connection = Open(path);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT SchemaVersion FROM staging_seed_meta WHERE Id=1";
        return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
    }

    private static void CreateStagingSchema(string path)
    {
        using var connection = Open(path);
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE staging_seed_meta (
                Id INTEGER NOT NULL PRIMARY KEY CHECK (Id=1),
                SchemaVersion INTEGER NOT NULL,
                InitializedAtUtc TEXT NOT NULL
            );
            CREATE TABLE hop_dong (
                id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ma_hop_dong TEXT NOT NULL UNIQUE,
                phong_id INTEGER NOT NULL REFERENCES phong_tro(id) ON DELETE RESTRICT,
                khach_dung_ten_id INTEGER NOT NULL REFERENCES khach_thue(id) ON DELETE RESTRICT,
                tien_coc_thoa_thuan INTEGER NOT NULL,
                trang_thai TEXT NOT NULL,
                ngay_kich_hoat TEXT NOT NULL,
                ngay_tra_phong TEXT,
                nguoi_lap_id INTEGER NOT NULL REFERENCES tai_khoan(id) ON DELETE RESTRICT,
                ngay_tao TEXT NOT NULL,
                phien_ban INTEGER NOT NULL DEFAULT 0
            );
            CREATE TABLE ky_hop_dong (
                id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                hop_dong_id INTEGER NOT NULL REFERENCES hop_dong(id) ON DELETE RESTRICT,
                so_thu_tu INTEGER NOT NULL,
                ngay_bat_dau TEXT NOT NULL,
                ngay_ket_thuc TEXT NOT NULL,
                so_thang INTEGER NOT NULL,
                gia_thue INTEGER NOT NULL,
                nguoi_lap_id INTEGER NOT NULL REFERENCES tai_khoan(id) ON DELETE RESTRICT,
                ngay_tao TEXT NOT NULL,
                UNIQUE (hop_dong_id, so_thu_tu)
            );
            CREATE TABLE hoa_don (
                id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ma_hoa_don TEXT NOT NULL UNIQUE,
                hop_dong_id INTEGER NOT NULL REFERENCES hop_dong(id) ON DELETE RESTRICT,
                thang INTEGER NOT NULL,
                nam INTEGER NOT NULL,
                tu_ngay TEXT NOT NULL,
                den_ngay TEXT NOT NULL,
                ngay_chot TEXT NOT NULL,
                so_nguoi_tinh_phi INTEGER NOT NULL,
                loai_hoa_don TEXT NOT NULL DEFAULT 'DINH_KY',
                ngay_lap TEXT NOT NULL,
                ngay_phat_hanh TEXT,
                han_thanh_toan TEXT NOT NULL,
                tong_tien INTEGER NOT NULL,
                trang_thai TEXT NOT NULL,
                nguoi_lap_id INTEGER NOT NULL REFERENCES tai_khoan(id) ON DELETE RESTRICT,
                phien_ban INTEGER NOT NULL DEFAULT 0,
                UNIQUE (hop_dong_id, nam, thang)
            );
            CREATE TABLE chi_tiet_hoa_don (
                id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                hoa_don_id INTEGER NOT NULL REFERENCES hoa_don(id) ON DELETE RESTRICT,
                so_thu_tu INTEGER NOT NULL,
                ky_hop_dong_id INTEGER NOT NULL REFERENCES ky_hop_dong(id) ON DELETE RESTRICT,
                loai_khoan TEXT NOT NULL,
                ten_khoan TEXT NOT NULL,
                so_luong NUMERIC NOT NULL,
                don_gia INTEGER NOT NULL,
                thanh_tien INTEGER NOT NULL,
                UNIQUE (hoa_don_id, so_thu_tu)
            );
            CREATE TABLE thanh_toan (
                id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ma_thanh_toan TEXT NOT NULL UNIQUE,
                khoa_chong_trung TEXT NOT NULL UNIQUE,
                hoa_don_id INTEGER NOT NULL REFERENCES hoa_don(id) ON DELETE RESTRICT,
                so_tien INTEGER NOT NULL,
                ngay_thanh_toan TEXT NOT NULL,
                hinh_thuc TEXT NOT NULL,
                trang_thai TEXT NOT NULL,
                nguoi_bao_id INTEGER NOT NULL REFERENCES tai_khoan(id) ON DELETE RESTRICT,
                ngay_bao TEXT NOT NULL
            );
            INSERT INTO staging_seed_meta (Id, SchemaVersion, InitializedAtUtc)
            VALUES (1, 1, strftime('%Y-%m-%dT%H:%M:%fZ','now'));
            """;
        command.ExecuteNonQuery();
    }

    private static void ValidateStagingSchema(string path)
    {
        var tables = ReadUserTables(path);
        var required = new[] { "tai_khoan", "khach_thue", "toa_nha", "phong_tro", "app_role", "app_module", "role_permission", "hop_dong", "ky_hop_dong", "hoa_don", "chi_tiet_hoa_don", "thanh_toan" };
        var missing = required.Where(table => !tables.Contains(table)).ToArray();
        if (missing.Length > 0)
            throw new InvalidOperationException("Staging schema is incomplete; no data was reset. Missing: " + string.Join(", ", missing));
    }

    private static Dictionary<string, int> InsertAccounts(SqliteConnection connection, SqliteTransaction transaction, string passwordHash)
    {
        var ids = new Dictionary<string, int>(StringComparer.Ordinal);
        var now = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
        for (var index = 0; index < Roles.Length + ContractCount - 1; index++)
        {
            var role = index == 0 ? "KHACH_THUE" : index switch
            {
                1 => "CHU_NHA",
                2 => "QUAN_LY",
                3 => "ADMIN",
                _ => "KHACH_THUE"
            };
            var email = index < Roles.Length
                ? "demo-" + role.ToLowerInvariant() + "@staging.test"
                : $"tenant-{index - Roles.Length + 2:D2}@staging.test";
            var phone = "090" + (index + 1).ToString("D7", CultureInfo.InvariantCulture);
            Execute(connection, transaction, """
                INSERT INTO tai_khoan(ho_ten,email,so_dien_thoai,mat_khau,vai_tro,dang_hoat_dong,is_staff,is_superuser,ngay_tao,ngay_cap_nhat)
                VALUES ($name,$email,$phone,$hash,$role,1,0,0,$now,$now)
                """, ("$name", "Demo " + role + (index >= Roles.Length ? " " + (index - Roles.Length + 2).ToString("D2", CultureInfo.InvariantCulture) : "")), ("$email", email), ("$phone", phone), ("$hash", passwordHash), ("$role", role), ("$now", now));
            var id = Convert.ToInt32(Scalar(connection, transaction, "SELECT last_insert_rowid()"), CultureInfo.InvariantCulture);
            if (index < Roles.Length) ids[role] = id;
            if (index == 0) ids["TENANT_01"] = id;
            else if (index >= Roles.Length) ids["TENANT_" + (index - Roles.Length + 2).ToString("D2", CultureInfo.InvariantCulture)] = id;
        }
        return ids;
    }

    private static List<int> InsertTenantProfiles(SqliteConnection connection, SqliteTransaction transaction, Dictionary<string, int> accounts)
    {
        var ids = new List<int>(ContractCount);
        var now = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
        for (var index = 1; index <= ContractCount; index++)
        {
            var accountId = accounts[index == 1 ? "TENANT_01" : "TENANT_" + index.ToString("D2", CultureInfo.InvariantCulture)];
            Execute(connection, transaction, "INSERT INTO khach_thue(tai_khoan_id,ho_ten,ngay_tao) VALUES ($account,$name,$now)", ("$account", accountId), ("$name", "Khách thuê mẫu " + index.ToString("D2", CultureInfo.InvariantCulture)), ("$now", now));
            ids.Add(Convert.ToInt32(Scalar(connection, transaction, "SELECT last_insert_rowid()"), CultureInfo.InvariantCulture));
        }
        return ids;
    }

    private static List<int> InsertBuildings(SqliteConnection connection, SqliteTransaction transaction, Dictionary<string, int> accounts)
    {
        var ids = new List<int>(2);
        for (var index = 1; index <= 2; index++)
        {
            Execute(connection, transaction, """
                INSERT INTO toa_nha(chu_nha_id,quan_ly_id,ten_toa_nha,dia_chi,phuong_xa,quan_huyen,tinh_thanh,so_tang,ngay_chot_hang_thang,dang_hoat_dong,ghi_chu)
                VALUES ($owner,$manager,$name,$address,'Phường mẫu','Quận mẫu','TP. Hồ Chí Minh',5,1,1,'Dữ liệu staging tổng hợp')
                """, ("$owner", accounts["CHU_NHA"]), ("$manager", accounts["QUAN_LY"]), ("$name", "Tòa nhà mẫu " + index.ToString("D2", CultureInfo.InvariantCulture)), ("$address", "" + index.ToString(CultureInfo.InvariantCulture) + " Đường Demo, TP. Hồ Chí Minh"));
            ids.Add(Convert.ToInt32(Scalar(connection, transaction, "SELECT last_insert_rowid()"), CultureInfo.InvariantCulture));
        }
        return ids;
    }

    private static List<int> InsertRooms(SqliteConnection connection, SqliteTransaction transaction, IReadOnlyList<int> buildings)
    {
        var ids = new List<int>(30);
        var now = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
        for (var index = 0; index < 30; index++)
        {
            var buildingIndex = index / 15;
            var roomInBuilding = index % 15 + 1;
            var rented = roomInBuilding <= 10;
            Execute(connection, transaction, """
                INSERT INTO phong_tro(toa_nha_id,ma_phong,tang,loai_phong,dien_tich,gia_thue,tien_coc_du_kien,so_nguoi_toi_da,trang_thai,mo_ta,ngay_tao,phien_ban)
                VALUES ($building,$code,1,'Studio',25.0,$rent,$deposit,2,$status,'Phòng dữ liệu staging', $now,0)
                """, ("$building", buildings[buildingIndex]), ("$code", roomInBuilding.ToString("D2", CultureInfo.InvariantCulture)), ("$rent", MonthlyRent), ("$deposit", MonthlyRent), ("$status", rented ? "DANG_THUE" : "TRONG"), ("$now", now));
            ids.Add(Convert.ToInt32(Scalar(connection, transaction, "SELECT last_insert_rowid()"), CultureInfo.InvariantCulture));
        }
        return ids;
    }

    private static void InsertContractsAndInvoices(SqliteConnection connection, SqliteTransaction transaction, Dictionary<string, int> accounts, IReadOnlyList<int> profiles, IReadOnlyList<int> rooms)
    {
        var now = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var currentMonth = new DateOnly(today.Year, today.Month, 1);
        var contractStart = currentMonth.AddMonths(-2);
        var contractEnd = contractStart.AddMonths(12).AddDays(-1);
        for (var index = 0; index < ContractCount; index++)
        {
            Execute(connection, transaction, """
                INSERT INTO hop_dong(ma_hop_dong,phong_id,khach_dung_ten_id,tien_coc_thoa_thuan,trang_thai,ngay_kich_hoat,ngay_tra_phong,nguoi_lap_id,ngay_tao,phien_ban)
            VALUES ($code,$room,$tenant,$deposit,'DANG_HIEU_LUC',$start,NULL,$owner,$now,0)
            """, ("$code", "HD-STG-" + (index + 1).ToString("D3", CultureInfo.InvariantCulture)), ("$room", rooms[index]), ("$tenant", profiles[index]), ("$deposit", MonthlyRent), ("$start", contractStart.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).ToString("O", CultureInfo.InvariantCulture)), ("$owner", accounts["CHU_NHA"]), ("$now", now));
            var contractId = Convert.ToInt32(Scalar(connection, transaction, "SELECT last_insert_rowid()"), CultureInfo.InvariantCulture);
            Execute(connection, transaction, """
                INSERT INTO ky_hop_dong(hop_dong_id,so_thu_tu,ngay_bat_dau,ngay_ket_thuc,so_thang,gia_thue,nguoi_lap_id,ngay_tao)
                VALUES ($contract,1,$start,$end,12,$rent,$owner,$now)
                """, ("$contract", contractId), ("$start", contractStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)), ("$end", contractEnd.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)), ("$rent", MonthlyRent), ("$owner", accounts["CHU_NHA"]), ("$now", now));
            var termId = Convert.ToInt32(Scalar(connection, transaction, "SELECT last_insert_rowid()"), CultureInfo.InvariantCulture);

            for (var period = 0; period < InvoicePeriodCount; period++)
            {
                var month = currentMonth.AddMonths(period - 2);
                var firstDay = new DateOnly(month.Year, month.Month, 1);
                var lastDay = new DateOnly(month.Year, month.Month, DateTime.DaysInMonth(month.Year, month.Month));
                var dueDate = period switch
                {
                    0 => firstDay.AddMonths(1).AddDays(6),
                    1 => today.AddDays(-1),
                    _ => today.AddDays(7)
                };
                var issuedAt = period == InvoicePeriodCount - 1 ? DateTime.UtcNow : firstDay.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                Execute(connection, transaction, """
                    INSERT INTO hoa_don(ma_hoa_don,hop_dong_id,thang,nam,tu_ngay,den_ngay,ngay_chot,so_nguoi_tinh_phi,loai_hoa_don,ngay_lap,ngay_phat_hanh,han_thanh_toan,tong_tien,trang_thai,nguoi_lap_id,phien_ban)
                    VALUES ($code,$contract,$month,$year,$start,$end,$end,1,'DINH_KY',$issued,$issued,$due,$total,'DA_PHAT_HANH',$owner,0)
                    """, ("$code", $"HDN-STG-{index + 1:D3}-{month:yyyyMM}"), ("$contract", contractId), ("$month", month.Month), ("$year", month.Year), ("$start", firstDay.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)), ("$end", lastDay.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)), ("$issued", issuedAt.ToString("O", CultureInfo.InvariantCulture)), ("$due", dueDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)), ("$total", MonthlyRent), ("$owner", accounts["CHU_NHA"]));
                var invoiceId = Convert.ToInt32(Scalar(connection, transaction, "SELECT last_insert_rowid()"), CultureInfo.InvariantCulture);
                Execute(connection, transaction, """
                    INSERT INTO chi_tiet_hoa_don(hoa_don_id,so_thu_tu,ky_hop_dong_id,loai_khoan,ten_khoan,so_luong,don_gia,thanh_tien)
                    VALUES ($invoice,1,$term,'TIEN_PHONG','Tiền thuê phòng',1,$rent,$rent)
                    """, ("$invoice", invoiceId), ("$term", termId), ("$rent", MonthlyRent));

                if (period == 1) continue;
                var amount = period == 0 ? MonthlyRent : MonthlyRent / 2;
                var paymentDate = (period == 0 ? dueDate.AddDays(-1) : today).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).ToString("O", CultureInfo.InvariantCulture);
                Execute(connection, transaction, """
                    INSERT INTO thanh_toan(ma_thanh_toan,khoa_chong_trung,hoa_don_id,so_tien,ngay_thanh_toan,hinh_thuc,trang_thai,nguoi_bao_id,ngay_bao)
                    VALUES ($code,$key,$invoice,$amount,$paid,'CHUYEN_KHOAN','DA_XAC_NHAN',$tenant,$paid)
                    """, ("$code", $"TT-STG-{index + 1:D3}-{period + 1}"), ("$key", $"staging:{index + 1}:{period + 1}"), ("$invoice", invoiceId), ("$amount", amount), ("$paid", paymentDate), ("$tenant", profiles[index]));
            }
        }
    }

    private static void AssertCounts(SqliteConnection connection, SqliteTransaction transaction)
    {
        AssertCount(connection, transaction, "toa_nha", 2);
        AssertCount(connection, transaction, "phong_tro", 30);
        AssertCount(connection, transaction, "hop_dong", ContractCount);
        AssertCount(connection, transaction, "hoa_don", ContractCount * InvoicePeriodCount);
        var stateCounts = new Dictionary<string, long>(StringComparer.Ordinal)
        {
            ["PAID"] = Convert.ToInt64(Scalar(connection, transaction, "SELECT COUNT(*) FROM hoa_don i WHERE (SELECT COALESCE(SUM(p.so_tien),0) FROM thanh_toan p WHERE p.hoa_don_id=i.id AND p.trang_thai='DA_XAC_NHAN') >= i.tong_tien"), CultureInfo.InvariantCulture),
            ["PARTIAL"] = Convert.ToInt64(Scalar(connection, transaction, "SELECT COUNT(*) FROM hoa_don i WHERE (SELECT COALESCE(SUM(p.so_tien),0) FROM thanh_toan p WHERE p.hoa_don_id=i.id AND p.trang_thai='DA_XAC_NHAN') > 0 AND (SELECT COALESCE(SUM(p.so_tien),0) FROM thanh_toan p WHERE p.hoa_don_id=i.id AND p.trang_thai='DA_XAC_NHAN') < i.tong_tien"), CultureInfo.InvariantCulture),
            ["OVERDUE"] = Convert.ToInt64(Scalar(connection, transaction, "SELECT COUNT(*) FROM hoa_don i WHERE (SELECT COALESCE(SUM(p.so_tien),0) FROM thanh_toan p WHERE p.hoa_don_id=i.id AND p.trang_thai='DA_XAC_NHAN') < i.tong_tien AND date(i.han_thanh_toan) < date('now')"), CultureInfo.InvariantCulture)
        };
        if (stateCounts.Values.Any(count => count != 20))
            throw new InvalidOperationException("Seed verification failed for invoice states: " + string.Join(", ", stateCounts.Select(pair => pair.Key + "=" + pair.Value)));
    }

    private static void AssertCount(SqliteConnection connection, SqliteTransaction transaction, string table, int expected)
    {
        var count = Convert.ToInt32(Scalar(connection, transaction, "SELECT COUNT(*) FROM " + table), CultureInfo.InvariantCulture);
        if (count != expected) throw new InvalidOperationException($"Seed verification failed: {table} expected {expected}, got {count}.");
    }

    private static SqliteConnection Open(string path)
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = path,
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true
        }.ToString());
        connection.Open();
        return connection;
    }

    private static void Execute(SqliteConnection connection, SqliteTransaction transaction, string sql, params (string Name, object Value)[] values)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        foreach (var (name, value) in values) command.Parameters.AddWithValue(name, value);
        command.ExecuteNonQuery();
    }

    private static object? Scalar(SqliteConnection connection, SqliteTransaction transaction, string sql, params (string Name, object Value)[] values)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        foreach (var (name, value) in values) command.Parameters.AddWithValue(name, value);
        return command.ExecuteScalar();
    }
}