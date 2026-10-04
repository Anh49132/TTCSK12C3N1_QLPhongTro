# Chạy dự án và kiểm tra từng tiêu chí Sprint 2

Tài liệu này dùng bộ mẫu do `verification/New-Sprint2Demo.ps1` tạo. Mọi dữ liệu đều giả và nằm trong `data/sprint2-demo/`; database cá nhân `QL_PhongTro/Data/local-dev.sqlite` không được sử dụng. Ảnh màu là ảnh kỹ thuật để kiểm tra upload/thứ tự, không phải ảnh phòng thật.

## 1. Tài khoản và mật khẩu

Mật khẩu chung mặc định của **cả 5 tài khoản**: **`DemoSprint2@2026`**. Tất cả đã xác nhận email, hoạt động và không cần đổi mật khẩu lần đầu.

| Email đăng nhập | Vai trò | Dùng để |
| --- | --- | --- |
| `owner.demo@demo.local` | Chủ nhà | Quản lý Alpha/Beta, phòng, dịch vụ, tin và yêu cầu |
| `tenant.demo@demo.local` | Khách thuê | Gửi yêu cầu, theo dõi, xem lịch sử, hủy |
| `conflict.demo@demo.local` | Khách thuê thứ hai | Có lịch hẹn sẵn để thử cảnh báo trùng; gửi độc lập với khách thứ nhất |
| `admin.demo@demo.local` | ADMIN | Kiểm tra trang quản trị nếu cần; không dùng thay Chủ nhà trong bài test |
| `other.owner@demo.local` | Chủ nhà khác | Quản lý riêng Tòa chủ nhà khác; kiểm tra không xem/sửa được phòng của chủ nhà chính |

Thông tin chính xác của bản mẫu đang chọn được lưu tại `access.json`. Nếu tạo mẫu với `-Password`, dùng mật khẩu trong file đó thay cho mật khẩu mặc định ở tài liệu.

## 2. Chạy dự án từng bước

### Bước 1 — mở PowerShell tại repo

Trong VS Code chọn Terminal → New Terminal, hoặc mở PowerShell rồi nhập:

```powershell
Set-Location D:\TTCS_T926_K12C3_N1
dotnet --list-sdks
```

Cần .NET SDK 10.0.x. Máy hiện tại đã có 10.0.202. Script chỉ dùng PowerShell và .NET, không cần Python, SQL Server hay cài SQLite riêng.

### Bước 2 — sử dụng bộ mẫu đã tạo

Bộ mẫu đã được tạo trong phiên làm việc này. Đọc vị trí, ngày tháng, tài khoản và ảnh:

```powershell
$demoFolder = (Get-Content .\data\sprint2-demo\latest.txt -Raw).Trim()
$demo = Get-Content (Join-Path $demoFolder 'access.json') -Raw | ConvertFrom-Json
$demo.directory
$demo.accounts | Format-Table email,role
$demo.password
$demo.currentPeriod
$demo.nextPeriod
$demo.clashVietnam
explorer $demo.sampleImages
```

Nếu chuyển sang máy khác, chưa có `latest.txt`, hoặc cần dữ liệu sạch để test lại, tạo bản mới:

```powershell
.\verification\New-Sprint2Demo.ps1
```

Script build dự án, tạo thư mục mới, khởi tạo schema, nạp mẫu và kiểm tra integrity/foreign keys. Không ghi đè bản cũ. Chỉ cập nhật `latest.txt` khi tạo thành công. Dừng server demo cũ bằng Ctrl+C trước khi tạo lại vì file build có thể đang bị khóa.

Nếu PowerShell báo không cho chạy script, chỉ mở quyền cho phiên terminal này:

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
```

### Bước 3 — khởi động

```powershell
.\verification\Start-Sprint2Demo.ps1
```

Giữ terminal mở. Đợi `Now listening on: http://localhost:5268`. Script tự truyền đúng database, thư mục ảnh và thư mục khóa cookie; không cần chạy updater/admin initializer của README cho bộ mẫu này. Nếu chưa cấu hình SMTP, email được lưu vào thư mục thử nghiệm. Nếu terminal có đủ các biến `PasswordReset__Host`, `PasswordReset__Username`, `PasswordReset__Password` và `PasswordReset__From`, script tự tắt chế độ lưu file và dùng SMTP. Sau khi đổi cấu hình email phải dừng server cũ bằng Ctrl+C rồi khởi động lại để ứng dụng nạp cấu hình mới.

### Bước 4 — mở website và đăng nhập

Mở **http://localhost:5268/Account/Login**, nhập email và mật khẩu ở mục 1.

Nên dùng cửa sổ thường cho Chủ nhà, cửa sổ ẩn danh cho Khách thuê để không đăng xuất qua lại. Dùng cửa sổ ẩn danh thứ hai hoặc trình duyệt khác cho khách thứ hai nếu cần. Mọi URL bên dưới dùng tiền tố `http://localhost:5268`.

