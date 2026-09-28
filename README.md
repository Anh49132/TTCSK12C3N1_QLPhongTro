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

Build Debug ở trên phù hợp để chạy và báo cáo local. Build Release và Docker hiện yêu cầu cấu hình license hợp lệ cho `SixLabors.ImageSharp` 4.1.2; nếu chưa có license, bước build Release sẽ dừng thay vì chỉ cảnh báo. Không thêm khóa license vào Git; cấu hình qua secret của môi trường triển khai.
