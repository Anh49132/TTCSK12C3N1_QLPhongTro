using Microsoft.Data.Sqlite;

namespace QL_PhongTro.Data;

public static class LocalDatabaseInitializer
{
    public static void Create(string path, string seed)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        // Atomic refusal to overwrite, including two simultaneous initializers.
        using (new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None)) { }
        using (var c = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = path, Mode = SqliteOpenMode.ReadWrite }.ToString()))
        {
            c.Open();
            using var cmd = c.CreateCommand();
            cmd.CommandText = """
                CREATE TABLE tai_khoan (
                    id INTEGER PRIMARY KEY AUTOINCREMENT, ho_ten TEXT NOT NULL,
                    email TEXT NOT NULL, so_dien_thoai TEXT NOT NULL, mat_khau TEXT NOT NULL,
                    vai_tro TEXT NOT NULL DEFAULT 'KHACH_THUE', dang_hoat_dong INTEGER NOT NULL DEFAULT 1,
                    is_staff INTEGER NOT NULL DEFAULT 0, is_superuser INTEGER NOT NULL DEFAULT 0,
                    last_login TEXT, ngay_tao TEXT NOT NULL, ngay_cap_nhat TEXT NOT NULL
                );
                """;
            cmd.ExecuteNonQuery();
        }
        DatabaseUpdates.Update(path, seed);
        Console.WriteLine("New local database created. Configure LocalAdmin separately; no accounts or demo data seeded.");
    }
}