### Bước 5 — dừng và chạy lại

Nhấn **Ctrl+C trong đúng terminal đang chạy server**. Chạy lại `Start-Sprint2Demo.ps1` để dùng tiếp database hiện tại; các thay đổi test trước đó vẫn được giữ.

Muốn quay về trạng thái ban đầu: dừng server → chạy `New-Sprint2Demo.ps1` → chạy `Start-Sprint2Demo.ps1` → đăng nhập lại. Không cần xóa file cũ. Có thể chọn lại bản cũ bằng:

```powershell
.\verification\Start-Sprint2Demo.ps1 -Directory 'D:\TTCS_T926_K12C3_N1\data\sprint2-demo\TEN-THU-MUC-CU'
```

Cổng 5268 bị chiếm: dừng đúng server cũ, hoặc tạo bản mới dùng cổng khác:

```powershell
.\verification\New-Sprint2Demo.ps1 -Port 5270
.\verification\Start-Sprint2Demo.ps1
```

Sau đó thay tiền tố các URL trong tài liệu thành `http://localhost:5270`.

## 3. Bản đồ dữ liệu mẫu

ID bên dưới ổn định vì script luôn tạo database mới. `access.json` vẫn là nguồn chính xác khi kiểm tra.

| Dữ liệu | ID / URL | Ý nghĩa |
| --- | --- | --- |
| Demo Tòa Alpha, Quận 1 | Tòa 1, `/PhongTro/Index?toaNhaId=1` | Điện 4.000 đ/kWh; nước 80.000 đ/người/tháng |
| Demo Tòa Beta, Quận 3 | Tòa 2, `/PhongTro/Index?toaNhaId=2` | Điện 4.000 đ/kWh; nước 18.000 đ/m³ |
| Tòa chủ nhà khác, Quận 7 | Tòa 3 | Chỉ tài khoản Chủ nhà khác sở hữu |
| A101-DEMO, tầng trệt | Phòng 1, tin 1 | 3 ảnh; thuê 3.500.000; cọc 3.500.000; tối đa 3; Internet 150.000; gửi xe giá riêng 70.000; điện giá riêng 4.200 |
| A102-DEMO | Phòng 2, tin 2 | Không gửi xe; Internet 150.000; có 2 yêu cầu để thử trùng lịch |
| B202-DEMO | Phòng 3, tin 3 | Yêu cầu chờ từ chối ở tòa Beta |
| A103-TAO-TIN | Phòng 4 | Trống, chưa có tin; dùng đăng tin mới |
| A104-UPLOAD | Phòng 5, `/PhongTro/Edit/5` | Trống, chưa có ảnh; dùng upload tới giới hạn 8 |
| C001-HOA-DON | Phòng 6, hợp đồng HD-DEMO-C001 | Đang thuê; Internet và gửi xe riêng 70.000; hóa đơn cũ ID 1 tổng 3.720.000 |
| C002-KHONG-GUI-XE | Phòng 7, HD-DEMO-C002 | Đang thuê; không gửi xe; hóa đơn cũ ID 2 tổng 3.650.000 |
| A105-NHAP | Phòng 8, tin 4 | Tin Nháp |
| A106-HET-HAN | Phòng 9, tin 5 | Hết hạn hôm qua, tự chuyển Tạm ẩn khi chạy app |
| A107-TAM-AN | Phòng 10, tin 6 | Tin Tạm ẩn |
| OTHER-01 | Phòng 11, tin 7 | Thuộc Chủ nhà khác |
| A108-GUI-YEU-CAU | Phòng 12, tin 8 | Chưa có yêu cầu mở; tối đa 2 người; thuê 2.500.000, Internet 150.000 |
| PERF-001…PERF-492 | Các phòng/tin bổ sung | Giá, diện tích, sức chứa khác nhau; đủ **500 tin trong database**, bao gồm các tin không công khai |

Dịch vụ mặc định cho phòng mới ở Alpha/Beta: Điện, Nước, Internet. Gửi xe không mặc định. Các phòng PERF phục vụ tìm kiếm và không được gán dịch vụ để giữ dữ liệu đơn giản.

| Yêu cầu ID | Khách / tin | Trạng thái ban đầu | Dùng để |
| --- | --- | --- | --- |
| 1 | Khách chính / A101 | Mới, Thuê ngay | Duyệt đặt cọc |
| 2 | Khách chính / A102 | Mới, Xem phòng, tạo hơn 24 giờ | Xác nhận/đổi lịch và đánh dấu quá hạn |
| 3 | Khách thứ hai / A102 | Đã hẹn lịch, tạo hơn 24 giờ | Lịch trùng; giờ cụ thể trong `$demo.clashVietnam` |
| 4 | Khách chính / B202 | Mới, Xem phòng | Từ chối với lý do |
| 5 | Khách chính / PERF-001 | Từ chối | Lọc trạng thái, hiển thị lý do |
| 6 | Khách chính / PERF-002 | Đã hủy | Không còn nút hủy/khôi phục |
| 7 | Khách chính / PERF-003 | Đã duyệt | Phòng đã đặt cọc, tin Đã cho thuê |

