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
##
Truy cập http://localhost:5247. Giữ terminal đang hiện `Now listening on: http://localhost:5247` mở trong lúc sử dụng. Muốn dừng server, bấm vào đúng terminal đó, nhấn Ctrl+C và chờ dấu nhắc `PS C:\...>` xuất hiện lại rồi mới build hoặc chạy lần nữa.

Không cần cập nhật database hoặc tạo ADMIN mỗi lần chạy. Web tự nâng schema v4 lên v5 có backup để hỗ trợ xóa/tái sử dụng email và SĐT; tài khoản đã xóa không còn trong danh sách. Tài khoản mới đang chờ xác nhận thì tiếp tục nhập/gửi lại mã, không cần xóa rồi đăng ký lại.

Khối lệnh trên dùng tài khoản mẫu cho môi trường local. Lệnh tạo ADMIN không ghi đè tài khoản đã có, nên mật khẩu mẫu không thay đổi mật khẩu hiện tại. Nếu có lỗi, dừng và xử lý lỗi trước khi chạy bước tiếp theo. Xem thêm [hướng dẫn cập nhật SQLite](docs/cap-nhat-csdl.md).

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
```

Chạy lệnh khởi động cuối cùng trong mục **Chạy dự án** ở cùng terminal, sau đó chọn **Gửi lại mã**. Trang sẽ báo riêng chế độ thử nghiệm, lỗi gửi, hoặc SMTP đã nhận thư. SMTP nhận thư chưa chứng minh thư đã tới inbox; kiểm tra cả Spam. Xem [hướng dẫn mật khẩu ứng dụng Google](https://support.google.com/mail/answer/185833?hl=vi).

Kết quả `--check-email-config`: `PICKUP` nghĩa là chỉ lưu file; `SMTP` nghĩa là dùng máy chủ gửi thư. Với Gmail, host phải là `smtp.gmail.com`, port `587`, TLS `True` và username/password/sender đều `configured: True`. Lệnh này chỉ kiểm tra cấu hình, chưa thử kết nối hoặc xác thực Gmail.

### Xử lý lỗi khởi động

Nếu terminal đã hiện dấu nhắc `PS C:\...>` nhưng website vẫn truy cập được, server đang chạy ở terminal hoặc tiến trình khác. Dùng lệnh dừng tiến trình ở đầu mục **Chạy dự án**.

Lỗi `MSB3021` hoặc `MSB3027` kèm thông báo `QL_PhongTro.exe ... being used by another process` có nghĩa là server cũ đang khóa file build. Chạy lệnh dừng ở trên, đợi vài giây rồi chạy lại lệnh `dotnet run`. Lỗi SQLite như `no such table` có nguyên nhân khác; làm theo quy trình `--update-database` và tài liệu `docs/cap-nhat-csdl.md`.

**`FileLoadException`, `0x800711C7`, “An Application Control policy has blocked this file”**: Windows chặn thực thi DLL trước khi app chạy. Build có thể thành công nhưng web, updater và kiểm tra SMTP đều chưa chạy được. Không chạy lặp lại lệnh tạo ADMIN/cập nhật DB để xử lý lỗi này.

Trên máy đã kiểm tra, nhật ký `Microsoft-Windows-CodeIntegrity/Operational` có event 3077, policy `VerifiedAndReputableDesktop` (Smart App Control), chặn `QL_PhongTro.dll` chưa ký số. Đây không phải cảnh báo license ImageSharp hoặc bằng chứng ổ D chỉ đọc. Kiểm tra tại **Windows Security → App & browser control → Smart App Control settings**. Dùng bản build ký số được tin cậy để giữ bảo vệ; nếu chủ máy cá nhân chọn tắt Smart App Control cho môi trường phát triển, cần hiểu thay đổi áp dụng toàn máy. Máy do tổ chức quản lý cần quản trị viên xử lý. Xem [hướng dẫn Microsoft](https://support.microsoft.com/en-us/windows/security/threat-malware-protection/smart-app-control-frequently-asked-questions). README không tự thay đổi chính sách bảo mật.

Lệnh `--update-database` sao lưu rồi cập nhật schema còn thiếu; nên dừng ứng dụng trước khi chạy. Ứng dụng yêu cầu file SQLite đã tồn tại và không tự tạo lại CSDL nền. Mặc định dùng `QL_PhongTro/Data/local-dev.sqlite`; có thể đặt biến môi trường `DatabasePath` để dùng file riêng.

Schema dịch vụ/hóa đơn S1-09 là module tùy chọn và không được tự ghi vào CSDL local. Khi chưa cài module này, ứng dụng tài khoản/phân quyền/phòng vẫn khởi động; các trang dịch vụ và hóa đơn chưa dùng được. Xem mục **Dịch vụ và hóa đơn tối thiểu (S1-09)** trong [bàn giao dự án](docs/tien-do.md) để chuẩn bị fixture riêng.

## Tài khoản quản trị local

Các lệnh tạo ADMIN mẫu đã có trong mục **Chạy dự án**. Lệnh chỉ chạy trong Development, không ghi đè tài khoản đã có và sao lưu trước khi tạo. Mật khẩu trong SQLite được lưu dưới dạng băm BCrypt.

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
Khởi động server theo mục **Chạy dự án**, rồi mở terminal khác để thử đăng nhập:

```powershell
# Đăng nhập (PowerShell)
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

