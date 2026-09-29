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

Schema dịch vụ/hóa đơn S1-09 là module tùy chọn và không được tự ghi vào CSDL local. Khi chưa cài module này, ứng dụng tài khoản/phân quyền/phòng vẫn khởi động; các trang dịch vụ và hóa đơn chưa dùng được. Xem mục **Dịch vụ và hóa đơn tối thiểu (S1-09)** trong [bàn giao dự án](docs/tien-do.md) để chuẩn bị fixture riêng.

## Tài khoản quản trị local

Đặt `LocalAdmin__Email`, `LocalAdmin__Password`, `LocalAdmin__Phone` trong terminal local, sau đó chạy `--create-local-admin` sau `--update-database`. Không ghi thông tin thật vào README, `appsettings*.json`, commit hoặc log. Lệnh chỉ chạy trong Development, không ghi đè tài khoản đã có và sao lưu trước khi tạo. Mật khẩu trong SQLite được lưu dưới dạng băm BCrypt. Xoá các biến môi trường khỏi terminal sau khi dùng nếu máy được chia sẻ.

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

Build Debug ở trên phù hợp để chạy và báo cáo local. Build Release hiện yêu cầu cấu hình license hợp lệ cho `SixLabors.ImageSharp` 4.1.2; nếu chưa có license, bước build Release sẽ dừng thay vì chỉ cảnh báo. Không thêm khóa license vào Git; cấu hình qua secret của môi trường triển khai.

## Nhật ký hoạt động S1-10

ADMIN xem tại `/NhatKy`. Mã hiện tại yêu cầu schema nhật ký v3; kiểm tra schema của đúng database trước khi chạy và chỉ nâng cấp DB đang dùng khi được yêu cầu. Xem mục **Nhật ký hoạt động (S1-10)** trong [bàn giao dự án](docs/tien-do.md).