Mã mẫu dạng `YC-yyyyMM-0001`…`0007` theo tháng tạo mẫu. Yêu cầu gửi mới bắt đầu từ `0008` nếu chưa gửi thêm yêu cầu trong tháng đó.

Ảnh thử ở `$demo.sampleImages`: `room-01.png`…`room-10.png`, `room-jpg.jpg`, `over-5mb.png`, `fake-image.png`, `not-an-image.txt`. Ảnh hợp lệ 1200×800; thumbnail sau upload là 400×267. Tệp quá cỡ và giả ảnh phải bị từ chối.

## 4. Cách ghi nhận kết quả

Mỗi hàng AC bên dưới là một tiêu chí độc lập. Thực hiện thao tác rồi đối chiếu cột kỳ vọng. Ghi **PASS/FAIL**, ảnh chụp hoặc thông báo thực tế; không đánh PASS chỉ vì trang mở được.

Nếu test làm thay đổi phòng/tin/yêu cầu, tạo mẫu mới trước story kế tiếp khi cần. Đặc biệt: duyệt yêu cầu 1 làm tin 1 ngừng công khai; test đổi điện nước/ngừng dịch vụ thay đổi cấu hình. Nên kiểm tra S2-01→S2-07 trước, S2-08→S2-09 sau, S2-10 cuối cùng.

## 5. S2-01 — gán dịch vụ và giá riêng (4 AC)

Đăng nhập Chủ nhà. Mở `/DichVuPhong?phongId=1` và `/DichVuPhong?phongId=2` để đối chiếu.

| AC | Thao tác từng bước | Kỳ vọng |
| --- | --- | --- |
| 1 | 1. Mở `/DichVu?toaNhaId=1`, xem cấu hình mặc định. 2. Mở `/PhongTro/Create?toaNhaId=1`, tạo phòng `TEST-MAC-DINH`, tầng 2, diện tích 25, giá 3.000.000, tối đa 2. 3. Vào Dịch vụ của phòng mới, thêm Gửi xe nếu muốn. | Phòng mới có Điện/Nước/Internet, không có Gửi xe mặc định. Thêm/bỏ được theo từng phòng; phòng khác không thay đổi. |
| 2 | 1. Mở dịch vụ A101. 2. So sánh Gửi xe chung 100.000 và riêng 70.000. 3. Đổi riêng thành 90.000, Lưu giá. 4. Mở tin 1 và hóa đơn cũ ID 1. | Giá đang áp dụng là 90.000; phí cố định tin 1 = 150.000+90.000; hóa đơn đã phát hành vẫn giữ gửi xe 70.000. Bấm Dùng giá chung sẽ quay về 100.000 cho giá mới. |
| 3 | Mở A101 rồi A102, kiểm tra bảng và dòng tổng. Với dữ liệu nguyên bản, chưa sửa giá. | A101 tổng cố định 220.000; A102 150.000. Không cộng tiền thuê, điện hoặc nước theo người vào tổng cố định. |
| 4 | 1. Mở `/HoaDonDichVu/Details/1`, ghi nhận gửi xe 70.000. 2. Mở `/DichVuPhong?phongId=6`, bấm Ngừng từ kỳ sau ở Gửi xe. 3. Mở `/HoaDonDichVu?toaNhaId=1`, chọn HD-DEMO-C001 và ngày đầu tháng hiện tại, Xem dịch vụ. 4. Chọn ngày đầu tháng kế tiếp, Xem dịch vụ. 5. Mở lại hóa đơn ID 1. | Kỳ hiện tại còn Gửi xe; kỳ kế tiếp không còn. Hóa đơn cũ vẫn tổng 3.720.000 và gửi xe 70.000. C002 luôn không có Gửi xe. |

Ngày/kỳ hiện tại và kế tiếp đọc từ `$demo.currentPeriod`, `$demo.nextPeriod`; không cần đổi đồng hồ máy để test kỳ sau.

## 6. S2-02 — ảnh phòng (4 AC)

Đăng nhập Chủ nhà, mở `/PhongTro/Edit/5`. Mở thư mục ảnh mẫu bằng lệnh ở mục 2.

