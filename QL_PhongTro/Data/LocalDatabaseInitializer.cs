using Microsoft.Data.Sqlite;

namespace QL_PhongTro.Data;

public static class LocalDatabaseInitializer
{
    public static void Create(string path, string seedPath)
    {
        // CreateNew is atomic: an existing database is never opened for writing here.
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        using (new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None)) { }
        using (var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = path, Mode = SqliteOpenMode.ReadWrite }.ToString()))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            // Small historical baseline; published updates build the current schema.
            command.CommandText = """
                CREATE TABLE tai_khoan (
                    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    ho_ten TEXT NOT NULL, email TEXT NOT NULL, so_dien_thoai TEXT NOT NULL,
                    mat_khau TEXT NOT NULL, vai_tro TEXT NOT NULL DEFAULT 'KHACH_THUE',
                    dang_hoat_dong INTEGER NOT NULL DEFAULT 1, is_staff INTEGER NOT NULL DEFAULT 0,
                    is_superuser INTEGER NOT NULL DEFAULT 0, last_login TEXT,
                    ngay_tao TEXT NOT NULL, ngay_cap_nhat TEXT NOT NULL
                );
                """;
            command.ExecuteNonQuery();
        }
        DatabaseUpdates.Update(path, seedPath);
        Console.WriteLine("Local database initialized without accounts. Use --create-local-admin with private configuration.");
    }
}
