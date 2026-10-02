# TTCS_QLPhongTro

Ứng dụng ASP.NET Core MVC dùng C#, .NET 10 và Entity Framework Core 10 (SQLite).

## Môi trường

- Cài [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) phù hợp với máy. Chỉ cài Runtime là chưa đủ để build mã nguồn.
- `global.json` chọn SDK 10.0 bản ổn định, cho phép các bản cập nhật trong dòng 10.0.
- Kiểm tra bằng `dotnet --list-sdks` và `dotnet --list-runtimes`; cần có SDK 10.0.x và Microsoft.AspNetCore.App 10.0.x.

## Chạy dự án

Mở Terminal tại thư mục chứa README này. Dừng đúng phiên app bằng Ctrl+C trước khi cập nhật database.

**Trước lần pull nhận thay đổi S2-01 bỏ theo dõi database:** sao lưu database local và các file phụ SQLite ra ngoài repository (dừng app trước). Git có thể xóa bản database trước đây được theo dõi khi pull. Sau pull, khôi phục bản của chính bạn nếu file bị mất; không ghi đè file đang tồn tại.

Mặc định dùng `QL_PhongTro/Data/local-dev.sqlite`; có thể đặt `$env:DatabasePath` là đường dẫn tuyệt đối riêng trong cùng terminal.

Máy mới **chưa có database**:

```powershell
dotnet restore .\QL_PhongTro\QL_PhongTro.csproj
dotnet run --project .\QL_PhongTro\QL_PhongTro.csproj -- --initialize-database
```

Lệnh tạo file mới từ schema nền và các bước cập nhật đã phiên bản hóa, chỉ nạp vai trò/quyền, không tạo tài khoản hoặc dữ liệu demo. Lệnh từ chối file đã tồn tại; nếu khởi tạo thất bại, giữ file để kiểm tra và chọn đường dẫn mới khi thử lại.

Khối lệnh chạy nhanh cố định cho máy **đã có database cần giữ dữ liệu**, hoặc vừa khởi tạo/khôi phục bản sao của chính mình:

Quy ước cập nhật README: luôn giữ khối lệnh dưới đây và nguyên các giá trị ADMIN local `admin-local@example.test`, `ThayBangMatKhauManh123!`, `0900000000`; chỉ thay đổi khi người dùng yêu cầu trực tiếp. Đây là cấu hình mẫu cho máy local. Nếu ADMIN đã tồn tại, bỏ qua lệnh `--create-local-admin` và đăng nhập bằng mật khẩu hiện có; lệnh này không đổi mật khẩu tài khoản cũ.

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

Updater kiểm tra schema, sao lưu và nâng lên v8; không tạo lại database. Web không tự nâng schema v7 lên v8. Sau khi cập nhật, có thể kiểm tra chỉ đọc bằng `dotnet run --project .\QL_PhongTro\QL_PhongTro.csproj -- --check-database`.
Truy cập http://localhost:5247. Giữ terminal đang hiện `Now listening on: http://localhost:5247` mở trong lúc sử dụng. Muốn dừng server, bấm vào đúng terminal đó, nhấn Ctrl+C và chờ dấu nhắc `PS C:\...>` xuất hiện lại rồi mới build hoặc chạy lần nữa.

Không cần cập nhật database hoặc tạo ADMIN mỗi lần chạy. Sau khi pull thay đổi schema, chạy updater một lần. Tài khoản mới đang chờ xác nhận thì tiếp tục nhập/gửi lại mã, không cần xóa rồi đăng ký lại.

Lệnh tạo ADMIN không ghi đè tài khoản đã có. Nếu có lỗi, dừng và xử lý lỗi trước khi chạy bước tiếp theo. Xem thêm [hướng dẫn cập nhật SQLite](docs/cap-nhat-csdl.md).

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

Nếu terminal đã hiện dấu nhắc `PS C:\...>` nhưng website vẫn truy cập được, server đang chạy ở terminal hoặc tiến trình khác. Xác định đúng phiên đang dùng và dừng bằng Ctrl+C.

Lỗi `MSB3021` hoặc `MSB3027` kèm thông báo `QL_PhongTro.exe ... being used by another process` có nghĩa là server cũ đang khóa file build. Chạy lệnh dừng ở trên, đợi vài giây rồi chạy lại lệnh `dotnet run`. Lỗi SQLite như `no such table` có nguyên nhân khác; làm theo quy trình `--update-database` và tài liệu `docs/cap-nhat-csdl.md`.