| AC | Thao tác từng bước | Kỳ vọng |
| --- | --- | --- |
| 1 | 1. Upload `room-01.png` đến `room-08.png` vào A104. 2. Thử thêm `room-09.png`. 3. Ở phòng còn chỗ, thử JPG hợp lệ, `over-5mb.png`, `fake-image.png`, tệp TXT. | Tối đa 8 ảnh; ảnh thứ 9 bị từ chối. PNG/JPG hợp lệ được chấp nhận. File >5 MiB hoặc nội dung giả bị báo lỗi. TXT có thể bị trình chọn file chặn trước; kiểm tra API phía server đã nằm trong test tự động. |
| 2 | 1. Mở `/PhongTro/Edit/1`. 2. Kéo ảnh màu thứ ba lên đầu. 3. Reload. 4. Mở `/TinDang`, tìm tin Alpha A101 và mở chi tiết tin 1. | Thứ tự được giữ sau reload; ảnh đầu có nhãn Ảnh đại diện và được dùng trên danh sách/chi tiết. Màn hình cảm ứng có mũi tên thay thế. |
| 3 | 1. Trong danh sách tin, Inspect ảnh đại diện A101. 2. Kiểm tra `src` có `-thumb`. 3. Mở DevTools → Console, chọn phần tử `<img>` rồi chạy `$0.naturalWidth`. | Danh sách dùng thumbnail, rộng 400px với ảnh mẫu 1200px. Chi tiết dùng bản gốc. Có thể bật Network throttling Slow 3G để quan sát tải; đây là kiểm tra trải nghiệm, không phải test thời gian S2-04. |
| 4 | 1. Ở `/PhongTro/Edit/1`, bấm Xóa một ảnh rồi Hủy. 2. Bấm Xóa lại và xác nhận. 3. Reload. 4. Kiểm tra cả bản gốc và `-thumb` trong `$demo.roomImagesPath\1`. | Hủy giữ ảnh. Xác nhận xóa bản ghi, gốc và thumbnail; thứ tự còn lại liên tục; xóa ảnh đầu thì ảnh kế lên đại diện. Nếu kho lỗi, ảnh có trạng thái chờ xóa/retry. |

Chỉ dùng bản sao ảnh mẫu ở thư mục demo để thử xóa. Kiểm tra không còn tệp mồ côi sau thao tác bằng lệnh riêng ở mục 15.

## 7. S2-03 — đăng tin từ phòng trống (5 AC)

Đăng nhập Chủ nhà, mở `/TinDang/QuanLy`.

| AC | Thao tác từng bước | Kỳ vọng |
| --- | --- | --- |
| 1 | Mở `/PhongTro/Index?toaNhaId=1`. So sánh A103-TAO-TIN với C001-HOA-DON. Thử trực tiếp `/TinDang/Tao?phongId=6` rồi gửi form. | A103 được đăng; C001 nút bị vô hiệu và có lý do đang thuê. Backend không cho tạo tin cho C001 dù mở URL trực tiếp. |
| 2 | Mở `/TinDang/Tao?phongId=4`. Kiểm tra diện tích, giá và bảng dịch vụ. Với phòng A101, xem form và ảnh lấy từ phòng. | Lấy sẵn diện tích 25, giá 3.500.000 và dịch vụ đúng phòng; chỉnh được tiêu đề/mô tả. Chỉ xem form A101, không đăng thêm vì đang có tin. |
| 3 | 1. Với A103, nhập tiêu đề riêng và Lưu bản nháp. 2. Mở lại bằng Đăng lại. 3. Đăng tin. 4. Gỡ tin trong quản lý. 5. Quan sát PERF-003 đã cho thuê. | Có Nháp→Đang hiển thị→Tạm ẩn; nội dung nháp không mất. Tin mới hết hạn sau 30 ngày. PERF-003 hiện Đã cho thuê; duyệt Thuê ngay trong S2-08 cũng tạo trạng thái này. |
| 4 | Sau khi A103 đã hiển thị, mở lại form `/TinDang/Tao?phongId=4` và thử đăng nữa. | Bị từ chối với thông báo đã có tin hiển thị. Không có hai tin hiển thị cùng phòng. |
| 5 | Quan sát A106-HET-HAN trong quản lý, rồi tìm tin này ở trang công khai. Reload sau tối đa một phút nếu cần. | Tin hết hạn tự thành Tạm ẩn, có nhãn/cảnh báo Đã hết hạn và không còn trong tìm kiếm. Dữ liệu đã quá hạn được script chuẩn bị sẵn. |

## 8. S2-04 — tìm kiếm và lọc (5 AC)

Không cần đăng nhập. Mở `/TimTin`.