### Database local và tìm tin (Task 1, 2, 3)

Database local đã bỏ theo dõi Git. **Sao lưu DB riêng ra ngoài repository trước lần pull nhận thay đổi này**, vì Git có thể xóa file từng theo dõi. Không chép đè DB đang sử dụng.

Dùng đúng `DatabasePath` trong cùng terminal; mặc định `QL_PhongTro/Data/local-dev.sqlite`. Web không tự tạo database thiếu, không tự cài bảng tin. Schema v6 chỉ thêm bảng tin/index/FK; không tự tạo tin, tài khoản hoặc mật khẩu.

**Máy mới chưa có database:** chọn đường dẫn mới, rồi khởi tạo (lệnh từ chối file đã tồn tại):

```powershell
$env:DatabasePath = Join-Path (Get-Location) 'data/my-local.sqlite'
dotnet run --project QL_PhongTro -- --initialize-database
dotnet run --project QL_PhongTro -- --check-database
```

ADMIN dùng cấu hình riêng và `--create-local-admin` theo hướng dẫn phía trên; không đưa credential lên Git.

**Máy đã có database:** dừng app dùng file đó, giữ dữ liệu riêng, chọn đúng DatabasePath rồi chạy:

```powershell
dotnet run --project QL_PhongTro -- --update-database
dotnet run --project QL_PhongTro -- --check-database
dotnet run --project QL_PhongTro --launch-profile http
```

Updater tạo backup cạnh DB trước ghi, thêm v6 trong transaction và kiểm tra integrity/FK. Nếu đã có bảng `tin_dang` chưa được quản lý phiên bản, updater từ chối để kiểm tra thủ công; không xóa bảng/chạy initializer để thay DB cũ. Giữ backup ngoài Git. Chỉ nâng cấp trên bản sao khi kiểm thử.

Mở `/TimTin`, kết hợp quận/huyện, giá thuê (VND nguyên), diện tích (m², nhập `20.5` cho 20,5 m²) và số người ở tối đa. Khoảng bao gồm cả hai biên; bỏ trống bỏ qua điều kiện. Số người lọc đúng sức chứa đã chọn. Form giữ điều kiện, báo lỗi min > max. Chỉ hiển thị tin DANG_HIEN_THI còn hạn UTC; hạn NULL bị loại. Không chọn quận thì tìm mọi khu vực. Có sắp xếp và phân trang theo Task 3; chưa có gợi ý khi không có kết quả. DB thiếu bảng tin báo chưa sẵn sàng.

