# TTCS_QLPhongTro

Ứng dụng ASP.NET Core MVC dùng C#, .NET 10 và Entity Framework Core 10 (SQLite).

## Môi trường

- Cài [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) phù hợp với máy. Chỉ cài Runtime là chưa đủ để build mã nguồn.
- `global.json` chọn SDK 10.0 bản ổn định, cho phép các bản cập nhật trong dòng 10.0.
- Kiểm tra bằng `dotnet --list-sdks` và `dotnet --list-runtimes`; cần có SDK 10.0.x và Microsoft.AspNetCore.App 10.0.x.

## Chạy dự án

Mở Terminal tại thư mục chứa README này:

```powershell
# Dừng phiên QL_PhongTro cũ nếu terminal trước đã đóng hoặc không còn thấy dòng "Now listening"
Get-Process QL_PhongTro -ErrorAction SilentlyContinue | Stop-Process -Force

dotnet restore .\QL_PhongTro\QL_PhongTro.csproj
dotnet run --project .\QL_PhongTro\QL_PhongTro.csproj -- --update-database
$env:LocalAdmin__Email = 'admin-local@example.test'
$env:LocalAdmin__Password = 'ThayBangMatKhauManh123!'
$env:LocalAdmin__Phone = '0900000000'
dotnet run --project .\QL_PhongTro\QL_PhongTro.csproj -- --create-local-admin
dotnet run --project .\QL_PhongTro\QL_PhongTro.csproj --launch-profile http
```

Truy cập http://localhost:5247. Giữ terminal đang hiện `Now listening on: http://localhost:5247` mở trong lúc sử dụng. Muốn dừng server, bấm vào đúng terminal đó, nhấn Ctrl+C và chờ dấu nhắc `PS C:\...>` xuất hiện lại rồi mới build hoặc chạy lần nữa.

Nếu terminal đã hiện dấu nhắc `PS C:\...>` nhưng website vẫn truy cập được, server đang chạy ở terminal hoặc tiến trình khác. Dừng tiến trình cũ bằng:

```powershell
Get-Process QL_PhongTro -ErrorAction SilentlyContinue | Stop-Process -Force
```

Lỗi `MSB3021` hoặc `MSB3027` kèm thông báo `QL_PhongTro.exe ... being used by another process` có nghĩa là server cũ đang khóa file build. Chạy lệnh dừng ở trên, đợi vài giây rồi chạy lại lệnh `dotnet run`. Lỗi SQLite như `no such table` có nguyên nhân khác; làm theo quy trình `--update-database` và tài liệu `docs/cap-nhat-csdl.md`.

Lệnh `--update-database` sao lưu rồi cập nhật schema còn thiếu; nên dừng ứng dụng trước khi chạy. Ứng dụng yêu cầu file SQLite đã tồn tại và không tự tạo lại CSDL nền. Mặc định dùng `QL_PhongTro/Data/local-dev.sqlite`; có thể đặt biến môi trường `DatabasePath` để dùng file riêng.

Schema dịch vụ/hóa đơn S1-09 là module tùy chọn và không được tự ghi vào CSDL local. Khi chưa cài module này, ứng dụng tài khoản/phân quyền/phòng vẫn khởi động; các trang dịch vụ và hóa đơn chưa dùng được. Dùng fixture riêng theo `docs/s1-09-dich-vu.md` để demo S1-09.

## Tài khoản quản trị local

Đặt `LocalAdmin__Email`, `LocalAdmin__Password`, `LocalAdmin__Phone` trong terminal local, sau đó chạy `--create-local-admin` sau `--update-database`. Không ghi thông tin thật vào README, `appsettings*.json`, commit hoặc log. Lệnh chỉ chạy trong Development, không ghi đè tài khoản đã có và sao lưu trước khi tạo. Mật khẩu trong SQLite được lưu dưới dạng băm BCrypt. Xoá các biến môi trường khỏi terminal sau khi dùng nếu máy được chia sẻ.

Chạy trong lúc phát triển:

```powershell
dotnet watch --project .\QL_PhongTro\QL_PhongTro.csproj run --launch-profile http
```

SQLite mặc định nằm tại `QL_PhongTro/Data/local-dev.sqlite`. Không cần cài SQL Server; giữ lại file này nếu đã có dữ liệu và không chia sẻ CSDL có dữ liệu cá nhân.

Quy trình đồng bộ schema và hướng dẫn kiểm thử S1-03 nằm tại:

- `docs/cap-nhat-csdl.md`
- `docs/s1-03-tai-khoan.md`

## Kiểm tra build