| AC | Thao tác từng bước | Kỳ vọng |
| --- | --- | --- |
| 1 | Chọn Quận 1, giá 2.000.000–3.000.000, diện tích 20–30, số người tối đa 2; bấm Tìm kiếm. Kiểm tra từng tin. | Tất cả kết quả thỏa đồng thời các bộ lọc. Bộ lọc sức chứa là chọn đúng mức tối đa của phòng. |
| 2 | Tìm tiêu đề A105-NHAP/A106-HET-HAN/A107-TAM-AN/PERF-003 trong kết quả; thử URL chi tiết tin 4/5/6/11 khi anonymous. | Không xuất hiện; chi tiết không công khai trả 404. Chỉ tin Đang hiển thị còn hạn và phòng trống/tòa hoạt động được trả. |
| 3 | Bỏ lọc; đổi Mới đăng nhất, Giá tăng dần, Giá giảm dần. Sang trang 2. | Giá đúng thứ tự; tối đa 12 tin/trang, giữ bộ lọc khi chuyển trang; bấm tên/ảnh mở đúng chi tiết. |
| 4 | Giữ nguyên bộ mẫu trước khi tạo thêm tin. Chạy lệnh đo HTTP ở mục 15 trong terminal thứ hai. Hoặc DevTools→Network→Disable cache, reload và xem request document `/TimTin`. | Database có 500 tin; HTTP hoàn tất dưới 2.000ms. 500 là tổng tin trong DB, không phải 500 tin đều công khai. Không tính thời gian khởi động server vào truy vấn. |
| 5 | Giá tối thiểu 50.000.000, giá tối đa 60.000.000, Tìm kiếm. Bấm Áp dụng khoảng giá gợi ý. | Có thông báo không tìm thấy và gợi ý nới giá; trang không trắng; áp dụng gợi ý cập nhật bộ lọc. |

## 9. S2-05 — chi tiết và chi phí (4 AC)

Mở cửa sổ ẩn danh chưa đăng nhập, truy cập `/TinDang/ChiTiet/1`. Dùng mẫu sạch trước khi duyệt yêu cầu 1.

| AC | Thao tác từng bước | Kỳ vọng |
| --- | --- | --- |
| 1 | Xem ảnh lớn/dải ảnh và thông tin phòng. Bấm từng thumbnail. | 3 ảnh đúng thứ tự; diện tích 25m², thuê 3.500.000, tối đa 3 người, cọc 3.500.000 và mô tả. |
| 2 | Đọc bảng dịch vụ A101; mở thêm tin 3 ở Beta. | A101 điện riêng 4.200 đ/kWh, nước 80.000 đ/người/tháng; Internet 150.000 và gửi xe 70.000/tháng. Beta nước 18.000 đ/m³. A102 không xuất hiện gửi xe. |
| 3 | Xem tổng đầu tháng của tin 1, rồi tin 2. | Tin 1 = 3.720.000; tin 2 = 3.650.000. Có chú thích chưa gồm điện/nước theo sử dụng. Tiền cọc hiển thị riêng, không cộng vào công thức AC này. |
| 4 | Khi chưa đăng nhập, bật DevTools→Device toolbar, chọn Responsive width 360px, reload, cuộn hết trang. | Nội dung đọc được, ảnh co theo chiều rộng, không tràn ngang toàn trang; vẫn xem được thông tin và bảng giá. |

## 10. S2-06 — gửi yêu cầu (5 AC)

Đăng nhập `tenant.demo@demo.local`, mở `/TinDang/ChiTiet/8` (A108, tối đa 2 người). Tạo mẫu mới nếu tin này đã có yêu cầu mở.

| AC | Thao tác từng bước | Kỳ vọng |
| --- | --- | --- |
| 1 | Kiểm tra form có loại Xem phòng/Thuê ngay, ngày, số người và lời nhắn. Chọn Xem phòng, ngày hôm nay+2, 2 người, lời nhắn mẫu; chỉ gửi sau khi làm AC2/3. | Có đủ trường; loại yêu cầu được lưu đúng. Dùng tài khoản khách thứ hai để thử Thuê ngay trên cùng tin sau đó. |
| 2 | Thử hôm qua và hôm nay+61. Thử hôm nay và hôm nay+60 trong form (có thể chỉ xem validation, chưa gửi). | Ngày quá khứ/+61 bị chặn; hai mốc hôm nay/+60 hợp lệ. Bộ test DesiredDate xác minh cả backend và ranh giới thời gian Việt Nam. |
| 3 | Nhập 3 người và ngày hợp lệ; bấm gửi. Sau đó sửa 2 người. | 3 người bị từ chối, thông báo tối đa 2. Không tạo yêu cầu lỗi. |
| 4 | Gửi một lần hợp lệ với 2 người. Mở lại tin và thử gửi nữa. | Không tạo yêu cầu mở thứ hai của cùng khách/tin; hiển thị/chỉ dẫn tới yêu cầu cũ. Khách khác vẫn có thể gửi yêu cầu riêng. |
| 5 | Xem màn hình thành công sau lần gửi hợp lệ. Ghi mã và mở Yêu cầu của tôi. | Có mã `YC-yyyyMM-xxxx` ngay. Trên mẫu mới trong cùng tháng, lần đầu thường là `...-0008`. Không bắt buộc chính xác 0008 nếu đã gửi trước. |