**Kiểm thử và demo dữ liệu giả riêng** (Python 3, không cần package Python):

```powershell
dotnet restore QL_PhongTro/QL_PhongTro.csproj --source https://api.nuget.org/v3/index.json
dotnet build QL_PhongTro/QL_PhongTro.csproj --no-restore -o data/task1-build/runtime
python verification/timtin_http.py --serve
```

Script tạo DB mới/schema v6 và tin giả, chạy 25 ca HTTP; nâng cấp chỉ trên bản sao read-only backup của DB nguồn và đối chiếu bảo toàn dữ liệu/hash. Nguồn mặc định là local-dev.sqlite; chọn nguồn khác bằng `TIMTIN_SOURCE_DATABASE` (không đổi DB nguồn). Không đưa dữ liệu nguồn vào demo. Nếu chưa có DB nguồn, dùng DB riêng đã khởi tạo làm nguồn. Script in URL/PID, giữ demo chạy với `--serve`; bỏ cờ này thì dừng sau kiểm thử. Trên Windows server nền chạy ẩn. `data/timtin-demo/latest.txt` trỏ thư mục kết quả có `result.json` và log. Xác nhận PID đúng trước khi `Stop-Process -Id <PID>`; không dừng các server khác. Nếu sandbox chặn Event Log/Data Protection, chạy từ terminal Windows bình thường.

Chọn Quận 2, giá 2000000..2000000, diện tích 20.5..20.5, sức chứa 3 → chỉ TIN-C. Demo public không cần đăng nhập; không có tài khoản dùng được hoặc mật khẩu cố định. Không seed dữ liệu giả vào DB đang sử dụng.

Task 3: chọn Mới đăng nhất (mặc định, ngày đăng NULL xếp cuối), Giá tăng dần hoặc Giá giảm dần rồi bấm Tìm kiếm. Mỗi trang tối đa 12 tin; tổng kết quả/tổng trang hiện phía trên. Link chuyển trang giữ lọc và sắp xếp, tìm lại từ form luôn về trang 1. Trùng ngày/giá dùng ID giảm dần. Không đổi schema/database cho Task 3.

Kiểm thử/demo Task 3 với 25 tin giả Quận 3 (3 trang: 12/12/1):

```powershell
dotnet build QL_PhongTro/QL_PhongTro.csproj --no-restore -o data/task1-build/task3-runtime
python verification/timtin_http.py --runtime data/task1-build/task3-runtime/QL_PhongTro.dll --task3 --serve
```

Script cũng chạy lại 25 ca Task 1/2; chỉ dùng DB mới/bản sao, không ghi database nguồn. URL và PID in sau khi PASS; bỏ --serve để tự dừng server sau kiểm thử. Chưa xác nhận hiệu năng dưới 2 giây hoặc UI bằng trình duyệt đồ họa.

### Chạy thử tìm tin — đầy đủ 5 tiêu chí

Từ gốc repository, tạo demo mới 500 tin giả bằng các lệnh:

```powershell
dotnet restore QL_PhongTro/QL_PhongTro.csproj --source https://api.nuget.org/v3/index.json
dotnet build QL_PhongTro/QL_PhongTro.csproj --no-restore -o data/task1-build/final-search-runtime
python verification/timtin_performance.py --runtime data/task1-build/final-search-runtime/QL_PhongTro.dll --serve
```

Python 3 không cần package ngoài cho HTTP/performance. Script in URL/PID sau PASS, giữ server nền ẩn trên Windows; mở URL in ra, không cần đăng nhập. Không dùng database thật: mỗi lần tạo thư mục UUID/file mới, schema v6 qua initializer, 500 tin giả ở 5 quận (300 đang hiển thị còn hạn, còn lại ẩn/nháp/đã thuê/hết hạn). Không có mật khẩu cố định/dữ liệu cá nhân. Artifact và backup trong data/timtin-demo đã ignore.

