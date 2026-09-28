# S1-03 – Tạo tài khoản, mật khẩu tạm, khoá và danh sách

## Quy tắc đã chốt với PO

- Chỉ ADMIN tạo/khoá/mở khoá Chủ nhà và Quản lý toà nhà; không tự khoá, không khoá ADMIN hoặc Khách thuê trong chức năng này.
- Mật khẩu tạm: 16 ký tự ngẫu nhiên mật mã, đủ chữ hoa, chữ thường, số, ký tự đặc biệt. Chỉ lưu BCrypt trong CSDL, không hiển thị mật khẩu trong danh sách.
- Mật khẩu người dùng đặt: tối thiểu 8 ký tự có chữ và số, khác mật khẩu hiện tại, nhập xác nhận khớp.
- Email văn bản thuần, tiêu đề “Tài khoản Nhà Trọ – yêu cầu đổi mật khẩu lần đầu”; gồm tên, vai trò, email đăng nhập, mật khẩu tạm, liên kết đăng nhập, yêu cầu đổi ngay và không chia sẻ mật khẩu.
- Danh sách dành cho ADMIN, hiển thị mọi vai trò; kết hợp vai trò + trạng thái, 20 dòng/trang, mới nhất trước. Không có kết quả hiển thị trang rỗng 1/1.

## Chạy bản demo đã chuẩn bị trên máy này

Bản demo `data/s103-demo-ready.sqlite` đã có ADMIN, Chủ nhà, Quản lý, Khách thuê mẫu và thêm 45 Quản lý hoạt động + 8 bị khoá. Thông tin đăng nhập ngẫu nhiên được cung cấp trong câu trả lời bàn giao, không lưu mật khẩu vào Git. CSDL chính không đổi trong task này.

Nếu app đang chạy, dùng ngay http://localhost:5247/Account/Login. Nếu cần chạy lại, mở PowerShell tại thư mục gốc dự án:

```powershell
dotnet restore QL_PhongTro
dotnet build QL_PhongTro --no-restore
$env:DatabasePath = (Resolve-Path 'data/s103-demo-ready.sqlite').Path
$env:PasswordReset__PublicBaseUrl = 'http://localhost:5247'
$env:PasswordReset__PickupDirectory = 's103-mail-preview'
dotnet run --no-build --project QL_PhongTro --launch-profile http
```

Dừng phiên đang nghe cổng 5247 trước khi chạy phiên khác. Dùng Ctrl+C để dừng. Không chạy hai updater/web cùng lúc trên cùng file DB.

## Tạo demo mới trên máy đồng đội

Sau build, chạy tại thư mục gốc (cần Python 3):

```powershell
python verification/prepare_s103_demo.py data/s103-demo-new.sqlite
$env:DatabasePath = (Resolve-Path 'data/s103-demo-new.sqlite').Path
$env:PasswordReset__PublicBaseUrl = 'http://localhost:5247'
$env:PasswordReset__PickupDirectory = 's103-mail-preview'
dotnet run --no-build --project QL_PhongTro --launch-profile http
```

Script sao chép CSDL hiện có, cập nhật schema trên bản sao, in bốn email `demo-...` cùng mật khẩu demo dùng chung và thêm dữ liệu phân trang. Chọn email `demo-admin-...` để quản trị. Nếu file đích đã có, script từ chối ghi đè; dùng lại file hoặc chọn tên mới. Cần có CSDL nền `QL_PhongTro/Data/local-dev.sqlite`; không tự tạo lại DB nền.

## Test 1 – Tạo Chủ nhà và kiểm tra dữ liệu không hợp lệ

1. Trình duyệt A đăng nhập bằng ADMIN demo. Chọn **Quản lý tài khoản** hoặc mở `/ManagedAccounts`.
2. Chọn **Tạo tài khoản**; nhập tên `Chủ nhà kiểm thử`, email mới như `owner-s103@example.test`, số điện thoại chưa dùng `0912345678`, vai trò **Chủ nhà**.
3. Bấm **Tạo và gửi email**. Kỳ vọng quay lại danh sách, tài khoản có trạng thái **Đang hoạt động / Chờ đổi mật khẩu**. Không có mật khẩu hiện trên trang.
4. Thử tạo lại với cùng email (kể cả viết hoa). Kỳ vọng báo email đã sử dụng, không thêm tài khoản.
5. Thử email `abc`, số `123` hoặc số không bắt đầu bằng 0. Kỳ vọng trình duyệt hoặc server báo định dạng sai; không tạo tài khoản. Số điện thoại trùng cũng bị từ chối.
6. Vai trò tạo mới chỉ có Chủ nhà/Quản lý. Truy cập `/ManagedAccounts` bằng tài khoản khác ADMIN phải bị từ chối; gửi POST thiếu chống giả mạo bị HTTP 400.