**`FileLoadException`, `0x800711C7`, “An Application Control policy has blocked this file”**: Windows chặn thực thi DLL trước khi app chạy. Build có thể thành công nhưng web, updater và kiểm tra SMTP đều chưa chạy được. Không chạy lặp lại lệnh tạo ADMIN/cập nhật DB để xử lý lỗi này.

Trên máy đã kiểm tra, nhật ký `Microsoft-Windows-CodeIntegrity/Operational` có event 3077, policy `VerifiedAndReputableDesktop` (Smart App Control), chặn `QL_PhongTro.dll` chưa ký số. Đây không phải cảnh báo license ImageSharp hoặc bằng chứng ổ D chỉ đọc. Kiểm tra tại **Windows Security → App & browser control → Smart App Control settings**. Dùng bản build ký số được tin cậy để giữ bảo vệ; nếu chủ máy cá nhân chọn tắt Smart App Control cho môi trường phát triển, cần hiểu thay đổi áp dụng toàn máy. Máy do tổ chức quản lý cần quản trị viên xử lý. Xem [hướng dẫn Microsoft](https://support.microsoft.com/en-us/windows/security/threat-malware-protection/smart-app-control-frequently-asked-questions). README không tự thay đổi chính sách bảo mật.

Lệnh `--update-database` sao lưu rồi cập nhật schema còn thiếu; nên dừng ứng dụng trước khi chạy. Ứng dụng yêu cầu file SQLite đã tồn tại và không tự tạo lại CSDL nền. Mặc định dùng `QL_PhongTro/Data/local-dev.sqlite`; có thể đặt biến môi trường `DatabasePath` để dùng file riêng.

Schema v6 bổ sung dịch vụ S1-09 nếu chưa có và hai bảng `dich_vu_toa_nha`, `dich_vu_phong`. Schema v7 thêm cột giá riêng nullable theo từng cặp phòng/dịch vụ; giá riêng thắng giá chung và không thay đổi snapshot hóa đơn. Schema v8 bổ sung lịch sử ngừng dịch vụ phòng. Hợp đồng/hóa đơn vẫn là module tùy chọn, không được tự triển khai bởi S2-01.

### Dịch vụ tòa nhà và phòng (S2-01)

Chủ nhà mở `/DichVu`, khai báo tên/đơn giá và bật “Áp dụng mặc định”. Từ danh sách phòng chọn **Dịch vụ** để thêm/bỏ và xem tổng dịch vụ cố định dự kiến/tháng. Khi gán dịch vụ có thể nhập đơn giá riêng; bảng hiển thị giá chung, giá áp dụng, đánh dấu giá riêng khác giá chung và cho sửa hoặc quay lại giá chung. Giá riêng chỉ áp dụng cho phòng đó. Phòng mới (đơn hoặc hàng loạt) nhận lựa chọn mặc định tại lúc tạo; thay đổi mặc định không cập nhật các phòng cũ. Phòng có trước migration giữ nguyên dữ liệu và bắt đầu với danh sách lựa chọn mới rỗng; chủ nhà chọn dịch vụ cần dùng.

Tổng không gồm tiền thuê, phí theo chỉ số/theo người hoặc dịch vụ chưa có giá đang áp dụng. PO đã chốt: bỏ dịch vụ giữa tháng vẫn tính hết tháng đó và ngừng từ ngày đầu tháng sau. Hóa đơn đã phát hành hiển thị snapshot tên, đơn giá và thành tiền đã lưu; không đọc lại trạng thái dịch vụ phòng hiện tại. Khi phát hành hóa đơn, chọn hợp đồng/phòng và kỳ để danh sách dịch vụ được lọc theo đúng phòng và tháng.

### Demo cấu hình điện nước theo tòa (S2-10)

Fixture này tạo database hoàn toàn mới trong `data/s210-demo/`, không đọc hoặc ghi `QL_PhongTro/Data/local-dev.sqlite`. Cổng demo là `5251`; hãy dừng phiên demo cũ trên đúng cổng trước khi tạo bản mới.

```powershell
dotnet build .\QL_PhongTro\QL_PhongTro.csproj -c Debug -o .\data\s210-demo\runtime --no-restore
python .\verification\prepare_s210_demo.py
```

Script tạo bốn tài khoản giả dùng chung một mật khẩu ngẫu nhiên, một tòa và hai phiên bản cấu hình cho mỗi dịch vụ; sau đó kiểm tra đăng nhập, lưu kỳ sau, từ chối giá 0, integrity/FK và giữ server chạy tại `http://localhost:5251`. Đọc thông tin bản mới nhất:

```powershell
$demoFolder = (Get-Content .\data\s210-demo\latest.txt -Raw).Trim()
$demo = Get-Content (Join-Path $demoFolder 'access.json') -Raw | ConvertFrom-Json
$demo.url
$demo.accounts.CHU_NHA.email
$demo.password
$demo.current
$demo.pending
```

Đăng nhập bằng tài khoản `CHU_NHA`, mở **Quản lý tòa nhà → Cấu hình điện nước**. Màn hình mẫu cho thấy kỳ hiện tại có điện theo chỉ số và nước theo đầu người; kỳ kế tiếp đảo lại hai cách tính và ghi rõ tháng bắt đầu áp dụng. Đổi lựa chọn hoặc giá hợp lệ để thấy thông báo kỳ trước nút lưu. Xóa giá hoặc nhập `0`, bấm lưu để thấy lỗi và dữ liệu không đổi.

Muốn chạy lại đúng database demo sau khi server đã dừng:

```powershell
$env:DatabasePath = $demo.database
$env:DataProtectionKeysPath = Join-Path $demoFolder 'keys'
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ASPNETCORE_URLS = $demo.url
dotnet .\data\s210-demo\runtime\QL_PhongTro.dll
```

`DataProtectionKeysPath` chỉ định thư mục khóa cookie riêng cho phiên demo, tránh dùng chung kho khóa của database khác. Database, khóa, mật khẩu, log và HTML kiểm tra đều nằm trong `data/` đã được Git ignore; không commit hoặc chia sẻ chúng.

### Demo bỏ dịch vụ khỏi phòng

Tạo fixture hoàn toàn giả trên database mới, không ghi database local:

```powershell
dotnet run --project .\verification\ServiceRemovalDemo\ServiceRemovalDemo.csproj -- .
$demoAccessPath = (Get-Content -Raw data\service-removal-demo\latest.txt).Trim()
$demo = Get-Content -Raw $demoAccessPath | ConvertFrom-Json
$env:DatabasePath = $demo.database
dotnet run --project .\QL_PhongTro\QL_PhongTro.csproj --launch-profile http
```

Mở file `access.json` tại đường dẫn trong `latest.txt` để lấy email/mật khẩu fixture. Đăng nhập Chủ nhà, vào **Phòng → Dịch vụ** của `DEMO-A`, bấm **Ngừng từ kỳ sau** tại Gửi xe. Mở lại hóa đơn kỳ trước để thấy Gửi xe vẫn là 100.000đ. Vào **Lập hóa đơn**, chọn `HD-DEMO-A` và kỳ sau ngày ngừng: danh sách không còn Gửi xe; phát hành sẽ chỉ còn tiền phòng. Chọn `HD-DEMO-B` cùng kỳ vẫn thấy Gửi xe. Database, credential và backup demo nằm trong `data/` đã được Git ignore.

Kiểm thử không cần database cá nhân: `dotnet test Tests/QL_PhongTro.Tests/QL_PhongTro.Tests.csproj`. Kiểm tra nâng cấp trên bản sao database local: build trước rồi chạy `python verification/s201_database.py`. Không commit database, backup, file phụ SQLite hoặc cấu hình bí mật.

## Tài khoản quản trị local

Các lệnh tạo ADMIN từ cấu hình riêng đã có trong mục **Chạy dự án**. Lệnh chỉ chạy trong Development, không ghi đè tài khoản đã có và sao lưu trước khi tạo. Mật khẩu trong SQLite được lưu dưới dạng băm BCrypt.

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

Các lệnh trong mục này là hướng dẫn demo lịch sử S1-09, **chưa được cập nhật/nghiệm thu cho v6**; không dùng để xác minh S2-01. Dùng bộ xUnit và `verification/s201_database.py` ở trên cho nhánh này.

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