1. Không lọc: 300 kết quả hợp lệ, 25 trang ×12 tin.
2. Chọn Quận 1, giá 1500000..3000000, diện tích 15..25, sức chứa 3: 6 tin khớp tất cả điều kiện.
3. Xóa bớt điều kiện để có >12 tin, thử ba sắp xếp và Trang sau/Trang trước; giữ lọc/sắp xếp. Bấm Tìm kiếm sau đổi điều kiện về trang đầu.
4. Xóa giá tối thiểu, nhập giá tối đa 1: 0 tin và gợi ý nới rộng khoảng giá. Khoảng min > max vẫn báo lỗi riêng.
5. DevTools Network throttling để quan sát Đang tải kết quả khi tìm/chuyển trang; trở lại trang không mắc spinner.

Bản sửa chuẩn bị truy vấn/Razor lúc startup trước khi nhận request (startup khoảng 3.19s, không phải thời gian tìm kiếm). Không cache dữ liệu kết quả. Đo hai lần khởi động 02/10/2026: request đầu 1.014s/0.223s, 260 lượt sau tối đa 0.241s, đạt <2s trên HTTP localhost tuần tự/500 tin. Report ghi riêng startup và đầy đủ samples/median/p95/max; không gồm browser render/static assets, không phải cam kết nhiều người dùng. Các lần FAIL của mã cũ được giữ trong artifact.

`--repeats` đổi số lượt mỗi kịch bản (mặc định 10); bỏ --serve sẽ tự dừng server sau đo. latest-performance.txt trỏ demo PASS có --serve; latest-performance-test.txt trỏ lần test tự dừng. performance.json ghi số liệu; browser.json/ảnh ghi xác minh Chrome. Chỉ dừng đúng PID đã in bằng Stop-Process, không dừng server khác. DB thật vẫn dùng quy trình máy mới/máy có DB phía trên; lần sửa này không đổi schema.

Kiểm thử chức năng/Chrome (chạy sau khi đo performance xong):

```powershell
python verification/timtin_http.py --runtime data/task1-build/final-search-runtime/QL_PhongTro.dll --task3
python -m pip install --target data/task1-build/browser-packages playwright
python verification/timtin_browser.py
```

Browser script dùng Chrome có sẵn, demo --serve từ latest-performance.txt và package Playwright trong thư mục đã ignore; không cần tải browser riêng. Chrome đã xác minh spinner/gợi ý/form sai/Back. HTTP regression 50 ca PASS, kiểm tra cả nguồn DB chỉ đọc/nâng cấp trên bản sao và bảo toàn dữ liệu. Windows sandbox có thể chặn Event Log/Data Protection, khi đó chạy terminal Windows bình thường.

### Task 5 — khoảng giá đề xuất và tìm lại

Khi không có kết quả, khoảng giá đề xuất nới mỗi đầu đã nhập 500.000đ (min không âm, max không tràn); đầu trống tiếp tục không giới hạn. Bấm **Áp dụng khoảng giá gợi ý** để tìm lại ở trang 1, giữ khu vực/diện tích/sức chứa/sắp xếp. Có kết quả thì gợi ý ẩn. Nếu giá đã không giới hạn hoặc không thể nới thêm, hiển thị giải thích và đề nghị chỉnh bộ lọc khác; không giả lập khoảng rộng hơn. Nới giá không bảo đảm có tin nếu các điều kiện khác vẫn loại hết.

Demo mới/data giả riêng:

```powershell
dotnet build QL_PhongTro/QL_PhongTro.csproj --no-restore -o data/task1-build/task5-runtime
python verification/timtin_performance.py --runtime data/task1-build/task5-runtime/QL_PhongTro.dll --serve
```

Mở URL in ra; Quận 1, giá 2100000..2200000, diện tích15..25, sức chứa3 → 0 tin và đề xuất1600000..2700000; bấm áp dụng để tìm thấy tin. Không cần đăng nhập. DB thật không được ghi; task không đổi schema/quy trình khởi tạo/nâng cấp cho máy mới/máy cũ.