## Test 2 – Email mật khẩu tạm và bắt buộc đổi

1. Với cấu hình demo, mở PowerShell khác và liệt kê email:

   ```powershell
   Get-ChildItem "$env:TEMP\s103-mail-preview" -Filter *.eml | Sort-Object LastWriteTime -Descending
   ```

2. Mở file mới nhất bằng ứng dụng đọc `.eml` hỗ trợ MIME; nếu mở bằng Notepad, nội dung có thể được mã hoá MIME nên không đọc trực tiếp được. Kiểm tra **To** đúng email vừa tạo, tiêu đề/nội dung theo mẫu đã chốt. Lấy mật khẩu tạm trong nội dung thư. File này chứa mật khẩu, chỉ dùng local và không đưa lên Git.
3. Mở trình duyệt B hoặc cửa sổ ẩn danh, đăng nhập bằng email mới và mật khẩu tạm. Kỳ vọng tự chuyển `/Account/ChangePassword` và hiện cảnh báo bắt buộc đổi.
4. Gõ trực tiếp `/`, `/PhongTro`, `/HoSo`, `/Account/Register`, `/ManagedAccounts`. Kỳ vọng vẫn bị chuyển về đổi mật khẩu. API bị chặn; API login bằng mật khẩu tạm không cấp token mà hướng dẫn đăng nhập MVC để đổi.
5. Thử mật khẩu hiện tại sai, mật khẩu mới ngắn/không có số, xác nhận không khớp, hoặc mới giống tạm. Kỳ vọng không thành công.
6. Nhập đúng mật khẩu tạm và mật khẩu mới, ví dụ `ChuNhaMoi123!`, xác nhận khớp. Kỳ vọng trở về đăng nhập, tất cả phiên cũ bị thu hồi.
7. Đăng nhập bằng mật khẩu tạm phải thất bại; đăng nhập bằng mật khẩu mới phải vào được hệ thống. Danh sách ADMIN đổi thành **Đã thiết lập**.
8. Tuỳ chọn thử gửi lại: với một tài khoản còn chờ đổi, ADMIN bấm **Gửi lại mật khẩu tạm** và xác nhận. Thư mới có mật khẩu mới; mật khẩu tạm và phiên cũ không còn dùng được.

Email thất bại: tài khoản vẫn được lưu ở trạng thái chờ đổi; trang hiện lỗi gửi thư và cho gửi lại sau khi sửa cấu hình. Không xoá tài khoản, không lưu mật khẩu rõ để thử lại. SMTP và CSDL không có transaction chung: nếu máy chủ thư nhận rồi mất kết nối, có thể nhận thư dù UI báo thất bại; lần gửi lại luôn thay mật khẩu cũ.

## Test 3 – Khoá và mở khoá Quản lý

1. Trình duyệt B đăng nhập tài khoản `demo-quan_ly-...` bằng mật khẩu demo; giữ trang ở trạng thái đang mở. Có thể dùng Quản lý do ADMIN tạo sau khi đổi mật khẩu.
2. Trình duyệt A (ADMIN) lọc **Quản lý toà nhà**, tìm đúng email. Bấm **Khoá**: chọn Huỷ trước để xác nhận không đổi trạng thái; bấm lại và đồng ý để khoá.
3. Kỳ vọng danh sách hiện **Đã khoá**. Trên B, yêu cầu dữ liệu tiếp theo bị từ chối ngay; trang hoạt động kiểm tra phiên mỗi 20 giây và chuyển về đăng nhập, trong vòng 1 phút khi trình duyệt hoạt động và có mạng.
4. Đăng nhập lại trên B bằng đúng mật khẩu vẫn thất bại. Cookie/JWT/refresh token cũ đều bị thu hồi, kể cả sau khi mở khoá.
5. ADMIN bấm **Mở khoá**, xác nhận. B đăng nhập lại bằng mật khẩu hiện tại thành công; phải tạo phiên mới.
6. Không có nút khoá cho ADMIN/Khách thuê. POST giả mạo để tự khoá hoặc khoá vai trò ngoài phạm vi bị server từ chối.

