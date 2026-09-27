# TTCS_QLPhongTro

Ứng dụng ASP.NET Core MVC dùng C#, .NET 10 và Entity Framework Core 10 (SQLite).

## Môi trường

- Cài [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) phù hợp với máy. Chỉ cài Runtime là chưa đủ để build mã nguồn.
- `global.json` chọn SDK 10.0 bản ổn định, cho phép các bản cập nhật trong dòng 10.0.
- Kiểm tra bằng `dotnet --list-sdks` và `dotnet --list-runtimes`; cần có SDK 10.0.x và Microsoft.AspNetCore.App 10.0.x.

## Chạy dự án

Mở Terminal tại thư mục chứa README này:

```powershell
dotnet restore .\QL_PhongTro\QL_PhongTro.csproj
dotnet run --project .\QL_PhongTro\QL_PhongTro.csproj --launch-profile http
```

Truy cập http://localhost:5247. Nhấn Ctrl+C để dừng.

Chạy trong lúc phát triển:

```powershell
dotnet watch --project .\QL_PhongTro\QL_PhongTro.csproj run --launch-profile http
```

Database SQLite được tạo tự động tại `QL_PhongTro/Data/local-dev.sqlite` trên Windows. Không cần cài SQL Server; giữ lại file này nếu đã có dữ liệu.

## Kiểm tra build

```powershell
dotnet build .\QL_PhongTro\QL_PhongTro.csproj -c Release
```

## Auth / S1-02 (đăng nhập, phiên, khoá, đăng xuất)

- Endpoint: `POST /api/auth/login` (SĐT/email + mật khẩu) → access token (30 phút) + refresh token (7 ngày)
- Endpoint: `POST /api/auth/refresh` (refresh token) → access token mới
- Endpoint: `POST /api/auth/logout` (refresh token + access token) → vô hiệu hoá, xoá token
- Khoá tài khoản: 5 lần sai trong 15 phút → khoá 15 phút; tự mở khi hết hạn hoặc đăng nhập thành công
- Tài khoản test: `test@example.com` / `Test123456`
- Cấu hình JWT: xem `QL_PhongTro/appsettings.json` (JwtSettings) và `.env.example`

Các view `.cshtml` được biên dịch cùng dự án C#. Dùng `dotnet watch` khi phát triển để cập nhật thay đổi giao diện.
