# TTCS_QLPhongTro

Ứng dụng ASP.NET Core MVC dùng C#, .NET 10 và Entity Framework Core 10 (SQLite).

## Môi trường

- Cài [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) phù hợp với máy. Chỉ cài Runtime là chưa đủ để build mã nguồn.
- `global.json` chọn SDK 10.0 bản ổn định, cho phép các bản cập nhật trong dòng 10.0.
- Kiểm tra bằng `dotnet --list-sdks` và `dotnet --list-runtimes`; cần có SDK 10.0.x và Microsoft.AspNetCore.App 10.0.x.

## Chạy dự án

### Chạy hằng ngày

Mở terminal tại thư mục chứa README (`D:\DEV\TTCS_T926_K12C3_N1` trên máy hiện tại). Nếu app đang chạy, dừng bằng Ctrl+C trong terminal của app trước khi chạy lại. Nếu cần nhận email thật, thực hiện mục **Nhận mã xác nhận qua Gmail** trước.

```powershell
dotnet run --project .\QL_PhongTro\QL_PhongTro.csproj --launch-profile http
```

Truy cập http://localhost:5247. Giữ terminal đang hiện `Now listening on: http://localhost:5247` mở trong lúc sử dụng. Muốn dừng server, bấm vào đúng terminal đó, nhấn Ctrl+C và chờ dấu nhắc `PS C:\...>` xuất hiện lại rồi mới build hoặc chạy lần nữa.

Không cần cập nhật database hoặc tạo ADMIN mỗi lần chạy. Web tự nâng schema v4 lên v5 có backup để hỗ trợ xóa/tái sử dụng email và SĐT; tài khoản đã xóa không còn trong danh sách. Tài khoản mới đang chờ xác nhận thì tiếp tục nhập/gửi lại mã, không cần xóa rồi đăng ký lại.

### Chuẩn bị lần đầu hoặc nâng schema cũ

Giữ file SQLite hiện có và dừng app trước khi chạy. Với schema cũ hơn v4 hoặc khi nhận thay đổi CSDL yêu cầu cập nhật, làm theo [hướng dẫn cập nhật SQLite](docs/cap-nhat-csdl.md):

```powershell
dotnet restore .\QL_PhongTro\QL_PhongTro.csproj
dotnet run --project .\QL_PhongTro\QL_PhongTro.csproj --launch-profile http -- --update-database
dotnet run --project .\QL_PhongTro\QL_PhongTro.csproj --launch-profile http -- --check-database
```

Chỉ tạo ADMIN nếu chưa có tài khoản quản trị; xem mục **Tài khoản quản trị local**. Nếu có lỗi, dừng và xử lý lỗi trước khi chạy bước tiếp theo.

### Nhận mã xác nhận qua Gmail

Các lệnh chạy app ở trên **không cấu hình Gmail**. Development mặc định lưu thư `.eml` vào `%TEMP%\s105-mail-preview`, không gửi tới hộp thư. Nếu dùng `$env:...`, phải cấu hình trong cùng terminal chạy app; terminal mới không giữ các biến của terminal cũ.

Sau Ctrl+C, kiểm tra chế độ gửi (không hiện mật khẩu, không gửi thư):

```powershell
dotnet run --project .\QL_PhongTro\QL_PhongTro.csproj --launch-profile http -- --check-email-config
```

Để gửi Gmail thật, đặt các biến dưới đây rồi chạy app trong **cùng terminal**. Dùng mật khẩu ứng dụng Google của tài khoản gửi; không nhập mật khẩu vào chat hoặc commit vào file cấu hình.

```powershell
$env:PasswordReset__PickupDirectory = ' ' # Dấu cách để ghi đè chế độ lưu file
$env:PasswordReset__Host = 'smtp.gmail.com'
$env:PasswordReset__Port = '587'
$env:PasswordReset__EnableSsl = 'true'
$env:PasswordReset__Username = Read-Host 'Gmail gui thu'
$env:PasswordReset__From = $env:PasswordReset__Username
$smtpSecret = Read-Host 'Mat khau ung dung Google' -AsSecureString
$env:PasswordReset__Password = [System.Net.NetworkCredential]::new('', $smtpSecret).Password
$env:PasswordReset__PublicBaseUrl = 'http://localhost:5247'
dotnet run --project .\QL_PhongTro\QL_PhongTro.csproj --launch-profile http -- --check-email-config
dotnet run --project .\QL_PhongTro\QL_PhongTro.csproj --launch-profile http
```