Kiểm thử:

```powershell
python verification/timtin_http.py --runtime data/task1-build/task5-runtime/QL_PhongTro.dll --task3 --task5
python verification/timtin_browser.py
```

Browser cần Chrome và package Playwright theo hướng dẫn trên; dùng URL của demo --serve mới nhất. HTTP 9 ca Task 5/50 ca Task 1–3 và thao tác áp dụng trên Chrome đã PASS; báo cáo/ảnh trong thư mục demo đã ignore. Chỉ dừng đúng PID được in sau khi thử xong.

### Cấu hình điện/nước theo tòa nhà

Chủ nhà vào **Dịch vụ**, chọn tòa nhà rồi **Cấu hình điện** hoặc **Cấu hình nước**. Chọn theo chỉ số (VND/kWh hoặc VND/m³) hay khoán theo đầu người (VND/người/tháng); nhập đơn giá nguyên đồng lớn hơn 0. Giao diện hiển thị kỳ bắt đầu trước khi lưu. Cấu hình đầu tiên áp dụng từ ngày khai báo; thay đổi cấu hình đã có áp dụng ngày đầu tháng kế tiếp, giữ lịch sử và hóa đơn cũ. Kỳ đã lên lịch không được ghi đè. Giả định kỳ hóa đơn là tháng dương lịch, theo luồng hóa đơn hiện có.

Không đổi schema/quy trình database trong lần này. Máy mới dùng quy trình khởi tạo đã hướng dẫn ở trên; máy có database giữ file và dữ liệu cũ. Nếu chưa có schema dịch vụ, màn hình báo chưa sẵn sàng; dùng quy trình thiết lập dịch vụ có backup hiện có, không xóa/tạo lại database. Database local hiện tại chưa có bảng dịch vụ và được giữ nguyên.

Demo/kiểm thử độc lập từ thư mục gốc:

```powershell
dotnet build QL_PhongTro/QL_PhongTro.csproj --no-restore -o data/service-demo/utilities-runtime
python verification/utilities_http.py --serve
python verification/utilities_browser.py
```

HTTP script tạo database nền trống mới, sao chép file trống đó thành database thử và tạo tài khoản/dữ liệu giả; không sao chép dữ liệu cá nhân. Mở URL được in, lấy tài khoản CHU_NHA và mật khẩu ngẫu nhiên từ access.json tại đường dẫn được in (file đã ignore). Chọn tòa nhà đầu tiên: điện đã lên lịch đổi sang đầu người, nước còn có thể đổi cách tính để demo. Thử giá trống/0 rồi giá dương, kiểm tra kỳ áp dụng và lịch sử. Browser script dùng Chrome/Playwright theo hướng dẫn phía trên. Báo cáo/ảnh và thông tin đăng nhập chỉ nằm trong data/service-demo đã ignore. Bỏ --serve để tự dừng sau kiểm thử; trước khi build lại, dừng đúng PID demo từ access.json để tránh khóa DLL.

Để thử thủ công từng tiêu chí từ cấu hình chưa lên lịch, chạy `python verification/prepare_utilities_walkthrough.py` sau khi đã build utilities-runtime. Script tạo demo mới, chạy 29 kiểm tra HTTP rồi thêm tòa nhà “Demo thử từng tiêu chí điện nước” với điện 3.500đ/kWh và nước 25.000đ/người/tháng. File `HUONG-DAN-TEST.md` trong thư mục demo được in ra chứa URL, tài khoản/mật khẩu ngẫu nhiên, dữ liệu ban đầu và từng bước thử bốn tiêu chí. Thử giá trống/0 trước khi lưu thay đổi hợp lệ. Muốn thử lại từ đầu, chạy script lần nữa; mỗi lần tạo file mới, không ghi đè dữ liệu cũ. Không đưa file hướng dẫn chứa mật khẩu lên Git.