```powershell
dotnet build .\QL_PhongTro\QL_PhongTro.csproj
```

Các view `.cshtml` được biên dịch cùng dự án C#. Dùng `dotnet watch` khi phát triển để cập nhật thay đổi giao diện.

## Story #S1-02 — Auth & Session (Đăng nhập, Khoá tài khoản, Đăng xuất)

### Tính năng
- **Đăng nhập 1 ô**: Nhập số điện thoại (bắt đầu bằng 0, 10 chữ số) **hoặc** email. Hệ thống tự phát hiện loại tài khoản.
- **Khoá tài khoản**: Sau 5 lần đăng nhập sai trong 15 phút → khoá 15 phút. Hiển thị đếm ngược trên trang Login.
- **JWT Access Token** (mặc định 30 phút) + **Refresh Token** (7 ngày). Access token gửi qua header `Authorization: Bearer <token>`.
- **Đăng xuất**: Xoá refresh token trên server + blacklist access token. Client gọi `POST /api/auth/logout`.
- **Auto-refresh**: `auth-interceptor.js` tự làm mới access token khi hết hạn (401 → refresh → retry).

### API endpoints
| Method | Endpoint | Mô tả |
|--------|----------|-------|
| POST | `/api/auth/login` | Đăng nhập, nhận `{accessToken, refreshToken, ...}` |
| POST | `/api/auth/refresh` | Làm mới access token bằng refresh token |
| POST | `/api/auth/logout` | Đăng xuất, xoá token |
| POST | `/api/auth/me` | Lấy thông tin user từ JWT (cần Bearer token) |

### Request / Response mẫu
**Login request**
```json
{
  "TaiKhoanDangNhap": "0912345678",
  "MatKhau": "Passw0rd"
}
```

**Login success (200)**
```json
{
  "accessToken": "eyJhbGciOi...",
  "refreshToken": "dGhpcyBpcyBh...",
  "accessTokenExpiresIn": 1800,
  "refreshTokenExpiresIn": 604800,
  "userId": "12",
  "hoTen": "Nguyễn Văn A",
  "vaiTro": "KHACH_THUE"
}
```

**Login failed (401)**
```json
{ "error": "LOGIN_FAILED", "message": "Tài khoản đã khoá. Vui lòng thử lại sau 12 phút", "code": 401 }
```

### Trang Login (`/Account/Login`)
- 1 ô nhập `TaiKhoanDangNhap` (sđt/email), 1 ô `MatKhau`.
- Hiển thị lỗi chung (`asp-validation-summary`).
- Nếu tài khoản bị khoá → hiện **alert vàng** với badge đếm ngược `mm:ss`.
- JS tự gọi `/api/auth/login`, lưu token vào `localStorage`, chuyển hướng về `/`.

### Cấu hình (appsettings.json)
```json
"JwtSettings": {
  "Issuer": "QL_PhongTro",
  "Audience": "QL_PhongTro_Client",
  "SecretKey": "thay-doi-secret-key-day-chieu-dai-32-ky-tu",
  "AccessTokenMinutes": 30,
  "RefreshTokenDays": 7
}
```

### Test nhanh
```powershell
# 1. Build
dotnet build .\QL_PhongTro\QL_PhongTro.csproj

# 2. Chạy server
dotnet run --project .\QL_PhongTro\QL_PhongTro.csproj --launch-profile http

# 3. Đăng nhập (PowerShell)
$body = @{ TaiKhoanDangNhap = '0912345678'; MatKhau = 'Passw0rd' } | ConvertTo-Json
Invoke-RestMethod -Method Post -Uri 'http://localhost:5247/api/auth/login' -Body $body -ContentType 'application/json'
```

### Lưu ý triển khai
- `AuthService.LoginAsync` dùng **transaction** đảm bảo atomicity khi ghi refresh token.
- `SessionVersionStore` phiên bản hồ sơ để vô hiệu hóa token cũ khi đổi mật khẩu.
- `TokenBlacklistService` (in-memory) chặn access token đã logout cho đến khi hết hạn tự nhiên.
- Đảm bảo `JwtSettings.SecretKey` đủ dài (≥32 ký tự) và khác nhau giữa môi trường dev/staging/prod.

### ⚠️ Lưu ý build Release
Build Debug ở trên phù hợp để chạy và báo cáo local. Build Release hiện yêu cầu cấu hình license hợp lệ cho `SixLabors.ImageSharp` 4.1.2; nếu chưa có license, bước build Release sẽ dừng thay vì chỉ cảnh báo. Không thêm khóa license vào Git; cấu hình qua secret của môi trường triển khai.