Sau đó chọn **Gửi lại mã**. Trang sẽ báo riêng chế độ thử nghiệm, lỗi gửi, hoặc SMTP đã nhận thư. SMTP nhận thư chưa chứng minh thư đã tới inbox; kiểm tra cả Spam. Xem [hướng dẫn mật khẩu ứng dụng Google](https://support.google.com/mail/answer/185833?hl=vi).

Kết quả `--check-email-config`: `PICKUP` nghĩa là chỉ lưu file; `SMTP` nghĩa là dùng máy chủ gửi thư. Với Gmail, host phải là `smtp.gmail.com`, port `587`, TLS `True` và username/password/sender đều `configured: True`. Lệnh này chỉ kiểm tra cấu hình, chưa thử kết nối hoặc xác thực Gmail.

### Xử lý lỗi khởi động

Nếu terminal đã hiện dấu nhắc `PS C:\...>` nhưng website vẫn truy cập được, server đang chạy ở terminal hoặc tiến trình khác. Dừng tiến trình cũ bằng:

```powershell
Get-Process QL_PhongTro -ErrorAction SilentlyContinue | Stop-Process -Force
```

Lỗi `MSB3021` hoặc `MSB3027` kèm thông báo `QL_PhongTro.exe ... being used by another process` có nghĩa là server cũ đang khóa file build. Chạy lệnh dừng ở trên, đợi vài giây rồi chạy lại lệnh `dotnet run`. Lỗi SQLite như `no such table` có nguyên nhân khác; làm theo quy trình `--update-database` và tài liệu `docs/cap-nhat-csdl.md`.

**`FileLoadException`, `0x800711C7`, “An Application Control policy has blocked this file”**: Windows chặn thực thi DLL trước khi app chạy. Build có thể thành công nhưng web, updater và kiểm tra SMTP đều chưa chạy được. Không chạy lặp lại lệnh tạo ADMIN/cập nhật DB để xử lý lỗi này.

Trên máy đã kiểm tra, nhật ký `Microsoft-Windows-CodeIntegrity/Operational` có event 3077, policy `VerifiedAndReputableDesktop` (Smart App Control), chặn `QL_PhongTro.dll` chưa ký số. Đây không phải cảnh báo license ImageSharp hoặc bằng chứng ổ D chỉ đọc. Kiểm tra tại **Windows Security → App & browser control → Smart App Control settings**. Dùng bản build ký số được tin cậy để giữ bảo vệ; nếu chủ máy cá nhân chọn tắt Smart App Control cho môi trường phát triển, cần hiểu thay đổi áp dụng toàn máy. Máy do tổ chức quản lý cần quản trị viên xử lý. Xem [hướng dẫn Microsoft](https://support.microsoft.com/en-us/windows/security/threat-malware-protection/smart-app-control-frequently-asked-questions). README không tự thay đổi chính sách bảo mật.

Lệnh `--update-database` sao lưu rồi cập nhật schema còn thiếu; nên dừng ứng dụng trước khi chạy. Ứng dụng yêu cầu file SQLite đã tồn tại và không tự tạo lại CSDL nền. Mặc định dùng `QL_PhongTro/Data/local-dev.sqlite`; có thể đặt biến môi trường `DatabasePath` để dùng file riêng.

Schema dịch vụ/hóa đơn S1-09 là module tùy chọn và không được tự ghi vào CSDL local. Khi chưa cài module này, ứng dụng tài khoản/phân quyền/phòng vẫn khởi động; các trang dịch vụ và hóa đơn chưa dùng được. Xem mục **Dịch vụ và hóa đơn tối thiểu (S1-09)** trong [bàn giao dự án](docs/tien-do.md) để chuẩn bị fixture riêng.

## Tài khoản quản trị local

Đặt `LocalAdmin__Email`, `LocalAdmin__Password`, `LocalAdmin__Phone` trong terminal local, sau đó chạy `--create-local-admin` sau `--update-database`. Không ghi thông tin thật vào README, `appsettings*.json`, commit hoặc log. Lệnh chỉ chạy trong Development, không ghi đè tài khoản đã có và sao lưu trước khi tạo. Mật khẩu trong SQLite được lưu dưới dạng băm BCrypt. Xoá các biến môi trường khỏi terminal sau khi dùng nếu máy được chia sẻ.

Chỉ chạy khi cần tạo ADMIN lần đầu trên DB đã sẵn sàng:

```powershell
$env:LocalAdmin__Email = Read-Host 'Email ADMIN'
$env:LocalAdmin__Phone = Read-Host 'So dien thoai ADMIN (10 chu so, bat dau bang 0)'
$adminSecret = Read-Host 'Mat khau ADMIN (it nhat 8 ky tu, co chu va so)' -AsSecureString
$env:LocalAdmin__Password = [System.Net.NetworkCredential]::new('', $adminSecret).Password
dotnet run --project .\QL_PhongTro\QL_PhongTro.csproj --launch-profile http -- --create-local-admin
Remove-Item Env:\LocalAdmin__Password
Remove-Variable adminSecret
```

Các biến `LocalAdmin__...` chỉ phục vụ tạo tài khoản, không cấu hình Gmail. Nếu ADMIN đã tồn tại, dùng tài khoản đó để đăng nhập.

Chạy trong lúc phát triển:

```powershell
dotnet watch --project .\QL_PhongTro\QL_PhongTro.csproj run --launch-profile http
```

SQLite mặc định nằm tại `QL_PhongTro/Data/local-dev.sqlite`. Không cần cài SQL Server; giữ lại file này nếu đã có dữ liệu và không chia sẻ CSDL có dữ liệu cá nhân.

Quy trình đồng bộ schema và hướng dẫn kiểm thử S1-03 nằm tại:

- [Cập nhật SQLite](docs/cap-nhat-csdl.md)
- [Tiến độ, chức năng và kiểm thử](docs/tien-do.md)

## Demo dịch vụ và hóa đơn trên bản sao

Demo riêng dùng **http://localhost:5250**, không phải cổng 5247 của DB mặc định. Tạo mới từ gốc repo khi cổng 5250 đang trống:

```powershell
dotnet build QL_PhongTro/QL_PhongTro.csproj -c Debug -o data/service-demo/runtime
python verification/prepare_service_demo.py
```

Cần Python 3. Script dành cho DB nguồn hiện có chưa cài các bảng hợp đồng/dịch vụ/hóa đơn; nếu đã có, script dừng để kiểm tra fixture. Script đọc schema, tạo bản sao mới, cập nhật v3 và schema demo; không ghi DB nguồn. Windows sandbox có thể chặn Data Protection/Event Log: chạy từ terminal Windows bình thường, không thay đổi bảo mật ứng dụng.

Script in **email và mật khẩu thật**, tạo 4 tài khoản, 1 tòa, 3 phòng, 2 hợp đồng hiệu lực 24 tháng và 5 dịch vụ có giá; xác minh đăng nhập/hóa đơn rồi giữ server chạy nền. Dữ liệu hợp đồng là fixture SQL vì chưa có CRUD hợp đồng. Tạo tòa/phòng/dịch vụ và phát hành hóa đơn đi qua HTTP thật để ghi audit. Mọi artifact, credential và log nằm trong `data/service-demo/` đã ignore; file `latest.txt` trỏ thư mục demo mới nhất thành công.

Đăng nhập bằng email CHU_NHA được in ra. Mở `/DichVu`, sau đó `/HoaDonDichVu` và chọn tòa demo. DEMO-101 có hóa đơn tháng hiện tại 2.030.000đ (phòng 2.000.000 + điện 10×3.000). DEMO-102 chưa có hóa đơn: chọn HD-DEMO-2, chỉ chọn Điện, nhập đầu 100/cuối 110, kỳ hiện tại để thử phát hành cùng tổng tiền. ADMIN xem `/NhatKy`; ADMIN không thay thế vai trò Chủ nhà để quản lý dịch vụ.

Đọc lại credential và chạy lại bản demo đã có, không cần tạo thêm:

```powershell
$demoFolder = Get-Content data/service-demo/latest.txt -Raw
$demoInfo = Get-Content (Join-Path $demoFolder 'access.json') -Raw | ConvertFrom-Json
$demoInfo.accounts
$demoInfo.password
# Chỉ chạy tiếp khi phiên demo cổng 5250 đã dừng
$env:DatabasePath = $demoInfo.database
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:PasswordReset__PublicBaseUrl = 'http://localhost:5250'
$env:PasswordReset__PickupDirectory = Join-Path $demoFolder 'mail'
$env:IdentityImagePath = Join-Path $demoFolder 'images'
dotnet run --project QL_PhongTro --launch-profile http -- --urls http://localhost:5250
```

Lần tạo đầu server chạy nền, PID được in và lưu trong access.json; xác nhận tiến trình đúng trước khi dừng. Khi chạy lại bằng terminal, dùng Ctrl+C. Không commit database hoặc access.json. Chưa có xác minh UI đồ họa/SMTP thật trong công cụ này.

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

## Nhật ký hoạt động S1-10

ADMIN xem tại `/NhatKy`. Mã hiện tại yêu cầu schema nhật ký v3; kiểm tra schema của đúng database trước khi chạy và chỉ nâng cấp DB đang dùng khi được yêu cầu. Xem mục **Nhật ký hoạt động (S1-10)** trong [bàn giao dự án](docs/tien-do.md).
