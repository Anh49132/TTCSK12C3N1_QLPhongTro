using Microsoft.Data.Sqlite;

namespace QL_PhongTro.Data;

public static class TinDangSchema
{
    public static void Validate(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, phong_id, nguoi_dang_id, tin_goc_id, tieu_de, noi_dung, ngay_dang, ngay_het_han, trang_thai, ngay_tao FROM tin_dang LIMIT 0";
        using var reader = command.ExecuteReader();
    }

    public static void Upgrade(SqliteConnection connection)
    {
        using var transaction = connection.BeginTransaction();
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE name='tin_dang'";
        if (Convert.ToInt64(command.ExecuteScalar()) != 0)
            throw new InvalidOperationException("Existing unversioned tin_dang needs manual schema review; no listing schema was changed.");
        command.CommandText = """
            CREATE TABLE tin_dang (
                id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                phong_id INTEGER NOT NULL REFERENCES phong_tro(id) ON DELETE RESTRICT,
                nguoi_dang_id INTEGER NOT NULL REFERENCES tai_khoan(id) ON DELETE RESTRICT,
                tin_goc_id INTEGER REFERENCES tin_dang(id) ON DELETE RESTRICT,
                tieu_de TEXT NOT NULL CHECK(length(trim(tieu_de)) BETWEEN 1 AND 200),
                noi_dung TEXT, ngay_dang TEXT, ngay_het_han TEXT,
                trang_thai TEXT NOT NULL DEFAULT 'NHAP'
                    CHECK(trang_thai IN ('NHAP','DANG_HIEN_THI','TAM_AN','DA_CHO_THUE')),
                ngay_tao TEXT NOT NULL
            );
            CREATE INDEX ix_tin_phong_trang_thai ON tin_dang(phong_id,trang_thai);
            CREATE INDEX ix_tin_trang_thai_het_han ON tin_dang(trang_thai,ngay_het_han);
            CREATE UNIQUE INDEX ux_tin_phong_hien_thi ON tin_dang(phong_id) WHERE trang_thai='DANG_HIEN_THI';
            INSERT INTO app_schema_version(version,applied_at) VALUES(6,strftime('%Y-%m-%dT%H:%M:%fZ','now'));
            """;
        command.ExecuteNonQuery();
        command.CommandText = "PRAGMA foreign_key_check";
        using (var reader = command.ExecuteReader())
            if (reader.Read()) throw new InvalidOperationException("Foreign key check failed; listing update rolled back.");
        command.CommandText = "PRAGMA integrity_check";
        if (command.ExecuteScalar() as string != "ok") throw new InvalidOperationException("Integrity check failed; listing update rolled back.");
        transaction.Commit();
    }
}