## 11. S2-07 — danh sách yêu cầu Chủ nhà (4 AC)

Đăng nhập Chủ nhà, mở `/YeuCau`.

| AC | Thao tác từng bước | Kỳ vọng |
| --- | --- | --- |
| 1 | Xem yêu cầu 1/2/3/4. | Có mã, tên khách, điện thoại, phòng, loại, ngày mong muốn và trạng thái; không thiếu cột. |
| 2 | Lần lượt chọn Mới, Đã hẹn lịch, Đã duyệt, Từ chối, Đã hủy. Chọn Alpha rồi Beta. | Mỗi trạng thái có dữ liệu mẫu; chọn Beta thấy yêu cầu 4, Alpha không thấy yêu cầu 4. Bộ lọc kết hợp đúng. |
| 3 | Bỏ lọc, quan sát thứ tự và yêu cầu 2/3. | Mới nhất trước; yêu cầu chưa xử lý hơn 24 giờ được đánh dấu nổi bật. Mới và Đã hẹn lịch được coi là chưa xử lý. |
| 4 | Trên mẫu sạch xem badge Yêu cầu ở menu. Dùng khách gửi thêm một yêu cầu rồi refresh Chủ nhà. Duyệt/từ chối một yêu cầu và refresh. | Ban đầu badge **4** (ID 1–4). Gửi mới tăng 1; duyệt/từ chối/hủy giảm 1; đổi Mới→Đã hẹn lịch không giảm. Giá trị có thể khác nếu đã thao tác trước. |

## 12. S2-08 — lịch hẹn và xử lý (4 AC)

Đăng nhập Chủ nhà. Nên bắt đầu từ mẫu sạch cho các kịch bản sau.

| AC | Thao tác từng bước | Kỳ vọng |
| --- | --- | --- |
| 1 | 1. Mở `/LichHen/ChiTiet/2`. 2. Chọn đúng ngày/giờ đã có ở `$demo.clashVietnam` hoặc xem `/LichHen/ChiTiet/3`. 3. Quan sát cảnh báo; thử lệch 15 phút. 4. Chọn giờ cách trên 30 phút và xác nhận. 5. Mở lại, nhập lịch mới rồi Lưu lịch mới. | Phải có ngày và giờ cụ thể; cảnh báo trùng cùng phòng trong 30 phút, kể cả mốc 30 phút. Xác nhận/đổi lịch lưu thành công với giờ cụ thể; giờ hiển thị theo Việt Nam. |
| 2 | 1. Mở `/LichHen/ChiTiet/4`. 2. Thử Từ chối không chọn lý do. 3. Chọn Lý do khác, không ghi chú; thử lại. 4. Ghi chú `Khách muốn thuê quá số người cho phép`, xác nhận từ chối. | Bắt buộc lý do; danh sách có đủ 4 lựa chọn. Lý do khác cần ghi chú hợp lệ (tối thiểu 5 ký tự). Lý do/ghi chú được giữ để khách xem. |
| 3 | Sau xác nhận, đổi lịch, từ chối, mở mục Lịch sử trong chi tiết. Đăng nhập Khách chính và mở yêu cầu 2/4. | Mỗi thao tác có người thực hiện, thời điểm và trạng thái/lịch cũ–mới; khách xem được lịch sử của mình, không có nút xử lý của Chủ nhà. |
| 4 | Mở `/LichHen/ChiTiet/1`, bấm Duyệt thuê ngay. Sau đó mở quản lý phòng và tin; mở nút Lập hợp đồng. | Yêu cầu Đã duyệt; A101 Đã đặt cọc; tin 1 Đã cho thuê, không còn public; nút hợp đồng mở được. Màn hình soạn hợp đồng thuộc S3-01 nên hiện thông báo chưa triển khai, không phải form ký hợp đồng. |

## 13. S2-09 — theo dõi/hủy của khách (4 AC)

Đăng nhập Khách chính, mở `/YeuCau`.

