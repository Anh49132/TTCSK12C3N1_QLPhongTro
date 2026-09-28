# S1-05 — Đổi và đặt lại mật khẩu

## Trạng thái

Đã triển khai lần lượt 1 → 2 → 3 → 4, mỗi phần qua bộ xác minh riêng trước khi làm phần kế tiếp.
Bộ xác minh HTTP dùng WebApplicationFactory, SQLite bản sao, đồng hồ điều khiển và email `.eml` trong thư mục tạm.
Chưa xác minh SMTP đến hộp thư thật hoặc giao diện bằng trình duyệt đồ họa. Không đồng nhất build thành công với đủ AC end-to-end.

## Chức năng

- `/Account/ChangePassword`: cookie và vai trò KHACH_THUE; ID lấy từ claim, CSRF; kiểm tra mật khẩu hiện tại, mật khẩu mới khác cũ, xác nhận khớp. Cập nhật có điều kiện hash cũ để chống ghi đè đồng thời. Chỉ đổi hash và ngày cập nhật; không thu hồi phiên ở luồng này.
- `/Account/ForgotPassword`: email được trim/lowercase; thông báo trung tính cho email không tồn tại/không đủ điều kiện. Chỉ gửi cho đúng một tài khoản KHACH_THUE đang hoạt động; email trùng trong dữ liệu cũ không được tự chọn ngẫu nhiên.
- Token ngẫu nhiên 256 bit; DB chỉ lưu SHA-256; hạn 30 phút tính từ lúc tạo. `/Account/ResetPassword` kiểm tra lại ở GET và POST. Mật khẩu mới >=8 ký tự, có chữ ASCII và chữ số như đăng ký hiện hành, kèm xác nhận.
- Giới hạn 3 yêu cầu/email trong cửa sổ trượt một giờ. Lần 4 trả 429, không lưu token hoặc gửi email; tại đúng một giờ yêu cầu cũ ra khỏi cửa sổ. Đếm/ghi nhận trong giao dịch SQLite để chống yêu cầu đồng thời; dữ liệu bền vững qua restart.
- Reset thành công đổi mật khẩu, dùng hết token của tài khoản, đổi phiên bản phiên, xóa refresh token và bỏ khóa đăng nhập tạm thời trong cùng giao dịch. Cookie/JWT cũ bị từ chối ở request tiếp theo; các tài khoản khác không bị ảnh hưởng. Không thể thu hồi request đã hoàn tất trước reset.
- Phiên cũ chưa có claim phiên bản được coi là phiên bản `0`; sau reset đầu tiên sẽ mất hiệu lực. Phiên mới chứa phiên bản hiện hành. JWT được kiểm tra tại OnTokenValidated, cookie tại AppCookieEvents.

## Schema và dữ liệu

Lệnh riêng `--initialize-password-security` kiểm tra các cột tài khoản cần thiết, sao lưu SQLite rồi thêm:

| Bảng | Mục đích |
| --- | --- |
| `password_reset_token` | Hash token, tài khoản, hạn dùng, thời điểm sử dụng |
| `password_reset_request` | SHA-256 email chuẩn hóa, thời điểm yêu cầu; index email/thời điểm |
| `account_session_version` | Phiên bản phiên hiện hành theo tài khoản |

Không thêm cột vào bảng cũ; không thay ma trận quyền; không tạo lại DB. Chạy lại không xóa token, lịch sử hoặc phiên bản phiên.
Đã áp dụng vào `QL_PhongTro/Data/local-dev.sqlite`. Backup: `local-dev.sqlite.before-s105-20260927234116421.bak` cùng thư mục Data, được bỏ qua bởi Git.
Đối chiếu backup: schema và nội dung mọi bảng cũ giống nhau; ba bảng mới rỗng; integrity_check=ok, foreign_key_check không có lỗi.

DB khác cần chạy lệnh khởi tạo trước khi dùng phiên bản code này. Schema S1-05 không tự khởi tạo khi mở web.
Các initializer auth/phòng có sẵn ở startup được giữ nguyên, không refactor trong task này.

## Build và chạy

Từ gốc repository (dùng artifacts riêng vì cache obj cũ có lỗi quyền ghi trên máy này):

```powershell
dotnet restore QL_PhongTro/QL_PhongTro.csproj --artifacts-path verification/.artifacts --source https://api.nuget.org/v3/index.json
dotnet build QL_PhongTro/QL_PhongTro.csproj --artifacts-path verification/.artifacts --no-restore
```

Từ thư mục `QL_PhongTro`, với DB khác chưa có schema S1-05:

```powershell
dotnet ../verification/.artifacts/bin/QL_PhongTro/debug/QL_PhongTro.dll --initialize-password-security --DatabasePath 'D:\duong-dan\existing.sqlite'
```

