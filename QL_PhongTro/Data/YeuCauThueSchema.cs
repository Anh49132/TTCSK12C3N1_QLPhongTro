using Microsoft.Data.Sqlite;

namespace QL_PhongTro.Data;

internal static class YeuCauThueSchema
{
    internal static void Upgrade(SqliteConnection connection)
    {
        using var transaction = connection.BeginTransaction();
        using var command = connection.CreateCommand();
        command.Transaction = transaction;

        command.CommandText = "SELECT 1 FROM sqlite_master WHERE type='table' AND name='yeu_cau_thue'";
        if (command.ExecuteScalar() is not null)
        {
            command.CommandText = "PRAGMA table_info(yeu_cau_thue)";
            var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var reader = command.ExecuteReader())
                while (reader.Read()) existingColumns.Add(reader.GetString(1));
            var required = new[]
            {
                "id", "ma_yeu_cau", "khach_thue_id", "loai_yeu_cau", "ngay_mong_muon",
                "so_nguoi_du_kien", "loi_nhan", "lich_hen", "trang_thai", "ly_do_tu_choi",
                "nguoi_xu_ly_id", "ngay_xu_ly", "ngay_tao", "phien_ban"
            };
            var missing = required.Where(column => !existingColumns.Contains(column)).ToArray();
            if (missing.Length != 0)
                throw new InvalidOperationException(
                    "Existing yeu_cau_thue table is incompatible; no schema changes made. Missing columns: " +
                    string.Join(", ", missing));
        }

        using var stream = typeof(YeuCauThueSchema).Assembly.GetManifestResourceStream("S2-09-yeu-cau-thue.sql")
            ?? throw new InvalidOperationException("Missing embedded S2-09 schema update.");
        using var script = new StreamReader(stream);
        command.CommandText = script.ReadToEnd();
        command.ExecuteNonQuery();

        command.CommandText = "PRAGMA foreign_key_check";
        using (var reader = command.ExecuteReader())
            if (reader.Read()) throw new InvalidOperationException("Foreign key check failed after S2-09 schema update.");
        command.CommandText = "PRAGMA integrity_check";
        if (command.ExecuteScalar()?.ToString() != "ok")
            throw new InvalidOperationException("Integrity check failed after S2-09 schema update.");
        command.CommandText = "INSERT INTO app_schema_version(version,applied_at) VALUES(9,strftime('%Y-%m-%dT%H:%M:%fZ','now'))";
        command.ExecuteNonQuery();
        transaction.Commit();
    }
}
