using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace QL_PhongTro.Data;

// Append new versions; never rewrite an update already shared with the team.
public static class DatabaseUpdates
{
    private const int CurrentVersion = 9;
    private static SqliteConnection Open(string path, bool readOnly)
    {
        var c = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = path, Mode = readOnly ? SqliteOpenMode.ReadOnly : SqliteOpenMode.ReadWrite,
            ForeignKeys = true
        }.ToString());
        c.Open();
        return c;
    }

    private static void Execute(SqliteConnection c, string sql)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    private static bool HasTable(SqliteConnection c, string name)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=$name";
        cmd.Parameters.AddWithValue("$name", name);
        return Convert.ToInt64(cmd.ExecuteScalar()) != 0;
    }

    public static void Check(string path) => Check(path, true, true, true);

    private static void Check(string path, bool requireAudit, bool requireAccountSecurity = true, bool requireRoomServices = false, bool requireTenantRequests = true)
    {
        using var c = Open(path, true);
        var problems = new List<string>();
        using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(c).Options);
        var contracts = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "hop_dong", "ky_hop_dong" };
        var services = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "dich_vu", "cau_hinh_dich_vu", "khoi_tao_dich_vu" };
        var invoices = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "hoa_don", "chi_tiet_hoa_don", "hop_dong_dich_vu" };
        var requiredOptional = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (contracts.Any(table => HasTable(c, table))) requiredOptional.UnionWith(contracts);
        if (services.Any(table => HasTable(c, table))) requiredOptional.UnionWith(services);
        if (invoices.Any(table => HasTable(c, table)))
        {
            requiredOptional.UnionWith(contracts);
            requiredOptional.UnionWith(services);
            requiredOptional.UnionWith(invoices);
        }
        foreach (var entity in db.Model.GetEntityTypes())
        {
            var table = entity.GetTableName()!;
            if (!requireRoomServices && table is "dich_vu_toa_nha" or "dich_vu_phong" or "ngung_dich_vu_phong") continue;
            if (!requireTenantRequests && table == "yeu_cau_thue") continue;
            if (table == "nhat_ky_hoat_dong" && !requireAudit) continue;
            var isOptional = contracts.Contains(table) || services.Contains(table) || invoices.Contains(table);
            if (isOptional && !requiredOptional.Contains(table)) continue;
            var store = StoreObjectIdentifier.Table(table, entity.GetSchema());
            var columns = entity.GetProperties().Select(p => p.GetColumnName(store)!)
                .Where(column => requireAccountSecurity || table != "tai_khoan" || column is not ("email_confirmed" or "is_deleted"));
            Probe(table, columns);
        }
        Probe("password_reset_token", ["token_hash", "account_id", "expires_at", "used_at"]);
        Probe("password_reset_request", ["id", "email_key", "requested_at"]);
        Probe("account_session_version", ["account_id", "version"]);
        if (requireAccountSecurity) Probe("email_confirmation", ["account_id", "token_hash", "expires_at", "requested_at"]);
        if (HasTable(c, "app_module") && HasTable(c, "app_role") && HasTable(c, "role_permission"))
        {
            using var cmd = c.CreateCommand();
            cmd.CommandText = "SELECT (SELECT COUNT(*) FROM app_module) * (SELECT COUNT(*) FROM app_role) * (SELECT COUNT(*) FROM role_permission)";
            if (Convert.ToInt64(cmd.ExecuteScalar()) == 0) problems.Add("Permission seed is missing; inspect permissions.seed.json and existing permission data.");
        }
        if (requireAudit)
            AuditSchema.Validate(c);
        if (problems.Count != 0)
            throw new InvalidOperationException("Database schema is not ready: " + string.Join("; ", problems) +
                "\nStop the app, then run: dotnet run --project QL_PhongTro -- --update-database" +
                "\nUse the same DatabasePath for update and startup. Existing incompatible columns require manual review. No schema was changed by this check.");
        if (!services.Any(table => HasTable(c, table)))
            Console.WriteLine("Optional S1-09 service schema is not installed; service and service-invoice pages are unavailable for this database.");

        void Probe(string table, IEnumerable<string> columns)
        {
            using var cmd = c.CreateCommand();
            cmd.CommandText = $"PRAGMA table_info(\"{table}\")";
            using var reader = cmd.ExecuteReader();
            var actual = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            while (reader.Read()) actual.Add(reader.GetString(1));
            var missing = columns.Where(column => !actual.Contains(column)).ToArray();
            if (missing.Length > 0) problems.Add(table + ": missing " + string.Join(", ", missing));
        }
    }

    public static void Update(string path, string seedPath)
    {
        // Prevent two updater processes from interleaving. Stop web instances before updating.
        using var updateLock = new FileStream(path + ".update.lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        using (var readOnly = Open(path, true))
        {
            using var probe = readOnly.CreateCommand();
            probe.CommandText = "PRAGMA integrity_check";
            if (probe.ExecuteScalar()?.ToString() != "ok") throw new InvalidOperationException("Integrity check failed.");
            probe.CommandText = "PRAGMA foreign_key_check";
            using var reader = probe.ExecuteReader();
            if (reader.Read()) throw new InvalidOperationException("Foreign key check failed.");
        }
        using var c = Open(path, false);
        if (!HasTable(c, "tai_khoan")) throw new InvalidOperationException("Missing existing tai_khoan table; refusing to create a replacement database.");
        var version = 0;
        if (HasTable(c, "app_schema_version"))
        {
            using var cmd = c.CreateCommand();
            cmd.CommandText = "SELECT COALESCE(MAX(version),0) FROM app_schema_version";
            version = Convert.ToInt32(cmd.ExecuteScalar());
        }
        if (version > CurrentVersion) throw new InvalidOperationException("Database is newer than this code. Pull the matching branch before updating.");
        if (version == CurrentVersion)
        {
            Check(path);
            AccountReuseSchema.Ensure(path);
            Console.WriteLine($"Database already up to date (version {CurrentVersion}). No changes.");
            return;
        }
        if (version >= 5) Check(path, true, requireTenantRequests: version >= 9);
        var backupPath = path + ".before-update-" + Guid.NewGuid().ToString("N") + ".bak";
        using (var backup = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = backupPath }.ToString()))
        {
            backup.Open();
            c.BackupDatabase(backup);
        }
        Console.WriteLine("Database backup: " + backupPath);
        // Legacy initializers are additive and rerunnable. A failed update is not marked complete.
        // The backup covers the whole update; individual initializers also keep their own backups.
        if (version < 1)
        {
        AuthSchemaInitializer.Initialize(path);
        using (var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(c).Options))
            RoomSchemaInitializer.EnsureSchema(db);
        PermissionSchemaInitializer.Initialize(path, seedPath);
        PasswordSchemaInitializer.Initialize(path);
        Execute(c, """
            CREATE TABLE IF NOT EXISTS khach_thue (
                id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                tai_khoan_id INTEGER UNIQUE REFERENCES tai_khoan(id) ON DELETE RESTRICT,
                ho_ten TEXT NOT NULL, ngay_sinh TEXT, so_giay_to TEXT,
                giay_to_bon_so_cuoi TEXT, anh_giay_to_truoc TEXT, anh_giay_to_sau TEXT,
                dia_chi_thuong_tru TEXT, nghe_nghiep TEXT, so_dien_thoai TEXT,
                email TEXT, lien_he_khan_cap TEXT, ngay_tao TEXT NOT NULL
            );
            """);
        Execute(c, """
            CREATE TABLE IF NOT EXISTS app_schema_version (
                version INTEGER NOT NULL PRIMARY KEY, applied_at TEXT NOT NULL
            );
            INSERT INTO app_schema_version(version, applied_at) VALUES (1, strftime('%Y-%m-%dT%H:%M:%fZ','now'));
            """);
        }
        if (version < 2)
        {
            using var tx = c.BeginTransaction();
            using var command = c.CreateCommand();
            command.Transaction = tx;
            command.CommandText = "PRAGMA table_info(tai_khoan)";
            bool hasFlag = false;
            using (var reader = command.ExecuteReader())
                while (reader.Read()) hasFlag |= reader.GetString(1) == "must_change_password";
            if (!hasFlag)
            {
                command.CommandText = "ALTER TABLE tai_khoan ADD COLUMN must_change_password INTEGER NOT NULL DEFAULT 0 CHECK(must_change_password IN (0,1))";
                command.ExecuteNonQuery();
            }
            // Duplicate legacy data causes rollback, never automatic deletion/merging.
            command.CommandText = """
                CREATE UNIQUE INDEX IF NOT EXISTS ux_account_email_normalized ON tai_khoan(lower(trim(email)));
                CREATE UNIQUE INDEX IF NOT EXISTS ux_account_phone ON tai_khoan(so_dien_thoai);
                INSERT INTO app_schema_version(version,applied_at) VALUES(2,strftime('%Y-%m-%dT%H:%M:%fZ','now'));
                """;
            command.ExecuteNonQuery();
            tx.Commit();
        }
        if (version < 3)
        {
            Check(path, false, false, requireTenantRequests: false);
            AuditSchema.Upgrade(c);
        }
        if (version < 4)
        {
            Check(path, true, false, requireTenantRequests: false);
            using var tx = c.BeginTransaction();
            using var command = c.CreateCommand();
            command.Transaction = tx;
            command.CommandText = """
                ALTER TABLE tai_khoan ADD COLUMN email_confirmed INTEGER NOT NULL DEFAULT 1 CHECK(email_confirmed IN (0,1));
                ALTER TABLE tai_khoan ADD COLUMN is_deleted INTEGER NOT NULL DEFAULT 0 CHECK(is_deleted IN (0,1));
                CREATE TABLE email_confirmation (
                    account_id INTEGER NOT NULL PRIMARY KEY REFERENCES tai_khoan(id) ON DELETE RESTRICT,
                    token_hash TEXT NOT NULL UNIQUE,
                    expires_at INTEGER NOT NULL,
                    requested_at INTEGER NOT NULL
                );
                INSERT INTO app_schema_version(version,applied_at) VALUES(4,strftime('%Y-%m-%dT%H:%M:%fZ','now'));
                """;
            command.ExecuteNonQuery();
            tx.Commit();
        }
        AccountReuseSchema.Ensure(path);
        if (version < 6) RoomServicesSchema.Upgrade(c);
        if (version < 7) RoomServicePriceSchema.Upgrade(c);
        if (version < 8) RoomServiceRemovalSchema.Upgrade(c);
        if (version < 9) YeuCauThueSchema.Upgrade(c);
        Check(path);
        Console.WriteLine($"Database updated to version {CurrentVersion}. Existing business rows preserved.");
    }
}