Không cần chạy lại lệnh trên cho DB dự án đã áp dụng. Chạy web từ `QL_PhongTro`:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet ../verification/.artifacts/bin/QL_PhongTro/debug/QL_PhongTro.dll --urls http://localhost:5247
```

## Cấu hình email

Đặt cấu hình qua biến môi trường; không commit mật khẩu SMTP:

```powershell
$env:PasswordReset__PublicBaseUrl = 'https://ten-mien-cua-ban'
$env:PasswordReset__Host = 'smtp.nha-cung-cap-cua-ban'
$env:PasswordReset__Port = '587'
$env:PasswordReset__EnableSsl = 'true'
$env:PasswordReset__From = 'no-reply@ten-mien-cua-ban'
$env:PasswordReset__Username = 'tai-khoan-smtp'
$env:PasswordReset__Password = '<mat-khau-hoac-app-password>'
```

PublicBaseUrl phải là URL tin cậy của ứng dụng; không lấy Host header từ request để tạo reset link. Production yêu cầu HTTPS; Development cho phép HTTP loopback.
SMTP dùng STARTTLS qua SmtpClient; chọn endpoint/port tương thích (thường 587), không dùng endpoint chỉ hỗ trợ implicit TLS.

Demo email cục bộ, chỉ trong Development:

```powershell
$env:PasswordReset__PublicBaseUrl = 'http://localhost:5247'
$env:PasswordReset__From = 'no-reply@example.test'
$env:PasswordReset__PickupDirectory = Join-Path $env:TEMP 's105-mail-preview'
```

Mở tệp `.eml` bằng ứng dụng đọc email để lấy liên kết. Đây là xem trước email cục bộ, **không phải gửi đến hộp thư thật**. Không đặt thư mục này trong wwwroot hoặc commit email. Để gửi SMTP thật, bỏ biến PickupDirectory.
Thiếu cấu hình hoặc lỗi gửi có thông báo thất bại; token tương ứng bị xóa.

## Xác minh đã thực hiện

```powershell
dotnet restore verification/S105/S105.csproj --artifacts-path verification/.artifacts --source https://api.nuget.org/v3/index.json -p:NuGetAudit=false
dotnet run --project verification/S105/S105.csproj --artifacts-path verification/.artifacts --no-restore
```

- Phần 1: đúng/sai mật khẩu hiện tại, trùng mật khẩu, xác nhận lệch, mật khẩu yếu, đăng nhập mới/cũ; chống CSRF, chặn anonymous/sai vai trò, bỏ qua ID từ form, không đưa mật khẩu vào HTML.
- Phần 2: đọc liên kết từ email `.eml` thật do PasswordEmailSender tạo, mở/đặt lại thành công, thông báo và redirect, hết hạn ở mốc 30 phút tại GET/POST, token đã dùng, token hỏng, đăng nhập mới/cũ, email không tồn tại.
- Phần 3: lần 1–3, lần 4, chuẩn hóa email, email khác, đúng mốc một giờ, không tạo token/email khi bị chặn; 4 request song song chỉ 3 thành công.
- Phần 4: 2 cookie và JWT trước reset, cookie cũ redirect Login, access/refresh token cũ 401; cookie/JWT tài khoản khác còn dùng; đăng nhập và refresh mới thành công.
- Bổ sung: lỗi gửi email không báo thành công, token gửi lỗi bị xóa, 2 POST reset đồng thời chỉ một thành công, khởi tạo schema lại giữ dữ liệu phiên.
- SHA-256 nguồn SQLite trước/sau mỗi lần test bằng nhau. Test không chạy suite AuthTests cũ có EnsureDeleted.
- Build chính thành công. Rebuild cuối ghi nhận hai warning CS8601 có sẵn trong AuthController, 0 error. Lần restore đầu có NU1900 vì không truy cập được dịch vụ audit; không tuyên bố đã kiểm tra lỗ hổng dependency.
- `git diff --check` trên file thuộc S1-05 sạch; kiểm tra toàn repo còn báo dòng trắng EOF trong project test đã có sẵn, không sửa.

## Giả định và phần còn lại

- Nội dung email/thông báo là bản tối thiểu tạm dùng, **chưa được PO chốt**.
- Request đã được ghi nhận nhưng gửi email lỗi vẫn tính vào giới hạn để tránh retry flood; email không tồn tại cũng được đếm nhưng không gửi.
- Lịch sử yêu cầu lưu bền vững; chưa thêm job dọn lịch sử vì không thuộc AC hiện tại.
- Cần cấu hình SMTP/URL thực rồi xác minh nhận email và demo trên hai trình duyệt. Chưa thể tuyên bố mọi AC end-to-end đã nghiệm thu khi chưa có bước này.
- Không sửa lỗi Git phân biệt hoa/thường. Không stage, commit hoặc push.

## File tạo/sửa

Tạo:
- `QL_PhongTro/Controllers/AccountController.Passwords.cs`, `AccountController.ResetPassword.cs`.
- `QL_PhongTro/ViewModels/Auth/ChangePasswordViewModel.cs`, `ResetPasswordViewModels.cs`.
- `QL_PhongTro/Views/Account/ChangePassword.cshtml`, `ForgotPassword.cshtml`, `ResetPassword.cshtml`, `InvalidResetLink.cshtml`.
- `QL_PhongTro/Services/PasswordEmailSender.cs`, `PasswordResetService.cs`, `SessionVersionStore.cs`.
- `QL_PhongTro/Data/PasswordSchemaInitializer.cs`.
- `verification/.gitignore`, `verification/S105/S105.csproj`, `verification/S105/Runner.cs`, tài liệu này.

Sửa:
- `AccountController.cs`: partial controller và claim phiên bản khi đăng nhập/đăng ký.
- `AppCookieEvents.cs`, `TokenService.cs`: phát/kiểm tra phiên bản phiên.
- `AuthService.cs`: giao dịch đăng nhập/refresh tránh race với reset, giữ kiểm tra tài khoản hoạt động; không viết lại luồng auth.
- `Program.cs`: đăng ký dịch vụ, lệnh schema riêng và xác minh phiên JWT.
- `Views/Account/Login.cshtml`, `Views/Shared/_Layout.cshtml`: liên kết và thông báo.
- `appsettings.json`: khóa cấu hình email rỗng; `.gitignore`: bỏ qua backup S1-05.
- `Data/local-dev.sqlite`: chỉ thêm ba bảng mới/index như trên; `docs/tien-do.md`: bàn giao.
