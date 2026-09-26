# TTCS_QLPhongTro

Ứng dụng ASP.NET Core MVC dùng F#, .NET 10 và Entity Framework Core 10 (SQLite).

## Môi trường

- Cài [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) phù hợp với máy. Chỉ cài Runtime là chưa đủ để build mã nguồn.
- `global.json` chọn SDK 10.0 bản ổn định, cho phép các bản cập nhật trong dòng 10.0.
- Kiểm tra bằng `dotnet --list-sdks` và `dotnet --list-runtimes`; cần có SDK 10.0.x và Microsoft.AspNetCore.App 10.0.x.

## Chạy dự án

Mở Terminal tại thư mục chứa README này:

```powershell
dotnet restore .\QL_PhongTro\QL_PhongTro.fsproj
dotnet run --project .\QL_PhongTro\QL_PhongTro.fsproj --launch-profile http
```

Truy cập http://localhost:5247. Nhấn Ctrl+C để dừng.

Chạy trong lúc phát triển:

```powershell
dotnet watch --project .\QL_PhongTro\QL_PhongTro.fsproj run --launch-profile http
```

Database SQLite được tạo tự động tại `QL_PhongTro/Data/local-dev.sqlite` trên Windows. Không cần cài SQL Server; giữ lại file này nếu đã có dữ liệu.

## Kiểm tra build

```powershell
dotnet build .\QL_PhongTro\QL_PhongTro.fsproj -c Release
```

Ứng dụng F# hiện dùng Razor runtime compilation để hiển thị các view `.cshtml`. API này đã được đánh dấu obsolete trong ASP.NET Core 10; vẫn giữ lại để tương thích với cấu trúc hiện tại.