Tab bị trình duyệt treo/đóng hoặc mất mạng không thể tự chuyển trang đúng thời gian; kiểm tra bảo mật vẫn chạy tại server trên mọi yêu cầu. Không dựa vào JavaScript để cấp/từ chối quyền.

## Test 4 – Bộ lọc và phân trang

1. ADMIN mở `/ManagedAccounts`; kiểm tra tổng tài khoản, tổng trang và tối đa 20 dòng.
2. Lần lượt chọn từng vai trò và **Lọc**; mỗi dòng phải đúng vai trò.
3. Lần lượt chọn **Đang hoạt động**, **Đã khoá**; mỗi dòng phải đúng trạng thái.
4. Kết hợp **Quản lý toà nhà + Đang hoạt động**. Demo có ít nhất 45 dòng phù hợp, đủ ba trang.
5. Dùng **Sau**, **Trước**, **Đầu**, **Cuối**; bộ lọc phải giữ nguyên trên URL, trang giữa tối đa 20 dòng, trang cuối hiển thị phần còn lại.
6. Thay bộ lọc sẽ trở về trang 1. Chọn điều kiện không có dữ liệu để xem thông báo rỗng. Truyền số trang ngoài phạm vi sẽ được đưa về trang đầu/cuối hợp lệ.

## Chạy trên CSDL riêng và gửi thư thật

Schema S1-03 là phiên bản 2: thêm `tai_khoan.must_change_password` mặc định false và unique index email đã chuẩn hoá/số điện thoại. Dữ liệu trùng cũ khiến migration dừng/rollback phiên bản 2; cần xử lý có quyết định riêng, không tự xoá/gộp tài khoản.

```powershell
# Dừng app trước. Đặt DatabasePath đúng file muốn cập nhật.
$env:DatabasePath = (Resolve-Path 'QL_PhongTro/Data/local-dev.sqlite').Path
dotnet run --project QL_PhongTro -- --update-database
dotnet run --project QL_PhongTro -- --check-database
```

Updater tự backup trước khi cập nhật. CSDL gốc trên máy này chưa áp dụng v2 trong task S1-03; chỉ các bản sao demo/test đã được cập nhật. CSDL gốc chưa có ADMIN: không tự gán quyền cho tài khoản thật; dùng demo để nghiệm thu hoặc có quy trình cấp ADMIN được chủ dự án duyệt.

Để SMTP gửi thật, bỏ chế độ pickup bằng `$env:PasswordReset__PickupDirectory = ' '`, cấu hình biến môi trường `PasswordReset__Host`, `PasswordReset__Port`, `PasswordReset__EnableSsl`, `PasswordReset__Username`, `PasswordReset__Password`, `PasswordReset__From` và `PasswordReset__PublicBaseUrl`. URL phải HTTPS ngoài Development; Development cho phép HTTP localhost. Không commit mật khẩu SMTP. Dùng địa chỉ nhận thật do bạn kiểm soát, rồi kiểm tra hộp thư/spam. Thông báo SMTP thành công chỉ xác nhận máy chủ chấp nhận, không bảo đảm thư đã tới inbox.

## Xác minh đã thực hiện

- Build .NET 10 thành công, 0 lỗi; còn cảnh báo giấy phép ImageSharp và 2 CS8601 cũ.
- `python verification/s103_http.py`: PASS trên SQLite bản sao và pickup `.eml`; tạo/validation/trùng/CSRF/quyền ADMIN, nội dung/người nhận thư, lỗi thư/gửi lại, bắt buộc đổi và chặn bypass MVC/API, login mới, khoá cả Chủ nhà/Quản lý, cookie/JWT/refresh/unlock, lọc riêng/kết hợp và phân trang 3 trang, integrity/FK. SHA-256 CSDL gốc trước/sau không đổi.
- `python verification/database_updates.py`: PASS nâng cấp, backup, giữ dữ liệu, chạy lặp lại và từ chối phiên bản tương lai/file không tồn tại.
- Chưa kiểm tra bằng trình duyệt đồ hoạ, chưa xác minh SMTP tới hộp thư thật, chưa chạy toàn bộ test nghiệp vụ khác. Quy trình 20 giây đã triển khai bằng JavaScript; HTTP test xác minh phiên bị từ chối ngay, không giả định đã đo đồng hồ UI.