| AC | Thao tác từng bước | Kỳ vọng |
| --- | --- | --- |
| 1 | Xem danh sách Yêu cầu của tôi. Nếu đã xác nhận lịch yêu cầu 2 thì xem dòng đó. | Có mã/phòng/ngày gửi/trạng thái/lịch hẹn; chỉ yêu cầu của tài khoản này. Không thấy yêu cầu 3 thuộc khách thứ hai. |
| 2 | Xem yêu cầu mẫu 5 và yêu cầu 4 vừa bị từ chối. Mở chi tiết. | Hiển thị nhãn lý do chủ nhà chọn; chi tiết/lịch sử có ghi chú Lý do khác đã nhập. |
| 3 | 1. Hủy một yêu cầu Mới (ID 1 nếu chưa duyệt, hoặc yêu cầu A108 mới gửi). 2. Hủy yêu cầu 2 sau khi hẹn. 3. Quan sát yêu cầu 6 đã hủy và 7 đã duyệt. | Chỉ Mới/Đã hẹn lịch có nút Hủy; có xác nhận; hủy xong Đã hủy và không khôi phục. Đã hủy/Đã duyệt không có nút hủy. Hủy xong có thể gửi yêu cầu mới cùng tin nếu tin còn public. |
| 4 | Bấm Mở tin đăng từ các dòng, gồm yêu cầu 7 với tin đã cho thuê. Mở URL tin đã cho thuê bằng cửa sổ anonymous. | Đúng tin tương ứng. Khách đã gửi được xem tin không còn public ở chế độ chỉ đọc, có cảnh báo và không có form gửi mới; anonymous/khách không liên quan không xem được. |

## 14. S2-10 — điện nước theo tòa (4 AC)

Đăng nhập Chủ nhà, mở `/DichVu/DienNuoc?toaNhaId=1`. Làm cuối để giữ giá các bài test trước.

| AC | Thao tác từng bước | Kỳ vọng |
| --- | --- | --- |
| 1 | Xem Alpha rồi Beta. Ở Alpha chọn điện Theo đầu người, nước Theo chỉ số. | Mỗi dịch vụ có hai cách độc lập; Alpha ban đầu nước khoán, Beta nước đồng hồ. |
| 2 | Nhập điện 95.000/người/tháng, nước 18.000/m³ theo lựa chọn ở AC1. Quan sát nhãn/ô nhập. | Ô/nhãn đúng đơn vị cho từng cách; không nhầm giá/kWh với tiền/người. |
| 3 | 1. Ghi kỳ đang áp dụng. 2. Xem kỳ kế tiếp trước Lưu. 3. Lưu cấu hình hợp lệ. 4. Mở lại và xem cấu hình hiện tại/cấu hình chờ. 5. Xem hóa đơn cũ ID1. | Cách/giá hiện tại không đổi; cấu hình chờ bắt đầu kỳ kế tiếp. Hóa đơn cũ không đổi. Giá riêng 4.200 của A101 được giữ, tính theo đơn vị mới từ kỳ sau như thông báo trên form; cần chỉnh giá riêng nếu muốn 95.000/người. |
| 4 | Trước lần lưu hợp lệ, để trống giá của cách đang chọn, thử 0, rồi -1; bấm Lưu từng lần. | Bị từ chối và có thông báo; không tạo phiên bản cấu hình thiếu giá hoặc 0. Test tự động kiểm tra transaction không lưu dở. |

## 15. Chạy kiểm tra tự động và kiểm tra kho ảnh

Mở **terminal thứ hai**, đặt thư mục repo. Bộ test xUnit tạo SQLite tạm, không sửa database demo/cá nhân:

```powershell
Set-Location D:\TTCS_T926_K12C3_N1
dotnet test .\Tests\QL_PhongTro.Tests\QL_PhongTro.Tests.csproj --logger 'trx;LogFileName=sprint2.trx' --results-directory .\data\test-results
```

Kỳ vọng dòng cuối `Failed: 0`. Log `.trx` nằm trong `data/test-results`. Cảnh báo NU1900 hoặc ImageSharp khác với test FAILED; không bỏ qua lỗi build/test.

Chạy riêng nhóm cần đối chiếu:

```powershell
# S2-01 / S2-10
dotnet test .\Tests\QL_PhongTro.Tests\QL_PhongTro.Tests.csproj --filter 'FullyQualifiedName~RoomServices'
# S2-02
dotnet test .\Tests\QL_PhongTro.Tests\QL_PhongTro.Tests.csproj --filter 'FullyQualifiedName~RoomImage'
# S2-03 / S2-05 và kết nối tìm kiếm
dotnet test .\Tests\QL_PhongTro.Tests\QL_PhongTro.Tests.csproj --filter 'FullyQualifiedName~PermissionTests'
# S2-06 ngày mong muốn
dotnet test .\Tests\QL_PhongTro.Tests\QL_PhongTro.Tests.csproj --filter 'FullyQualifiedName~DesiredDate'
# S2-07 / S2-08 / S2-09
dotnet test .\Tests\QL_PhongTro.Tests\QL_PhongTro.Tests.csproj --filter 'FullyQualifiedName~LichHen|FullyQualifiedName~YeuCau'
```

Đo HTTP tìm kiếm trên server demo đang chạy, 5 lần, lấy toàn bộ HTML:

```powershell
$demoFolder = (Get-Content .\data\sprint2-demo\latest.txt -Raw).Trim()
$demo = Get-Content (Join-Path $demoFolder 'access.json') -Raw | ConvertFrom-Json
1..5 | ForEach-Object {
    $timer = [Diagnostics.Stopwatch]::StartNew()
    $response = Invoke-WebRequest "$($demo.url)/TimTin?QuanHuyen=Qu%E1%BA%ADn%201&SapXep=gia-tang"
    $timer.Stop()
    [pscustomobject]@{ Lan = $_; HTTP = $response.StatusCode; Milliseconds = [math]::Round($timer.Elapsed.TotalMilliseconds,2); Duoi2Giay = $timer.Elapsed.TotalMilliseconds -lt 2000 }
} | Format-Table
```

Để kiểm tra schema/kho ảnh sau upload/xóa, dừng web bằng Ctrl+C rồi chạy trong thư mục app:

```powershell
$demoFolder = (Get-Content .\data\sprint2-demo\latest.txt -Raw).Trim()
$demo = Get-Content (Join-Path $demoFolder 'access.json') -Raw | ConvertFrom-Json
$demoRuntime = Join-Path (Get-Location) 'data\sprint2-demo\runtime\QL_PhongTro.dll'
Push-Location .\QL_PhongTro
try {
    dotnet $demoRuntime --environment Development --DatabasePath $demo.database --check-database
    dotnet $demoRuntime --environment Development --DatabasePath $demo.database --RoomImagesPath $demo.roomImagesPath --check-room-image-storage
} finally { Pop-Location }
```

Kỳ vọng schema ready; kho ảnh không có tệp mồ côi hoặc tệp bị thiếu sau xóa thành công. Nếu có xóa đang chờ do lỗi lưu trữ, sửa lỗi truy cập và khởi động lại web để retry; giữ nguyên database demo để kiểm tra.

## 16. Lỗi thường gặp

| Hiện tượng | Cách xử lý |
| --- | --- |
| Không mở được localhost | Terminal phải còn chạy và đã có Now listening. Kiểm tra cổng trong access.json. |
| Cổng đang dùng / address already in use | Dừng đúng phiên cũ bằng Ctrl+C; đừng mở hai Start script cùng cổng. |
| DLL đang bị khóa, MSB3021/MSB3027 | Dừng server demo trước New script/build runtime demo. Không cần xóa DB. |
| Đăng nhập bị quay về / dữ liệu khác mong đợi | Đăng xuất, mở cửa sổ ẩn danh mới, xác nhận script dùng latest.txt của bản mẫu đúng. |
| Email cần mã xác nhận | Tài khoản mẫu đã được xác nhận; kiểm tra bạn có chạy đúng database mẫu hay đang dùng database mặc định. |
| Tin A101 không còn public | Bạn đã duyệt yêu cầu 1; đúng hành vi. Tạo bộ mẫu mới để test lại S2-05/S2-06. |
| Ngày mẫu cũ hoặc không còn gửi được | Tạo bản mới; script tính ngày mong muốn/lịch hẹn/hạn theo ngày chạy thực tế. |
| NU1900 | Không truy cập được dữ liệu vulnerability NuGet; đọc dòng Build succeeded/Failed và kết quả test để phân biệt. |
| Cảnh báo ImageSharp license | Build có thể tiếp tục; cần xử lý giấy phép trước phát hành sản phẩm. |

Tài liệu này nghiệm thu Sprint 2. Chức năng soạn hợp đồng S3-01, mạng 3G thực tế và thiết bị cảm ứng vật lý cần bài nghiệm thu riêng; không đánh PASS cho các phần đó chỉ từ bộ test hiện tại.

## 17. Kết quả xác minh bản bàn giao

- Build và tạo mẫu thành công; đúng 500 tin, 5 tài khoản, 7 yêu cầu đủ 5 trạng thái; integrity/FK PASS.
- Đăng nhập HTTP thật bằng cả 5 email/mật khẩu trên: PASS; kiểm tra 10 trang chủ nhà, trang khách, trang công khai và quyền Chủ nhà khác: PASS.
- Hai hóa đơn cũ có tổng 3.720.000 và 3.650.000 đúng kỳ vọng.
- Kho ảnh: 6 tệp gốc/thumbnail, 0 missing, 0 orphan.
- 5 lần đo HTTP tìm kiếm trên bộ mẫu: 11,715–38,018ms.
- Full xUnit: 250/250 PASS; log `data/test-results/sprint2-demo-regression.trx`.
- Kết quả kiểm tra mẫu: `verification.json` trong cùng thư mục `access.json`. Nếu có Python 3, có thể chạy lại bằng `python verification/verify_sprint2_demo.py` khi server đang chạy và mẫu còn nguyên; script từ chối các mẫu đã bị thay đổi số lượng/trạng thái bởi bài test thủ công.
