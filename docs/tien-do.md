# Tiến độ và bàn giao dự án

## Đồng bộ màu giao diện Admin (06/10/2026)

- ADMIN dùng homepage-dashboard-theme.css cùng chủ nhà, quản lý và khách thuê: xanh #008060, nền trắng/xám nhạt, đồng bộ menu, nút, biểu mẫu với trang chủ công khai.
- Build thành công; đăng nhập AJAX và phiên Admin đạt; trang phân quyền, phòng trọ, tòa nhà, yêu cầu thuê HTTP 200 và nạp đúng theme. Trang chủ công khai kiểm tra đạt. Đã chạy lại localhost:5247, 7 tài khoản demo đăng nhập đạt; không chụp ảnh.

## Đồng bộ màu giao diện quản lý (06/10/2026)

- Vai trò QUAN_LY dùng chung homepage-dashboard-theme.css với chủ nhà và khách thuê: xanh #008060, nền trắng/xám nhạt, menu/nút/biểu mẫu theo trang chủ công khai.
- Build thành công; đăng nhập AJAX và phiên quản lý đạt; các trang phòng trọ, tòa nhà, quản lý tin, yêu cầu thuê HTTP 200 và nạp đúng theme. Kiểm tra phạm vi theme chủ nhà/khách thuê/admin đạt. Đã chạy lại localhost:5247, 7 tài khoản demo đăng nhập đạt; không chụp ảnh.

## Đồng bộ màu giao diện chủ nhà (06/10/2026)

- Chủ nhà và khách thuê dùng chung homepage-dashboard-theme.css: xanh #008060, nền trắng/xám nhạt, viền xám; đồng bộ menu, nút, biểu mẫu và nhãn ảnh chính. Trạng thái giữ chỗ dùng vàng nhạt, lỗi dùng đỏ để phân biệt rõ. Các vai trò khác giữ theme hiện có.
- Build thành công; kiểm tra HTTP 200 và theme trên các trang phòng trọ, tòa nhà, dịch vụ, yêu cầu thuê, quản lý tin của chủ nhà và hồ sơ/yêu cầu của khách thuê. Trang công khai/admin không nạp theme mới. Server localhost:5247 đã chạy lại và 7 tài khoản demo đăng nhập đạt; không chụp ảnh.

## Đồng bộ màu giao diện khách thuê (06/10/2026)

- Dashboard KHACH_THUE dùng xanh #008060, hover #006e52, nền trắng/xám nhạt và viền xám theo trang chủ công khai. Đồng bộ menu, nút, biểu mẫu, trạng thái thành công và phân trang qua tenant-theme.css, chỉ nạp cho vai trò khách thuê.
- Build thành công; HTTP trang Yêu cầu thuê và Hồ sơ cá nhân 200, có theme khách thuê; trang chủ công khai và giao diện chủ nhà không nạp theme này. Đã chạy lại tại localhost:5247, kiểm tra đăng nhập 7 tài khoản demo đạt. Không chụp ảnh.

## Đồng bộ giao diện xác nhận email (06/10/2026)

- ConfirmEmail dùng layout/CSS đăng nhập, hai cột ảnh nội thất và form nền kem, logo NhàTốt, nút xác nhận xanh và gửi lại mã dạng viền. Giữ Email/Code, CSRF, validation, thông báo lỗi/trạng thái, đường dẫn email local và formnovalidate của Gửi lại mã; mã có inputmode numeric/one-time-code và giới hạn 6 số. Có liên kết về đăng nhập.
- Build và HTTP GET PASS, đủ trường/CSRF/route gửi lại; server 5247 đã nạp bản mới, 7 tài khoản demo đăng nhập đạt. Không chụp ảnh, không gửi email hoặc xác nhận mã thử trong lượt chỉnh giao diện.

## Đồng bộ giao diện quên mật khẩu (06/10/2026)

- ForgotPassword dùng layout/CSS đăng nhập, ảnh nội thất bên trái, logo NhàTốt và form email nền kem/nút xanh bên phải; dùng cùng responsive mobile. Giữ thông báo ResetRequestMessage, validation, CSRF và liên kết về đăng nhập; không đổi nghiệp vụ gửi email.
- Build PASS; HTTP 200 và kiểm tra trường email/CSRF/branding đạt. Server 5247 đã nạp bản mới; 7 tài khoản demo đăng nhập đạt. Không chụp ảnh, không gửi email thử trong lượt chỉnh giao diện.

## Đồng bộ trang đăng ký với đăng nhập (06/10/2026)

- Trang Register dùng layout/CSS đăng nhập, hai cột ảnh nội thất và form nền kem, logo NhàTốt trên ảnh dùng chữ sáng, nút xanh; mobile dùng một cột. Giữ họ tên/email/điện thoại/mật khẩu, validation, CSRF và hiện/ẩn mật khẩu. Điều chỉnh khoảng cách cho form dài.
- Build PASS; HTTP Register 200, đủ trường/CSRF/logo; server 5247 đã nạp bản mới, đăng nhập 7 tài khoản demo đạt. Không chụp ảnh, không gửi đăng ký mới/email, không đổi mã nghiệp vụ đăng ký.

## Đồng nhất thương hiệu trang đăng nhập (06/10/2026)

- Hai vị trí logo trên trang đăng nhập dùng cùng `images/nhatot-logo.svg` với trang chủ: NhàTốt - SỐNG ĐÚNG NƠI. Tiêu đề tab và nhãn trợ năng đổi từ Nhà Mộc sang NhàTốt; logo trên ảnh có nền kem để dễ đọc.
- Build và xác minh HTML qua HTTP PASS; server 5247 đã nạp bản mới, đăng nhập 7 tài khoản demo đạt. Không chụp ảnh, không đổi dữ liệu/mật khẩu.

## Trang đăng nhập theo ảnh Nhà Mộc (06/10/2026)

- Layout đăng nhập riêng hai cột: ảnh nội thất hiện có bên trái, logo/nội dung theo ảnh tham chiếu, form nền kem và nút xanh bên phải. Mobile hiển thị form một cột. Không chụp ảnh.
- Giữ validation, CSRF, AJAX nhận token, ReturnUrl, khóa tài khoản/countdown, quên mật khẩu, đăng ký và nút hiện/ẩn mật khẩu. Thêm RememberMe gửi qua form/fetch và điều khiển cookie persistent trong Login.
- Build PASS; HTTP kiểm tra trang/assets, đăng nhập 7 tài khoản demo và cookie phiên/persistent PASS. Server 5247 đã nạp runtime mới; mật khẩu demo vẫn Demo@2026. Chưa xác minh bố cục trực quan trong browser; ảnh nền dùng asset có sẵn nên không giống hoàn toàn ảnh tham chiếu. Chưa commit/push.

## Bộ dữ liệu sạch nghiệm thu S2-01–S2-10 trong bản - test (05/10/2026)

- Đọc hướng dẫn, yêu cầu, mô hình SQLite/DBML và tiến độ; đối chiếu bộ demo cũ ở dự án gốc. Bộ cũ có 8 tài khoản và yêu cầu 8 đã bị đổi trạng thái, nên tạo bộ mới riêng bằng Sprint2DemoSeeder, không ghi hoặc xóa database cũ.
- Bộ mới trong `data/sprint2-demo/20261005-172234-428e1c`, được chọn qua `latest.txt`; server riêng `http://localhost:5271`. Có 7 tài khoản đủ 4 vai trò, 4 tòa (A sở hữu A1/A2, B sở hữu B1/B2), 45 phòng, 35 tin công khai và 6 tin bị loại. Chọn 35 thay vì 36 để trang cuối lẻ: 12/12/11.
- A1 có 5 dịch vụ mặc định có giá, gửi xe chỉ ở các phòng mẫu tầng trệt, có giá riêng và 2 hóa đơn tháng trước. A2 chưa cấu hình. Điện A1 theo chỉ số, nước theo người; cấu hình nước chờ kỳ 11/2026.
- 8 yêu cầu đủ 5 trạng thái ở 2 tòa; có yêu cầu quá 24 giờ và vừa tạo, 2 lịch cùng phòng cách 15 phút, Thuê ngay chờ duyệt; khách 1 chưa gửi trên A108, khách 2 có yêu cầu mở trên tin đó.
- Nạp 5 tệp ảnh nội thất JPG/PNG qua HTTP từ các asset hero hiện có (3 asset, có ảnh lặp). Kiểm tra thumbnail rộng 400px, không thiếu tệp/mồ côi. Thử tệp quá 5MB, JPG giả và ảnh thứ 9 đều bị từ chối; phục hồi phòng placeholder về 0 ảnh.
- Xác minh HTTP đăng nhập 7 tài khoản, các trang dịch vụ/tin/yêu cầu/hóa đơn, Chủ nhà B nhận đúng 403, phân trang 12/12/11 và áp dụng gợi ý giá tìm được tin. SQLite integrity/FK đạt. Tài khoản, mật khẩu chung và bản đồ ID ghi trong `access.json`/`report.md`, kết quả trong `verification.json`; tất cả thuộc `data/` đã ignore.
- Không đổi schema/mã nghiệp vụ, không thay `.env.local` đang chọn database cũ và không dừng server 5247. Chạy bộ mới bằng `verification/Start-Sprint2Demo.ps1` (đọc latest mới). S2-08 chỉ chuẩn bị lập hợp đồng; S3-01 chưa triển khai.

## Giao diện trang chủ NhàTốt khi chưa đăng nhập và đồng bộ điều hướng (05/10/2026)

- Thiết kế trang chủ theo nhận diện NhàTốt khi chưa đăng nhập: Logo NhàTốt - SỐNG ĐÚNG NƠI, thanh điều hướng (Trang Chủ, Tìm Phòng, Tin Đăng Cho Thuê, Giới Thiệu, Đăng nhập, Đăng ký), hero banner tràn viền với ảnh phòng khách hiện đại và slogan "Tìm chốn ở phù hợp, bắt đầu cuộc sống mới."
- Trang chủ hiện chỉ hiển thị hero banner; form tìm nhanh tại trang chủ chưa có trong phiên bản hiện tại.
- Controller chuẩn bị danh sách tin mới nhất, nhưng view trang chủ hiện chưa hiển thị danh sách này.
- Thêm trang Giới thiệu tại `/Home/GioiThieu` với 3 cam kết cốt lõi (Giá thật minh bạch, Ảnh thật xác thực, Liên hệ trực tiếp) và quy trình 4 bước thuê phòng.
- Khi đã đăng nhập, người dùng tiếp tục sử dụng không gian quản lý bình thường; layout công khai `_PublicLayout.cshtml` được đồng bộ cho cả `/`, `/TimTin`, `/TinDang` và `/Home/GioiThieu`.
- Kiểm thử ngày 05/10/2026 trước khi hợp nhất bản sửa email: Build PASS; 262/263 ca đạt, 1 ca trang chủ thất bại vì thiếu form tìm nhanh. Test còn kỳ vọng tin nổi bật và cam kết trên trang chủ. Không đổi schema CSDL.

## Đồng bộ nhánh dev mới nhất (05/10/2026)

- Trước khi pull đã dừng ứng dụng, xác nhận working tree sạch và sao lưu nguyên fixture local ra `D:\DevSprint2\database-backups\sprint2-demo-before-pull-20261005-ca363ac2`; SHA-256 database nguồn/bản sao khớp nhau.
- `git pull --ff-only origin dev` thành công, fast-forward từ `66ec95e` lên `66376d2`, không xung đột. `.env.local`, database và kho ảnh local vẫn tồn tại, vẫn được Git ignore; hash database không đổi sau pull.
- Xác minh sau pull: `run.bat --check-only` PASS, dùng đúng fixture `data/sprint2-demo/20261004-185115-e98174/sprint2.sqlite`; schema sẵn sàng. Còn cảnh báo NU1900/ImageSharp/nullable có sẵn, không có lỗi build hoặc schema. Không chạy updater và không ghi đè database.

## Thu gọn từ chối và chuẩn hóa giờ hẹn (05/10/2026)

- Đặt nút Từ chối ngay cạnh ô lý do; ghi chú Lý do khác nằm ở hàng dưới và ẩn cả cột khi không cần, bỏ khoảng trống đẩy nút ra xa.
- Ô xác nhận/đổi lịch dùng định dạng 24 giờ HH:mm, ví dụ 19:30, tối đa 5 ký tự và bàn phím cho phép nhập dấu hai chấm. Chuẩn hóa 1930 thành 19:30 và 9:30 thành 09:30; giờ ngoài 00:00–23:59 báo lỗi. Razor định dạng giờ với dấu hai chấm cố định và invariant culture. Giữ giá trị ngày giờ ISO gửi backend và cảnh báo trùng lịch.
- Không chụp ảnh. Kiểm tra cú pháp JavaScript PASS; build Razor để xác minh. Cần khởi động lại app tại terminal giữ cấu hình SMTP để nạp giao diện mới. Chưa push.

## Sắp xếp giá và phân trang danh sách tin (05/10/2026)

- `/TinDang` thêm Mới đăng nhất/Giá tăng dần/Giá giảm dần, phân trang ở database 12 tin/trang, hiển thị giá thuê và tổng tin/trang. Link Trang trước/Trang sau giữ cách sắp xếp; áp dụng sắp xếp về trang đầu. Form lọc sang `/TimTin` truyền cách sắp xếp đang chọn; `/TimTin` đã có lọc và phân trang 12 từ trước.
- Chỉ lấy tin công khai hợp lệ, giá trùng dùng ID giảm dần; chuẩn hóa cách sắp xếp lạ và giới hạn trang vào phạm vi hợp lệ. Không sửa schema/dữ liệu thật, không chụp ảnh.
- Build PASS; HTTP integration test PASS trên SQLite tạm với 25 tin hợp lệ và 1 nháp: thứ tự tăng/giảm đúng, trang 12/12/1, trang quá lớn về trang cuối, link giữ cách sắp xếp. Cần khởi động lại app trong terminal đã cấu hình SMTP để nạp thay đổi. Chưa push.

## Hiển thị trạng thái trong màn sửa tin (05/10/2026)

- Thêm trường chỉ đọc “Trạng thái tin đăng” tại phần Nội dung tin của màn chỉnh sửa; DANG_HIEN_THI hiển thị “Đã hiển thị” theo yêu cầu. Các trạng thái còn lại hiển thị Nháp, Tạm ẩn, Đã cho thuê. Trạng thái lấy từ tin đã lưu, giữ luồng đăng/lưu/gỡ hiện có. Không chụp ảnh; cần khởi động lại ứng dụng để nạp Razor mới.

## Bỏ nút mũi tên sắp xếp ảnh (05/10/2026)

- Theo yêu cầu người dùng, bỏ hai nút lên/xuống khỏi thẻ ảnh có sẵn và thẻ ảnh tạo sau upload; bỏ handler JavaScript tương ứng. Giữ kéo thả và nút Xóa. Kiểm tra cú pháp JavaScript và diff đạt; không chụp ảnh. Razor cần build/khởi động lại ứng dụng để nạp thay đổi.

## Sửa kéo thả thứ tự ảnh phòng (05/10/2026)

- `room-images.js` luôn chấp nhận drop trong grid khi đang kéo, gồm trên chính thẻ vừa di chuyển và khoảng trống; trước đây các vị trí này không preventDefault nên drop bị hủy và dragend phục hồi thứ tự cũ. Tắt native drag của img để trình duyệt kéo thẻ, không kéo riêng tệp ảnh; không khóa chuột dựa vào pointer coarse; nút thao tác không bắt đầu drag. Vị trí trước/sau dựa vào nửa trái/phải của thẻ trong grid.
- Kiểm tra cú pháp JS PASS. Kiểm tra trực tiếp browser localhost:5247: kéo ảnh ID 3 lên đầu nhận “Đã lưu thứ tự ảnh”, reload giữ thứ tự 3/1/2/4/5; dùng mũi tên đưa về 1/2/3/4/5. Ảnh xác minh tại data/room-drag-fixed.png đã ignore. Chỉ sửa static JS, không cần restart web; người dùng reload trang để nạp bản mới. Chưa push trong lượt này.

## Giới hạn quyền hồ sơ khách thuê (05/10/2026)

- Khách thuê chỉ xem/sửa hồ sơ gắn với tài khoản đang đăng nhập; endpoint xem chi tiết chặn hồ sơ khác ở backend và không còn form nhập mã hồ sơ bất kỳ. Ảnh giấy tờ áp dụng cùng phạm vi quyền.
- Chủ nhà có menu Hồ sơ khách thuê và danh sách `/HoSo/DanhSach`, chỉ gồm người đứng tên/người ở ghép của hợp đồng tại tòa thuộc mình. Chủ nhà xem chi tiết/ảnh nhưng GET/POST sửa và xóa đều bị chặn. Danh sách không trùng khi khách có nhiều hợp đồng.
- Diễn giải yêu cầu “đã có hợp đồng”: hợp đồng CHO_HIEU_LUC, DANG_HIEU_LUC hoặc DA_KET_THUC cấp quyền; NHAP/DA_HUY và yêu cầu thuê không cấp quyền. Giữ quyền ADMIN hiện có. Schema hợp đồng chưa cài/không tương thích không cấp quyền; bảng người ở ghép là tùy chọn.
- Build riêng tại data/profile-check PASS; 4/4 kiểm thử hồ sơ PASS, gồm truy cập hồ sơ/ảnh người khác, hợp đồng nháp/kết thúc, đổi chủ tòa, danh sách chỉ đúng khách, chủ nhà không sửa được và hồi quy tạo/sửa/xóa hồ sơ cá nhân. Không đổi schema/database thật.
- Phiên web của người dùng vẫn chạy mã cũ để giữ cấu hình SMTP trong terminal. Cần Ctrl+C rồi chạy lại run.bat tại chính terminal đó để nạp bản sửa. Chưa nghiệm thu UI trực quan.

## Thống nhất cổng localhost (05/10/2026)

- Theo yêu cầu người dùng, chuyển bộ demo `20261005-012411-0d3b1e` sang http://localhost:5247; cập nhật access.json, report.md và hướng dẫn test local. Giữ database, kho ảnh và mật khẩu demo hiện tại.
- `.env.local` đã ignore trỏ run.bat tới đúng database/kho ảnh/khóa/email pickup của bộ demo, cùng PublicBaseUrl http://localhost:5247. run.bat đọc thêm PublicBaseUrl và truyền URL 5247 tường minh; New-Sprint2Demo mặc định dùng 5247. Hướng dẫn Sprint 2 dùng cổng 5247.
- Dừng đúng hai tiến trình của dự án từng chạy ở 5248 và 5270 để tập trung sử dụng cổng 5247. Những lần sau chạy run.bat ở gốc repository; server cần đang chạy để truy cập website.

## Tạo bộ nghiệm thu Sprint 2 mới (05/10/2026)

- Đọc hướng dẫn, yêu cầu Sprint 2, mô hình DBML và tiến độ; dùng seeder hiện có tạo fixture mới `data/sprint2-demo/20261005-012411-0d3b1e`, runtime riêng, chạy tại http://localhost:5270. Không ghi database cá nhân. Credential chung của 7 tài khoản nằm trong access.json và report.md đã ignore.
- 35 tin công khai ở 4 quận/huyện, 6 tin bị loại; chọn 35 để trang cuối lẻ (12/12/11). Khách 1 chưa có yêu cầu trên tin A108 nhưng có yêu cầu khác phục vụ tự hủy. Hóa đơn cũ kỳ 09/2026, cấu hình hiện tại 10/2026 và nước chờ 11/2026.
- Build PASS; upload 5 ảnh PNG qua HTTP PASS; integrity/FK, đăng nhập 7 tài khoản và smoke test PASS. Xác minh riêng 3 trang HTTP 12/12/11, thumbnail rộng 400px, Chủ nhà B mở dịch vụ phòng A1 trả 403. Hướng dẫn từng AC theo cổng mới nằm ở huong-dan-test.md cạnh report.md.
- Python mặc định trỏ Microsoft Store; hoàn tất upload/xác minh bằng Python runtime có sẵn của Codex. latest.txt chỉ được cập nhật sau kiểm tra thành công. Hai thư mục chuẩn bị chưa hoàn tất trước đó được giữ, không chọn làm mẫu bàn giao.
- Chưa chạy full xUnit, nghiệm thu UI/responsive/3G, SMTP thật hoặc hiệu năng 500 tin trong phiên này. Email demo dùng pickup; các tài khoản đã xác nhận email. Server đang chạy để người dùng tự nghiệm thu các thao tác làm thay đổi dữ liệu.

## Dùng fixture Sprint 2 làm database local (04/10/2026)

- Không chép đè `QL_PhongTro/Data/local-dev.sqlite` vì file này đã tồn tại. Tạo `.env.local` đã Git ignore để `run.bat` trỏ `DatabasePath`, `RoomImagesPath`, Data Protection keys và thư mục email pickup tới fixture `data/sprint2-demo/20261004-185115-e98174` mà người dùng vừa kiểm thử.
- `run.bat` nay đọc bốn cấu hình local trên (và cấu hình Event Log cần cho môi trường hạn chế); biến môi trường đặt sẵn trong terminal vẫn được ưu tiên. README ghi cách dùng cho máy mới và máy có DB cần giữ.
- Xác minh thực tế: `run.bat --check-only` PASS và báo đúng đường dẫn fixture; `run.bat` mở web tại `http://localhost:5247`; `/TimTin` trả HTTP 200 và một thumbnail từ kho ảnh cấu hình trả `200 image/png`. Đã dừng server sau kiểm tra. Dữ liệu đã thay đổi do lượt test thủ công trước đó được giữ nguyên, không reset fixture.

## Dữ liệu mẫu nghiệm thu S2-01 đến S2-10 (04/10/2026)

- Hoàn thiện `Sprint2DemoSeeder` và bộ script `New/Start-Sprint2Demo`: mỗi lần tạo database/schema v13 và runtime riêng dưới `data/` đã ignore, không đọc/ghi `local-dev.sqlite`, không ghi đè bản cũ. Mật khẩu chung được sinh ngẫu nhiên; email/mật khẩu/role của 7 tài khoản chỉ nằm trong `access.json` và `report.md` local.
- Fixture mới có: ADMIN; CHU_NHA A sở hữu A1/A2; CHU_NHA B và tòa riêng để thử 403; QUAN_LY được gán A1; 3 KHACH_THUE. A1 có 5 dịch vụ/giá, khác biệt gửi xe theo tầng, giá riêng, hóa đơn cũ và cấu hình nước chờ kỳ sau; A2 chưa cấu hình. Có 35 tin công khai ở 4 quận (12/12/11), 6 tin bị loại, đủ trạng thái phòng/tin/yêu cầu, hai lịch cùng phòng cách 15 phút và ca gửi trùng.
- Thêm `upload_sprint2_demo_images.py`: server tạm upload 5 PNG vào A101 qua endpoint thật, sinh thumbnail 400px và xác minh đường dẫn DB/tệp; A104 giữ 0 ảnh. Thư mục mẫu có PNG/JPG hợp lệ, file >5 MiB, JPG nội dung giả và ảnh thứ 9.
- Cách dùng: `powershell -ExecutionPolicy Bypass -File .\verification\New-Sprint2Demo.ps1`, sau đó `.\verification\Start-Sprint2Demo.ps1`. Đọc credential/bản đồ ID trong đường dẫn `data/sprint2-demo/latest.txt` → `access.json`/`report.md`; hướng dẫn từng AC ở `docs/huong-dan-test-sprint2.md`.
- Xác minh thực tế trên bản `data/sprint2-demo/20261004-185115-e98174`: tạo mới + upload HTTP + smoke test PASS; 7/7 đăng nhập, integrity/FK PASS, 35 tin public/4 quận, đủ 8 yêu cầu/5 trạng thái, quyền CHU_NHA B bị chặn, tìm kiếm 5 lần 7,279–29,965 ms. `--check-database` PASS; kho ảnh expected 10, missing 0, orphan 0; chạy lại `Start-Sprint2Demo.ps1` và GET `/TimTin` trả HTTP 200. Full xUnit **259/259 PASS**; đã cập nhật fixture migration test từ v12 lên schema hiện hành v13. Còn cảnh báo NU1900 do nguồn NuGet và cảnh báo license ImageSharp; chưa thao tác thủ công toàn bộ UI từng AC.
- File chính: `Data/Sprint2DemoSeeder.cs`, `Data/RequestDemoSeeder.cs`, `Program.cs`, hai PowerShell demo, hai Python verifier/uploader, test migration, README và hướng dẫn Sprint 2. Không đổi schema/quy trình DB sản xuất, không commit/push.

## Chỉnh form tìm kiếm tin công khai (04/10/2026)

- Đổi form tại `/TinDang` sang card tìm kiếm dùng CSS riêng thay cho các lớp Bootstrap không được nạp trong public layout: ô nhập đồng nhất, bố cục 4/2/1 cột theo desktop/tablet/mobile, nhãn có đơn vị, focus rõ và nút tìm kiếm dễ nhận biết. Giữ nguyên action `/TimTin`, method GET, tên và ràng buộc của đủ tám tham số lọc.
- File thay đổi: `Views/TinDang/Index.cshtml`, `wwwroot/css/public-listing.css`, kiểm thử HTML trong `Sprint2UsabilityTests.cs`. Build và test `PublicListingIndexOffersKeywordAndRequestedFilters` PASS qua output riêng; còn cảnh báo ImageSharp/nullable có sẵn. Chưa kiểm tra trực quan trên trình duyệt vì tiến trình web hiện tại đang chạy bản cũ và khóa output; cần khởi động lại app rồi tải lại `/TinDang`. Không thay đổi database.

## Chỉ Chủ nhà/Quản lý được đăng tin (04/10/2026)

- `TinDangController` cho `CHU_NHA` và `QUAN_LY` mở quản lý/tạo/sửa/gỡ tin; Chủ nhà chỉ thao tác tòa mình sở hữu, Quản lý chỉ thao tác tòa có `quan_ly_id` là tài khoản hiện tại. Khách thuê, ADMIN và quản lý không được phân công bị chặn ở backend. Menu Tin đăng của Chủ nhà/Quản lý dẫn thẳng tới `/TinDang/QuanLy`.
- Quyền mặc định `TIN_DANG` của `QUAN_LY` đổi từ `READ` sang `WRITE`. Updater v13 sao lưu DB rồi chỉ nâng giá trị `READ` mặc định; quyền đã thu hồi thành `NONE` không bị mở lại. Máy có DB v12 cần dừng app, sao lưu DB ra ngoài repository, chạy `--update-database`, `--check-database`, rồi khởi động lại. Không sửa database local trong task này.
- File chính: `Controllers/TinDangController.cs`, `_Sidebar.cshtml`, `permissions.seed.json`, `DatabaseUpdates.cs`, `AccountReuseSchema.cs`, hai bộ test quyền/tin đăng, README và `docs/cap-nhat-csdl.md`. Kiểm thử trên database tạm: 6/6 ca quản lý/chủ nhà/phạm vi vai trò PASS và 4/4 ca ma trận quyền/updater v13 PASS; build qua output riêng vì web đang chạy khóa output mặc định. Còn cảnh báo giấy phép ImageSharp có sẵn; chưa nghiệm thu trực quan bằng trình duyệt. Giả định Quản lý chỉ phụ trách tòa được gán qua `toa_nha.quan_ly_id`.

## Thêm `run.bat` khởi động dự án trên Windows (04/10/2026)

- Thêm `run.bat` ở gốc repository: kiểm tra .NET SDK/project, restore, dùng `DatabasePath` đã cấu hình hoặc database mặc định, tự khởi tạo bằng quy trình của dự án khi file chưa tồn tại, kiểm tra schema rồi chạy profile HTTP tại `http://localhost:5247`.
- Với database đã tồn tại, script không ghi đè và không tự chạy updater; schema chưa sẵn sàng làm script dừng và hướng dẫn sao lưu/chạy `--update-database`. Không tạo ADMIN, không chứa credential. Có `run.bat --check-only` để xác minh mà không giữ web chạy.
- README đã bổ sung cách dùng cho cả máy mới và máy có database cần giữ dữ liệu. Xác minh thực tế `run.bat --check-only` PASS trên database thử mới: restore, khởi tạo schema hiện hành tại thời điểm kiểm thử và check thành công. Chạy lại trên chính file thử PASS, đi qua nhánh giữ database hiện có và SHA-256 trước/sau không đổi. Không dùng database cá nhân; database mặc định vô tình sinh trong lần gọi thử sai đã được dọn cùng toàn bộ backup liên quan sau khi xác nhận trước đó chưa tồn tại.

## Hoàn thiện sau rà soát S2-01 đến S2-10 (04/10/2026)

- S2-03: thêm `TinDangExpirationService` và tác vụ nền chạy mỗi phút; các lối vào danh sách công khai, tìm kiếm và quản lý cũng đồng bộ ngay. Tin `DANG_HIEN_THI` quá hạn chuyển `TAM_AN`; Chủ nhà thấy cảnh báo/nhãn **Đã hết hạn** và có thể đăng lại. Không đổi schema.
- S2-07: thống nhất `MOI` và `DA_HEN_LICH` là chưa xử lý cho cả đánh dấu quá 24 giờ và badge menu; badge 0 được ẩn. Bổ sung test xác nhận hai trạng thái được đếm/đánh dấu, trạng thái đã duyệt không bị tính.
- S2-09: mọi dòng yêu cầu của Khách thuê đều mở đúng tin liên kết. Tin còn public hoạt động như cũ; tin ẩn/hết hạn/phòng không còn trống chỉ chính khách đã gửi yêu cầu được xem lại ở chế độ chỉ đọc, có cảnh báo và không có form gửi mới. API/khách khác vẫn không truy cập được.
- Cập nhật `verification/s206_http.py` theo public layout và hành vi xem lại tin; script PASS toàn bộ trên database mới, gồm validation, quyền/CSRF, chống trùng, mã yêu cầu, gửi đồng thời, rollback, integrity/FK và tin hết hạn chỉ đọc. Không đọc/ghi database cá nhân.
- Xác minh: build test project PASS; `PermissionTests` 50/50 PASS; `RoomServices|RoomImage` 31/31 PASS; `LichHen|YeuCauTenant|DesiredDate` 152/152 PASS; nhóm hồi quy mới 6/6 PASS; `git diff --check` PASS. Đo lại S2-04 với 500 tin PASS dưới 2 giây: request đầu 140,808 ms, mẫu chậm nhất 36,82 ms. Cảnh báo còn lại: NU1900 do không lấy được dữ liệu lỗ hổng NuGet, thiếu license ImageSharp và CS8601 có sẵn trong `AuthController`.
- Database mặc định không tồn tại; mọi xác minh dùng database mới trong `data/` hoặc thư mục tạm. Không thay đổi quy trình database, không commit/push. Chưa nghiệm thu trực quan ở viewport 360 px; form lập hợp đồng vẫn thuộc S3-01.

## Chủ nhà quản lý tin đăng và gửi lại sau khi hủy yêu cầu (04/10/2026)

- Bổ sung `/TinDang/QuanLy` cho vai trò Chủ nhà: liệt kê phòng thuộc các tòa nhà do tài khoản sở hữu, cho đăng/đăng lại tin khi phòng `TRONG` và gỡ tin đang hiển thị. Gỡ tin chuyển trạng thái sang `TAM_AN`, không xóa bản ghi; mọi thao tác kiểm tra lại quyền sở hữu và quyền ghi `TIN_DANG` ở backend. Sidebar Chủ nhà dẫn vào màn quản lý; danh sách phòng có nút **Đăng tin** cho phòng trống.
- Form đăng tin lấy sẵn phòng, tòa nhà, diện tích, giá thuê và mô tả phòng; Chủ nhà nhập/chỉnh tiêu đề, mô tả. Tin được hiển thị 30 ngày và unique index hiện có tiếp tục bảo đảm mỗi phòng chỉ có tối đa một tin `DANG_HIEN_THI`.
- Xác nhận truy vấn nghiệp vụ đã loại `DA_HUY`; lỗi demo trước đây do cùng Khách Demo còn một yêu cầu `MOI` khác trên chính tin đó. Sửa `RequestDemoSeeder` để mỗi khách/tin chỉ có một yêu cầu mở, vẫn giữ khách phụ riêng cho ca cảnh báo trùng lịch. Tạo mới `data/s2-08-demo/demo-v2.sqlite`; file demo cũ được giữ nguyên, không ghi đè. Demo v2 có 3 tài khoản giả, 3 phòng, 3 tin, 4 yêu cầu và không có cặp khách/tin mở trùng.
- File chính: `TinDangController`, `TinDangViewModels`, Razor `TinDang/QuanLy`, `TinDang/Tao`, sidebar, danh sách phòng, `RequestDemoSeeder`, `OwnerListingManagementTests`. Không đổi schema hoặc quy trình database.
- Xác minh thực tế: build PASS; nhóm tin công khai + quản lý tin + lịch hẹn **150/150 PASS**; kiểm thử riêng xác nhận yêu cầu `DA_HUY` không chặn `Send`; HTTP đăng nhập thật xác nhận màn quản lý tin và toàn bộ tiêu chí lịch hẹn hiện có PASS; `git diff --check` PASS. Web demo v2 đang chạy tại `http://127.0.0.1:5268`.

## Hủy yêu cầu bởi Chủ nhà và Khách thuê (04/10/2026)

- Bổ sung nút **Hủy yêu cầu** trên trang chi tiết cho cả đúng Chủ nhà của phòng và đúng Khách thuê đã gửi yêu cầu. Chỉ hiện và chỉ cho phép xử lý ở trạng thái `MOI` hoặc `DA_HEN_LICH`; endpoint kiểm tra lại quyền/trạng thái ở backend và có hộp thoại xác nhận trước khi gửi.
- Khi hủy, yêu cầu chuyển `DA_HUY` bằng cập nhật có kiểm tra phiên bản, ghi lịch sử gồm người thực hiện/vai trò/thời điểm và gửi thông báo cho bên còn lại trong cùng transaction. Chủ nhà vẫn có luồng **Từ chối** riêng với danh sách lý do bắt buộc. Không thay đổi schema hoặc quy trình database.
- File chính: `Services/LichHenService.cs`, `Controllers/LichHenController.cs`, `Views/LichHen/ChiTiet.cshtml`, các model/view model lịch hẹn và test `LichHenTuChoiTests.cs`, `LichHenLichSuTests.cs`.
- Xác minh thực tế: build PASS; nhóm `LichHen` **148/148 PASS**; `git diff --check` PASS. HTTP đăng nhập thật xác nhận cả Chủ nhà và Khách thuê đều thấy form **Hủy yêu cầu** trỏ đúng endpoint trên yêu cầu còn mở; không bấm hủy để giữ nguyên fixture demo. Cảnh báo NuGet/ImageSharp có sẵn, không có test lỗi. Web demo đã chạy lại tại `http://127.0.0.1:5268`.

## Giờ hẹn 24 giờ và danh sách yêu cầu của Khách thuê (04/10/2026)

- Thay control `datetime-local` bằng ô ngày và ô giờ 24 giờ cho màn hình xác nhận/đổi lịch; chấp nhận `0:00` hoặc `00:00`, chuẩn hóa thành `00:00` trước khi gửi. Đồng thời sửa tên trường POST của luồng đổi lịch để bind đúng `LichHenNhap`.
- `/YeuCau` cho phép vai trò Khách thuê và chỉ truy vấn yêu cầu gắn với tài khoản khách hiện tại. Giao diện đổi thành **Yêu cầu của tôi**, ẩn bộ lọc tòa nhà và giữ nút **Chi tiết**; trang chi tiết tiếp tục chặn chỉnh sửa đối với khách.
- Không thay đổi schema/database. Xác minh: build PASS; nhóm `LichHen` **144/144 PASS** gồm ca `00:00`; HTTP đăng nhập thật xác nhận Khách thuê mở `/YeuCau`, thấy đúng 4 liên kết chi tiết và không có control xử lý; Chủ nhà thấy ô giờ 24 giờ `00:00`, cảnh báo trùng lịch và các thao tác cũ vẫn còn. Web demo đã chạy lại tại `http://127.0.0.1:5268`.

## Bổ sung lối vào chi tiết lịch hẹn từ danh sách (04/10/2026)

- Danh sách **Yêu cầu của khách** đã có cột **Thao tác** và nút **Chi tiết** trên từng dòng, dẫn tới `/LichHen/ChiTiet/{id}`. View model/projection được bổ sung ID thật của `yeu_cau_thue`; trang đích tiếp tục kiểm tra quyền chủ nhà/khách ở backend.
- Không đổi schema hoặc dữ liệu demo. Xác minh: build PASS; nhóm `LichHen` **143/143 PASS**; đăng nhập Chủ nhà qua HTTP, `/YeuCau` trả đủ bốn nút với đích `/LichHen/ChiTiet/1..4`. Web demo đã chạy lại tại `http://127.0.0.1:5268`.

## Dữ liệu demo và hướng dẫn nghiệm thu S2-08 (04/10/2026)

- Đã tạo database demo mới, schema v12 tại `data/s2-08-demo/demo.sqlite` (đang được Git ignore), gồm hai tài khoản giả Chủ nhà/Khách thuê, hai phòng và bốn yêu cầu tách riêng cho xác nhận/cảnh báo trùng lịch, từ chối, xem lịch sử và duyệt Thuê ngay. Không đọc, sao chép hoặc ghi database cá nhân.
- Sửa `RequestDemoSeeder` để lấy mật khẩu từ `RequestDemo:Password`, không nhúng mật khẩu và không tạo ADMIN. Seeder chỉ chấp nhận database mới không có tài khoản/dữ liệu nghiệp vụ, kiểm tra schema trước ghi và tạo backup; console in các URL demo cùng giờ lịch xung đột. `Program.cs` truyền cấu hình này; README có quy trình máy mới và nhắc máy có DB cần giữ dữ liệu không được chạy seeder.
- Web demo đang chạy tại `http://127.0.0.1:5268` với Data Protection keys riêng trong thư mục demo. Credential và hướng dẫn chi tiết nằm trong `data/s2-08-demo/access.txt`, không đưa vào Git.
- Xác minh thực tế: build PASS; `--check-database` PASS; truy vấn SQLite `mode=ro` xác nhận schema v12, không lỗi FK, 2 tài khoản có BCrypt hash dài 60, 4 yêu cầu và 2 phòng đều đúng trạng thái đầu; nhóm `LichHen` **143/143 PASS**; HTTP đăng nhập thật cho cả hai vai trò và kiểm tra form/cảnh báo/4 lý do/nút duyệt/quyền xem lịch sử đều PASS. Chưa bấm các thao tác ghi trong browser để giữ fixture nguyên trạng cho người nghiệm thu.

## Kiểm tra tiêu chí xử lý lịch hẹn S2-08 (03/10/2026)

- Đã đối chiếu controller, service, Razor/JavaScript, migration v10 và kiểm thử hiện có. Ba tiêu chí đầu đạt: xác nhận bắt buộc ngày giờ tương lai và cảnh báo lịch cùng phòng trong khoảng ±30 phút; từ chối chỉ nhận đúng 4 lý do và `LY_DO_KHAC` bắt buộc ghi chú 5–500 ký tự; các thao tác xác nhận, đổi lịch, từ chối, duyệt thuê ngay ghi lịch sử cùng người thực hiện/thời điểm, khách của yêu cầu xem được lịch sử.
- Tiêu chí duyệt **Thuê ngay** đạt phần cập nhật nghiệp vụ: yêu cầu chuyển `DA_DUYET`, phòng chuyển `DA_DAT_COC` trong cùng transaction và nút **Lập hợp đồng** chỉ hiện cho chủ nhà khi hai trạng thái khớp. Chưa đạt nếu hiểu nút phải mở form lập hợp đồng hoàn chỉnh: route hiện chỉ dẫn đến trang chờ S3-01, chưa tạo hợp đồng.
- Xác minh thực tế: `dotnet test Tests/QL_PhongTro.Tests/QL_PhongTro.Tests.csproj --filter "FullyQualifiedName~LichHen"` **143/143 PASS**. Có cảnh báo không tải được dữ liệu lỗ hổng NuGet, ImageSharp license và CS8601 sẵn có; không có test lỗi. Database mặc định `QL_PhongTro/Data/local-dev.sqlite` không tồn tại và cấu hình không chỉ định đường dẫn khác, nên không tạo/ghi DB local và chưa kiểm thử trình duyệt thật.
- Không thay đổi mã nguồn, schema hay quy trình database. Bước tiếp theo phù hợp là S3-01: thay trang chờ `Views/LichHen/LapHopDong.cshtml` bằng luồng lập hợp đồng thật và giữ điều kiện quyền/trạng thái hiện có.

## Merge S2-02 xoá ảnh phòng vào dev (03/10/2026)

- Đã merge `feature/S2-2/owner-delete-room-photo` vào nhánh local `dev` tracking `origin/dev`. Tự xử lý conflict ở `AccountReuseSchema.cs`, `DatabaseUpdates.cs`, `README.md` và `docs/tien-do.md`.
- Conflict schema được hợp nhất bằng cách giữ v11 của `dev` cho bảng `yeu_cau` và nâng thay đổi xoá ảnh sang v12 (`anh_phong.dang_cho_xoa`, `loi_xoa_gan_nhat`, `lan_thu_xoa_gan_nhat`). `AccountReuseSchema` chấp nhận dải v4-v12; tài liệu DB/README cập nhật schema hiện hành v12.
- Xác minh sau merge: build Debug project chính PASS; nhóm ảnh + migration/schema **16/16 PASS**; full xUnit **101/101 PASS**. `git diff --check` PASS; không có DB/SQLite/sidecar được Git theo dõi. Còn cảnh báo ImageSharp license và CS8601 trong `AuthController` là cảnh báo có sẵn.
- Chưa push `dev` lên remote trong lượt này. DB local thật không được ghi/nâng cấp; chỉ có artifact demo/build trong `data/` đang ignore.

## Kiểm tra tiêu chí ảnh phòng và demo nghiệm thu (03/10/2026)

- Đã đối chiếu mã hiện tại và chạy lại kiểm thử: hệ thống thỏa 4 tiêu chí ảnh phòng. `RoomImageStore` giới hạn tối đa 8 ảnh/phòng, 5 MiB/tệp, chỉ JPG/PNG hợp lệ và sinh thumbnail rộng 400px; form chủ nhà có kéo-thả/sắp xếp, ảnh `thu_tu=1` là ảnh đại diện trên `/TinDang`; xoá ảnh có xác nhận, xoá bản ghi + tệp gốc + thumbnail, và lệnh đối chiếu kho báo mồ côi/missing. Sau khi demo phát hiện ảnh vỡ khi `RoomImagesPath` nằm ngoài `wwwroot`, đã sửa `Program.cs` để phục vụ storage cấu hình riêng tại `/uploads/rooms`.
- Thêm script demo `verification/prepare_room_images_demo.py`. Script tạo DB và kho ảnh mới trong `data/room-images-demo`, chạy server riêng, upload 5 ảnh PNG qua HTTP thật, tạo tin public, xác minh danh sách tin dùng thumbnail ảnh đầu, chi tiết dùng ảnh gốc ảnh đầu và `--check-room-image-storage` báo `missing: 0; orphan: 0`. Không đọc/ghi DB local thật.
- Demo mới đang chạy: `http://localhost:5259`, PID `27972`, DB `data/room-images-demo/20261003-174320-e7c11c/demo.sqlite`, kho ảnh `data/room-images-demo/20261003-174320-e7c11c/uploads/rooms`. Chủ nhà: `demo-chu_nha-f4f04988@example.test`; mật khẩu chung: `42662CAC1411087053DEa1!`; phòng demo: `/PhongTro/Edit/1`; danh sách tin: `/TinDang`; chi tiết tin: `/TinDang/ChiTiet/1`. Đã kiểm tra trực tiếp thumbnail `/uploads/rooms/...-thumb.png` trả `200 image/png`.
- Hướng dẫn test nhanh: đăng nhập Chủ nhà, mở `/PhongTro/Edit/1`, kiểm tra bộ đếm `5/8`; kéo-thả ảnh khác lên đầu rồi mở `/TinDang` để thấy ảnh đại diện đổi; tải thêm 3 ảnh JPG/PNG hợp lệ để đạt `8/8`, ảnh thứ 9 bị từ chối; thử ảnh >5 MB hoặc định dạng khác để thấy lỗi; bấm **Xóa**, chọn **Huỷ** để ảnh còn nguyên, bấm lại và **Xoá** để ảnh biến mất, bộ đếm giảm và tệp gốc/thumbnail mất khỏi thư mục kho.
- Xác minh đã chạy trong lượt này: build runtime demo PASS; script demo PASS; `dotnet test Tests/QL_PhongTro.Tests/QL_PhongTro.Tests.csproj --no-restore --filter "FullyQualifiedName~RoomImage"` **10/10 PASS**. Cảnh báo ImageSharp license và CS8601 trong `AuthController` là cảnh báo có sẵn. Chưa thao tác browser thủ công thay người dùng; demo đã sẵn để nghiệm thu trực quan.

## S2-02 — ảnh phòng, thumbnail và xoá ảnh (03/10/2026)

- PO chốt: trang chi tiết dùng ảnh lớn + dải thumbnail; phòng chưa ảnh dùng placeholder chung; chủ nhà tải trong form sửa phòng; thumbnail giữ tỉ lệ, không phóng ảnh nhỏ; JPEG quality 82, PNG nén lossless. AC2: không thêm nút “Đặt làm ảnh đại diện”; desktop kéo-thả, màn hình cảm ứng dùng mũi tên lên/xuống; nhãn **Ảnh đại diện** nổi bật trên ảnh đầu.
- Hoàn tất upload tối đa 8 ảnh/phòng, 5 MiB/tệp, JPG/PNG kiểm tra cả nội dung; xác minh ảnh hỏng, ảnh vượt cỡ, MIME/đuôi giả và quyền sở hữu phía server. Ghi ảnh gốc + thumbnail đã auto-orient trong `wwwroot/uploads/rooms/{roomId}`, lưu đường dẫn/thứ tự/thời điểm vào `anh_phong`; transaction SQLite immediate cấp thứ tự, dọn cặp tệp nếu lỗi/hủy. Upload nhiều tệp có tiến trình/lỗi riêng và lưới xem trước `x/8`.
- Endpoint reorder kiểm tra chủ sở hữu và tập ID gửi lên phải khớp chính xác (không thiếu/thừa/trùng). Cập nhật thứ tự trong một transaction, giữ tập ảnh hiện có; tab lưu sau được phép thay thứ tự mới nhất. Ảnh tải mới nhận thứ tự cuối. List tin lấy thumbnail của `thu_tu=1`; API/chi tiết trả ảnh theo cùng thứ tự.
- UI dùng HTML drag/drop trên desktop, mũi tên lên/xuống cho cảm ứng, cập nhật badge/bộ đếm ngay; lỗi/mất mạng khôi phục thứ tự cũ và báo lỗi. AC4 đã bổ sung nút **Xóa** trên từng ảnh, hộp thoại xác nhận “Xoá ảnh này? Ảnh sẽ bị xoá vĩnh viễn và không thể khôi phục.” với **Huỷ/Xoá**, không có hoàn tác.
- Xoá ảnh kiểm tra chủ sở hữu phía server, xoá bản ghi + tệp gốc + thumbnail, đánh lại `thu_tu` liên tục. Xoá ảnh đại diện làm ảnh kế tiếp lên đại diện; xoá hết ảnh thì list/detail dùng placeholder. Xác nhận hai lần liên tiếp không gây lỗi; chủ nhà khác không xoá được ảnh không thuộc phòng của mình.
- Schema nền sau merge lên v12: v11 giữ bảng `yeu_cau` của dev, v12 thêm `dang_cho_xoa`, `loi_xoa_gan_nhat`, `lan_thu_xoa_gan_nhat` vào `anh_phong`. Nếu xoá tệp kho lưu trữ lỗi, giữ bản ghi ở trạng thái chờ xoá, lưu lỗi gần nhất, UI giữ nguyên ảnh và background service retry định kỳ. Có lệnh thủ công `--retry-pending-room-image-deletes` và `--check-room-image-storage`; README và `docs/cap-nhat-csdl.md` đã cập nhật quy trình máy mới/máy có DB.
- File/thành phần chính: `Services/RoomImageStore.cs`, `Services/RoomImageDeletionService.cs`, `Services/RoomImageCleanupHostedService.cs`, `Controllers/PhongTroController.cs`, `Data/DatabaseUpdates.cs`, `Models/AnhPhong.cs`, Razor `PhongTro/Create`, `wwwroot/js/room-images.js`, `wwwroot/css/room-images.css`, README và tài liệu DB. Test ở `RoomImageStoreTests.cs`, `RoomImageUploadTests.cs`, migration tests.
- Xác minh: build Debug PASS; nhóm ảnh focused **9/9 PASS**; nhóm migration/schema focused **6/6 PASS**; full xUnit **100/100 PASS**. Test xóa ảnh đại diện cập nhật thumbnail public/detail, xóa ảnh giữa giữ thứ tự liên tục, xóa hết về placeholder, upload lại đủ 8, idempotent, quyền sở hữu và đối chiếu kho 0 orphan/0 missing. Chưa chạy browser thật cho dialog; xác minh UI hiện tại qua markup/JS và HTTP.
- Browser demo: kéo ảnh thứ ba lên đầu, reload giữ thứ tự; mũi tên lên/xuống đổi vị trí; chặn POST reorder xác nhận rollback và thông báo lỗi; API/detail/list xác nhận ảnh đầu và thứ tự thumbnail/gốc khớp. Chọn 10 ảnh khi đã có 5 chỉ thêm 3 để đạt `8/8`, 7 ảnh còn lại báo đủ ảnh. Đã phục hồi 8 cặp ảnh giả cho demo, tất cả ảnh gốc/thumbnail tải được; list dùng thumbnail 400px. Server demo chạy tại `http://localhost:5259`, fixture hiện có 8 ảnh; manifest `data/room-images-demo-d51197c4/access.json` chỉ dành cho máy local, không chia sẻ/commit.
- Giới hạn xác minh: chưa throttling mạng 3G thật hoặc thử thiết bị cảm ứng vật lý; đã thử mũi tên ở viewport browser nhỏ và chặn request để xác nhận rollback. ImageSharp license warning và CS8601 trong `AuthController` là cảnh báo có sẵn.
- Bước tiếp theo: chạy nghiệm thu trình duyệt thật nếu cần quan sát dialog; DB thành viên cần dừng app, sao lưu ngoài repo, chạy `--update-database` rồi `--check-database` để lên v12 trước khi chạy web.

## S2-07 AC4 — số lượng yêu cầu chưa xử lý trên menu (03/10/2026)

- PO chốt trạng thái chưa xử lý là `MOI` (Mới) và `DA_HEN_LICH` (Đã hẹn lịch); các trạng thái còn lại đã xử lý. Số 0 được ẩn trên menu.
- Thêm `PermissionService.UnprocessedRequestCountAsync`: đếm độc lập với bộ lọc `/YeuCau`, chỉ lấy yêu cầu thuộc các tòa đang hoạt động do chủ nhà hiện tại quản lý. Menu yêu cầu trỏ trực tiếp `/YeuCau` và hiển thị badge khi số lượng lớn hơn 0; mỗi lần render menu đọc dữ liệu hiện tại nên phản ánh yêu cầu mới hoặc thay đổi trạng thái sau khi tải lại/chuyển trang.
- Không thay đổi schema, database local hoặc quy trình khởi tạo/nâng cấp. File thay đổi: `Authorization/PermissionService.cs`, `Views/Shared/_Sidebar.cshtml`.
- Xác minh: build project chính với output riêng `obj/S207AC4Check`/`bin/S207AC4Check` PASS, 0 lỗi; có cảnh báo license ImageSharp và CS8601 đã tồn tại. Chưa chạy test HTTP/browser hoặc ghi database local.

## Demo nghiệm thu S2-10 (02/10/2026)

- Thêm `verification/prepare_s210_demo.py` và hướng dẫn README. Script chỉ tạo database mới dưới `data/s210-demo/`, dùng initializer + permission demo của dự án, dữ liệu tòa/cấu hình giả; không đọc/ghi database local. Artifact và credential thuộc `data/` đã ignore. Bản thành công mới nhất chạy ở cổng 5251; `latest.txt` trỏ tới `access.json` chứa URL, PID, database và bốn tài khoản chung mật khẩu ngẫu nhiên.
- Dữ liệu nghiệm thu 10/2026: điện theo chỉ số 4.000đ/kWh, nước theo người 80.000đ/người/tháng. Cấu hình chờ 11/2026 được lưu qua HTTP thật: điện theo người 95.000đ/người/tháng, nước theo chỉ số 18.000đ/m³. HTTP form có đủ hai lựa chọn và hiển thị kỳ; POST giá điện 0 trả lại form có lỗi, bốn version trong DB không đổi. `--check-database`, integrity và FK PASS.
- `Program.cs` nhận cấu hình tùy chọn `DataProtectionKeysPath` để demo dùng kho khóa cookie riêng trong workspace. Mặc định không đặt biến này nên cách chạy và kho khóa hiện tại không đổi; không đổi schema/updater. Lần đầu trong sandbox thất bại do kho DPAPI người dùng không ghi được, các server lỗi đã dừng; bản sau dùng kho riêng thành công.
- Build runtime demo PASS, không lỗi; toàn suite sau thay đổi PASS 46/46 trong 1 phút 15 giây; còn cảnh báo ImageSharp và CS8601 có sẵn. Computer Use không có browser khả dụng nên chưa kiểm tra trực quan; xác minh UI dùng HTML MVC thật qua HTTP. Server demo thành công được giữ chạy để người dùng mở bằng trình duyệt trên máy.

## S2-10 — hoàn thiện cấu hình điện/nước lần đầu (02/10/2026)

- Đã bỏ điểm chặn còn lại của S2-10: chủ nhà có thể cấu hình điện/nước ngay cả khi tòa chưa khởi tạo danh mục gợi ý hoặc các dòng mặc định chưa chốt giá. Cấu hình hợp lệ đầu tiên cũng bắt đầu từ ngày 01 tháng kế tiếp; dòng chưa chốt của kỳ hiện tại, nếu có, chỉ được đóng `den_ngay` và không bị ghi đè. Nếu thiếu danh mục DIEN/NUOC hoặc liên kết tòa, service tạo đúng phần tối thiểu và không tự bật áp dụng mặc định cho phòng.
- Giữ nguyên giao diện hai lựa chọn độc lập, trường giá theo phương thức, thông báo “Áp dụng từ kỳ MM/yyyy”, validation giá bắt buộc lớn hơn 0, kiểm tra sở hữu/CSRF/stale form và transaction hiện có. Không đổi schema, updater, README hay database local.
- File thay đổi: `Services/DichVuService.DienNuoc.cs`; thêm `Tests/QL_PhongTro.Tests/RoomServicesTests.ElectricWater.cs`; cập nhật tài liệu này. Giữ nguyên thay đổi có sẵn trong `Tests/QL_PhongTro.Tests/QL_PhongTro.Tests.csproj`.
- Kiểm thử mới dùng SQLite tạm do fixture tạo: tạo cấu hình trước bước khởi tạo danh mục; chốt giá lần đầu từ dòng chưa có giá; điện theo người/nước theo chỉ số và đúng đơn vị; đổi cách tính giữ nguyên kỳ hiện tại; tra giá trước/sau ranh giới kỳ; từ chối giá trống/0/âm và không ghi dở. Các test mới PASS 6/6; toàn suite PASS 46/46 trong 1 phút 14 giây. Build đi kèm test không có lỗi; còn cảnh báo license ImageSharp có sẵn. Chưa chạy lại kiểm tra trực quan trên trình duyệt trong lần sửa này.
- Cách dùng không đổi: Chủ nhà mở **Cấu hình điện nước** từ danh sách tòa hoặc trang dịch vụ, chọn cách tính và nhập giá dương. Màn hình hiển thị kỳ áp dụng trước khi lưu; tải lại sẽ thấy cấu hình đang chờ. Không cần cập nhật database cho thay đổi này.

## S2-10:4 — lưu cấu hình điện/nước cho kỳ kế tiếp (02/10/2026)

- Hoàn thành luồng chính trên branch feature/S2-10/save-next-period-config. Giữ schema/version SQLite hiện có, không migration/bảng mới. Dùng cau_hinh_dich_vu.tu_ngay/den_ngay: cấu hình mới bắt đầu ngày 1 tháng kế tiếp UTC+7, chỉ đóng khoảng cũ, không ghi đè cách tính/giá hiện tại. Thời gian trong từng GET/lần lưu lấy một lần từ ITimeProvider (fallback UTC hệ thống).
- Điện/nước so sánh độc lập với baseline chờ nếu có, hiện tại nếu chưa có. Không thay đổi thì không tạo/sửa version. Có chờ đúng tòa + dịch vụ + phong_id NULL + ngày đầu kỳ sau thì UPDATE giữ ID; chưa có thì đóng khoảng trước INSERT trong cùng transaction. Unique index và trigger chống chồng lấn vẫn bảo vệ DB. Hai dịch vụ cùng transaction; lỗi bên sau rollback cả version và ngày kết thúc bên trước, gồm audit.
- Giao diện phân biệt cấu hình đang áp dụng/chờ, kỳ bắt đầu chờ, form sửa chờ; mở lại không báo thay đổi. Giữ JavaScript S2-10:3 hiện có vì baseline data-saved-* đã được cấp đúng. Giữ validation S2-10:2: giá trống/0/âm bị chặn, lỗi giữ dữ liệu nhập, ô không chọn bỏ qua kể cả lỗi binding. Quyền CHU_NHA/PHONG_TRO, sở hữu tòa và antiforgery giữ nguyên.
- POST và service tính lại kỳ; form qua tháng hoặc snapshot trạng thái không còn khớp bị từ chối trước khi ghi. Snapshot SHA256 so sánh toàn bộ phiên bản điện/nước, nhận biết UPDATE cùng ID. Không coi kỳ browser là nguồn quyết định hiệu lực. Các đường SuaGiaBanDau/DoiGia chung với DIEN/NUOC được chặn và hướng về màn hình cấu hình để không vượt quy tắc kỳ kế tiếp; dịch vụ khác giữ nguyên.
- Chưa triển khai nghiệp vụ chưa chốt: thiết lập lần đầu/giá chưa chốt; đổi chờ về đúng hiện tại (hủy); đổi cách tính khi có giá riêng phòng; có phiên bản sau kỳ kế tiếp; sửa chờ đã tham chiếu hóa đơn/hợp đồng. Service báo rõ và không ghi; không xóa chờ/không tự suy diễn quy tắc. Nguồn số người vẫn để lát hóa đơn sau. Dịch vụ ngừng áp dụng cũng dừng thay đổi để đối chiếu lịch.
- File thay đổi: Controllers/DichVuController.cs; Services/DichVuService.DienNuoc.cs; Services/DichVuService.Management.cs; ViewModels/CauHinhDienNuocViewModel.cs; Views/DichVu/DienNuoc.cshtml; docs/tien-do.md. Không sửa model/AppDbContext/schemaSQL/updater/HoaDonDichVuService/electric-water-config.js, không sửa hoặc build/run Tests/tests.
- Xác minh console tạm (obj/S2104Check, ignore; không dùng project Tests/tests) trên bản sao DB giả S2103: PASS đổi điện chỉ số→người, người→chỉ số; giá điện/nước riêng và cả hai; chỉ một dịch vụ thay đổi tăng 1 dòng, cả hai tăng 2; kỳ hiện tại giữ ID/cách tính/giá cũ, kỳ kế tiếp đúng mới; sửa chờ giữ ID/count; mở lại baseline sạch; POST không đổi không thêm dòng; UTC+7 31/12 16:59:59→17:00 chuyển năm, form cũ bị từ chối; trigger lỗi INSERT nước chứng minh rollback cả hai và các khoảng cũ; stale form bị từ chối; 12 tổ hợp giá sai ở service, ô không chọn bỏ qua; quyền chủ khác bị chặn. PASS guard route giá cũ, đổi chờ về hiện tại, lịch xa hơn và giá ban đầu chưa chốt đều không ghi dữ liệu. Chưa stress test nhiều writer đồng thời.
- HTTP thực tế assembly cuối trên DB giả riêng: PASS lưu/mở lại có hiện tại + chờ + kỳ 11/2026; số dòng 4→5 khi đổi điện, UPDATE điện 5→5 giữ ID; 24 tổ hợp validation trống/0/âm (hai dịch vụ × hai phương thức × tổ hợp của dịch vụ còn lại), giữ giá/phương thức, không ghi dở; ô không chọn trống/0/âm/text/overflow không chặn; thông báo trước lưu vẫn hiện khi có thay đổi hợp lệ dù bên còn lại sai; stale period/tab không ghi; tòa chủ khác GET/POST 403, thiếu CSRF 400; integrity/FK PASS. Bản chốt hóa đơn đã phát hành/chi tiết trên DB giả không đổi.
- Browser thật bằng skill computer-use: PASS baseline chờ sạch, sửa giá điện hiện kỳ, hoàn tác về baseline chờ ẩn kỳ, đổi phương thức bật kỳ và disabled ô cũ, giá trống/0/âm không lưu, sửa chờ qua nút lưu và reload đúng, thay đổi nước độc lập. Sau đổi riêng nước DB có đúng 6 dòng (4 gốc + 2 chờ), giá gốc điện 120000/nước 15000 giữ nguyên; chờ điện 140000/nước 16000 ngày 2026-11-01. Ảnh obj/S2104Check/pending-proof.png. Chưa nghiệm thu Safari/Firefox hoặc các viewport khác.
- Build project chính PASS: dotnet build QL_PhongTro.csproj --no-restore -c Debug -p:IntermediateOutputPath=obj/S2104Final/ -p:OutputPath=bin/S2104Final/ (từ thư mục app). Còn warning ImageSharp license/CS8601 AuthController có sẵn. Console xác minh tạm có NU1900 do feed vulnerability không truy cập được; đã chạy thành công bằng DLL, không dùng apphost bị khóa. git diff --check PASS.
- Không ghi local-dev.sqlite hoặc backup có sẵn; SHA256 local DB trước/sau bằng nhau: cea2190c068f4bb35eda4f28130258ce263b2347b6894d409b67de37b08ac2ac. Chỉ ghi các DB giả trong obj/S2104Check; không kill process người dùng. Không stage/commit/push, không bắt đầu task khác; giữ nguyên tests/QL_PhongTro.Tests/QL_PhongTro.Tests.csproj modified có sẵn.


## S2-10:3 — xác định và hiển thị kỳ bắt đầu áp dụng (02/10/2026)

- Quy chiếu nghiệp vụ: 01-yeu-cau-he-thong.md:690 và 02-csdl.dbml:105 xác định kỳ tháng dương lịch, ngày chốt chung của tòa không đổi ranh giới kỳ. HoaDonDichVuService.cs:39–40 dùng đầu/cuối tháng; DichVuService.Management.cs:8 (KySau) và DichVuPhongService.cs:13 (KyNgung) lấy tháng kế tiếp theo UTC+7, độc lập hóa đơn đã phát hành. Kỳ hiện tại là tháng lịch tại thời điểm mở/trả form; kỳ áp dụng là ngày 01 tháng kế tiếp. Tòa chưa có hóa đơn dùng cùng quy tắc, không cần hóa đơn đầu tiên hoặc lấy hóa đơn gần nhất +1. Không có điểm PO còn thiếu cho cách xác định kỳ của lát này.
- Tái sử dụng KyNgung và ITimeProvider hiện có trong DichVuService để lấy kỳ kế tiếp với một lần đọc thời gian; không tạo service/abstraction mới. GET lấy cấu hình và bản gốc riêng; POST lỗi đọc lại baseline/kỳ từ server, giữ ModelState/phương thức/giá đã nhập. Các trường baseline/kỳ có BindNever/ValidateNever, không tin giá trị client gửi.
- Phát hiện khác biệt chỉ xét phương thức và giá đang chọn của từng dịch vụ. Có ít nhất một thay đổi hợp lệ thì hiện “Áp dụng từ kỳ MM/yyyy” trước nút lưu; mở lần đầu/hoàn tác về giá cũ thì ẩn. Không xét ô giá ẩn; JS dùng BigInt so sánh nguyên VND chính xác. Giá trống/0/âm/ngoài long không được coi là thay đổi hợp lệ. Validation S2-10:2, quyền CHU_NHA/PHONG_TRO/sở hữu và CSRF được giữ.
- File sửa: Controllers/DichVuController.cs; Services/DichVuService.cs; Services/DichVuService.DienNuoc.cs; ViewModels/CauHinhDienNuocViewModel.cs; Views/DichVu/DienNuoc.cshtml; wwwroot/js/electric-water-config.js; docs/tien-do.md. Truy cập /DichVu/DienNuoc?toaNhaId=<id> hoặc nút Cấu hình điện nước hiện có.
- Không đổi schema/migration/quy trình DB hoặc cơ chế lưu S2-10:1/2. Thông tin kỳ trong lát này chỉ phục vụ xem trước; chưa lưu lịch/version hoặc trì hoãn hiệu lực thực tế. Chưa áp dụng cấu hình mới vào hóa đơn, không sửa hóa đơn cũ; S2-10:4 chưa thực hiện. Nguồn số người tính khoán vẫn để PO chốt ở lát hóa đơn sau.
- Xác minh thực tế trên DB mới chứa dữ liệu giả (obj/S2103Check, ignore), khởi tạo qua initializer/updater và script/installer hóa đơn hiện có: console với assembly cuối PASS 5 thời điểm UTC × 2 tòa (có hóa đơn/chưa có hóa đơn), gồm 16:59:59→17:00 cuối tháng theo UTC+7, chuyển năm, tháng 2 nhuận; baseline độc lập với dữ liệu đang chỉnh, đổi/hoàn tác PASS. Không dùng/sửa/build/run project Tests/tests.
- Browser thật PASS 12 trạng thái: ban đầu, đổi phương thức điện/nước, đổi giá từng dịch vụ/cả hai, hoàn tác, ô ẩn có giá, thay đổi sai duy nhất, một thay đổi hợp lệ khi dịch vụ còn lại sai; tòa chưa có hóa đơn ban đầu ẩn rồi đổi giá hiện kỳ 11/2026 trước nút lưu. Ảnh obj/S2103Check/period-proof.png (ignore). HTTP assembly cuối PASS 24 trường hợp validation giá và bốn tổ hợp hợp lệ/lưu/mở lại, giữ form lỗi/không ghi một phần, bỏ qua ô ẩn lỗi binding; baseline/kỳ giả gửi trong POST bị bỏ qua; GET/POST tòa chủ khác 403, thiếu CSRF 400, integrity/FK PASS. Hóa đơn giả đã phát hành và chi tiết không đổi qua các lần lưu cấu hình.
- Build project chính PASS: `dotnet build QL_PhongTro/QL_PhongTro.csproj --no-restore -c Debug -p:IntermediateOutputPath=obj/S2103Final/ -p:OutputPath=bin/S2103Final/` từ gốc repo. Còn cảnh báo ImageSharp license/CS8601 cũ. Chỉ dùng thư mục output/cache riêng; không kill process của người dùng. Server xác minh dùng biến môi trường tắt EventLog provider như phiên trước, không sửa cấu hình app.
- Chưa nghiệm thu mobile/Safari/Firefox, trang mở xuyên ranh giới tháng hoặc người dùng ghi đồng thời. Kỳ được server tính lúc GET/POST; chưa tự cập nhật thông báo trong trang đang mở khi tháng đổi. Database local chỉ đọc schema/đối chiếu hash; không sửa tài khoản/dữ liệu cá nhân. Không stage/commit/push; giữ nguyên project test modified có sẵn. Dừng sau S2-10:3.

## S2-10:2 — chặn cấu hình thiếu/sai đơn giá (02/10/2026)

- Hoàn thành AC không lưu giá thiếu/0 và bổ sung chặn giá âm, độc lập điện/nước theo cách tính đang chọn. Giữ validation IValidatableObject và kiểm tra service hiện có; controller bỏ ModelState của giá không được chọn rồi kiểm tra lại giá đang chọn (MVC có thể bỏ qua object validation khi trường số không dùng lỗi binding). Giữ lỗi binding/giá thử nhập của trường đang chọn; không tải lại cấu hình DB khi trả form lỗi. Một giá sai không gọi service, không ghi cấu hình/audit hoặc lưu một phần.
- Frontend dùng _ValidationScriptsPartial và jQuery unobtrusive hiện có, metadata required/number/range, HTML min=1/step=1. JavaScript bật required cho ô đang chọn, disabled/ẩn ô không dùng và dọn lỗi tại ô vừa ẩn; thông báo giá >0 bằng tiếng Việt. Ô ẩn không chặn submit, kể cả request thủ công gửi chuỗi không phải số/giá tràn long ở ô không dùng.
- File sửa: Controllers/DichVuController.cs; ViewModels/CauHinhDienNuocViewModel.cs; Views/DichVu/DienNuoc.cshtml; wwwroot/js/electric-water-config.js; docs/tien-do.md. Không sửa service, schema, initializer, hóa đơn hoặc cách xác định số người; không đổi phân quyền CHU_NHA/PHONG_TRO, quyền sở hữu hay CSRF.
- Truy cập như S2-10:1: Dịch vụ và đơn giá → chọn tòa → Cấu hình điện nước, hoặc /DichVu/DienNuoc?toaNhaId=<id>. Đơn vị tiếp tục VND/kWh, VND/m³ và VND/người/tháng. Hiệu lực kỳ sau và nguồn số người vẫn dành cho lát được giao sau, chưa triển khai.
- Xác minh HTTP thực tế trên DB mới chứa dữ liệu giả (initializer/updater v8, obj/S2102Check được ignore): 24 trường hợp trống/0/âm của hai dịch vụ × hai cách tính × các tổ hợp phương thức PASS; lỗi đúng trường, giữ phương thức/giá vừa nhập, toàn bộ cấu hình và audit không đổi. PASS bốn tổ hợp hợp lệ/lưu/mở lại; đồng thời hai giá sai; ví dụ điện khoán 120000/nước chỉ số 0; bỏ qua ô không chọn trống/0/âm/chuỗi/overflow; ô đang chọn vẫn báo sai khi ô không chọn lỗi binding. GET/POST tòa chủ khác 403, thiếu CSRF 400, SQLite integrity/FK PASS. Không chạy/sửa project Tests/tests.
- Browser thật (Codex in-app browser, DB giả) PASS 12 trường hợp trống/0/âm cho điện/nước và hai cách tính, lỗi ngay tại ô, không ghi cấu hình/audit; giữ điện khoán 120000/nước chỉ số 0; đổi cách tính với ô cũ -2 bị ẩn rồi lưu giá mới hợp lệ thành công. Ảnh xác minh tại obj/S2102Check/validation-proof.png (ignore). Chưa kiểm tra responsive/mobile, Safari/Firefox, người dùng ghi đồng thời hoặc toàn bộ giá biên; đây không phải kết quả test tự động Tests/tests.
- Build project chính Debug PASS với output/cache riêng; lệnh từ gốc repo: `dotnet build QL_PhongTro/QL_PhongTro.csproj --no-restore -c Debug -p:IntermediateOutputPath=obj/S2102Final/ -p:OutputPath=bin/S2102Final/`. Build mặc định thất bại vì không có quyền ghi obj/Debug/net10.0/apphost.exe; không kill process/sửa môi trường. Cảnh báo có sẵn ImageSharp license/CS8601 AuthController; harness tạm có NU1900 vì không truy cập được vulnerability feed NuGet. Server xác minh dùng Logging__EventLog__LogLevel__Default=None để tránh hạn chế Windows Event Log của môi trường; không sửa config app.
- Database local thực tế Data/local-dev.sqlite đã có schema v8, chỉ khảo sát read-only; không nâng cấp/ghi DB cá nhân. Không đổi quy trình DB/README. Không stage/commit/push hay tạo PR; thay đổi project test đã có sẵn được giữ nguyên. Dừng ở S2-10:2, không thực hiện S2-10:3.

## S2-10:1 — cấu hình cách tính điện/nước theo tòa (02/10/2026)

- Hoàn thành chọn độc lập Theo chỉ số đồng hồ / Khoán theo đầu người cho điện và nước, nhập giá tương ứng, lưu cùng transaction, thông báo thành công và mở lại đúng cấu hình. Tên tòa hiển thị từ database; GET/POST đều kiểm tra CHU_NHA, quyền ghi PHONG_TRO và quyền sở hữu; POST có CSRF. Audit dùng cơ chế hiện có.
- PO chốt đơn vị: điện VND/kWh, nước VND/m³, khoán VND/người/tháng. Không tự xác định số người hoặc thay đổi logic hóa đơn. Quy tắc nguồn số người/thời điểm chốt còn cần PO quyết định trong lát tính hóa đơn sau.
- Tận dụng CauHinhDichVu (cach_tinh, don_vi_tinh, don_gia, da_chot_gia), mã DIEN/NUOC và DichVuToaNha; không thêm bảng/cột/migration, không sửa quy trình DB. Lưu trực tiếp cấu hình đang hiệu lực; chưa trì hoãn đến kỳ sau. Khi chưa có cấu hình, tạo riêng điện/nước; không tự gán cho phòng hoặc bật mặc định. Giữ phiên bản khác, trạng thái áp dụng, cờ mặc định, dịch vụ khác và snapshot hóa đơn. Kiểm tra giá tối thiểu: bắt buộc số nguyên đồng >0, hai cách tính hợp lệ; kiểm tra đầy đủ đơn giá chưa thuộc lát này.
- File: Controllers/DichVuController.cs; Services/DichVuService.DienNuoc.cs; ViewModels/CauHinhDienNuocViewModel.cs; Views/DichVu/DienNuoc.cshtml; Views/DichVu/Index.cshtml; Views/PhongTro/ToaNha.cshtml; wwwroot/js/electric-water-config.js.
- Truy cập: Chủ nhà → Quản lý tòa nhà → Cấu hình điện nước (tòa đang hoạt động), hoặc Dịch vụ và đơn giá → Cấu hình điện nước. URL /DichVu/DienNuoc?toaNhaId=<id>. Chọn cách tính, nhập giá, Lưu cấu hình. Dùng quy trình README hiện có để khởi tạo/cập nhật DB rồi chạy project chính.
- Xác minh thực tế: build project chính Debug PASS với `dotnet build QL_PhongTro/QL_PhongTro.csproj --no-restore -c Debug -p:IntermediateOutputPath=obj/S210/ -p:OutputPath=bin/S210/` (từ gốc repo); đầu ra riêng vì cache Debug cũ bị từ chối quyền ghi. Còn cảnh báo ImageSharp license và CS8601 cũ. Build thử configuration S210Verification không đạt do ImageSharp yêu cầu license; không thay đổi thư viện/cấu hình project để xử lý.
- HTTP trên database mới chứa dữ liệu giả do LocalDatabaseInitializer/updater v8 tạo, nằm trong obj/S210Check (ignore): PASS cả bốn tổ hợp cách tính, mức giá/đơn vị persisted, thông báo, mở lại; GET/POST tòa chủ khác 403 và không ghi; thiếu CSRF 400; thiếu giá/0/cách tính sai/không phải số không lưu; audit; tòa có dịch vụ mặc định; bỏ qua giá không được chọn; giữ tòa khác/dịch vụ khác/cờ mặc định; integrity/FK PASS. JavaScript kiểm tra bằng Node với DOM giả: PASS khởi tạo, chuyển lựa chọn, ẩn/hiện, disabled/required, hai khu vực độc lập.
- Chưa xác minh trực quan trên browser/mobile hoặc database của thành viên; chưa thử giá biên đầy đủ, ghi đồng thời, snapshot hóa đơn/lịch giá trong phiên này. Database local mặc định Data/local-dev.sqlite không tồn tại; không sửa database cá nhân. Chỉ thử ghi trên DB giả. Server xác minh cần tắt EventLog provider bằng biến môi trường vì môi trường không có quyền ghi Windows Event Log; không sửa mã app cho vấn đề này.
- Không sửa Tests/tests, không commit/push. File project test có thay đổi sẵn trước task được giữ nguyên. Bàn giao tại thời điểm S2-10:1: validation đầy đủ và hiệu lực kỳ sau chưa thực hiện; trạng thái S2-10:2 mới nhất ở mục phía trên.

## Quy ước README ADMIN local (02/10/2026)

- Khôi phục khối chạy nhanh trong README theo đúng yêu cầu: dừng phiên cũ, restore, updater, cấu hình ADMIN mẫu, tạo ADMIN và chạy web. Giữ nguyên thông tin ADMIN mẫu trong các lần cập nhật sau, trừ khi người dùng yêu cầu đổi; ghi quy ước tại `00-huong-dan.md`.
- Giữ hướng dẫn máy mới khởi tạo DB và máy có DB giữ dữ liệu. ADMIN đã tồn tại thì bỏ qua lệnh tạo; ví dụ không thay đổi mật khẩu tài khoản cũ.
- Xác minh: đọc cấu hình tạo ADMIN và kiểm tra diff tài liệu; không chạy khối lệnh, build/test hoặc thao tác database trong lần sửa README này. Chưa commit/push.

## S2-01 — ngừng dịch vụ phòng theo kỳ hóa đơn (02/10/2026)

- Hoàn thành bốn tiêu chí: ghi `yeu_cau_luc_utc` và `ngung_tu_ky`; PO chốt bỏ giữa tháng vẫn tính hết tháng đó, ngừng từ ngày đầu tháng sau; hóa đơn cũ hiển thị snapshot đã phát hành; kỳ sau không còn dịch vụ đã ngừng, phòng khác không đổi.
- Migration **v8** thêm `ngung_dich_vu_phong`, liên kết lựa chọn phòng/dịch vụ, unique cho lần ngừng đang mở và lịch sử áp dụng lại. Không xóa lựa chọn, giá riêng hoặc hóa đơn; updater tạo backup, chạy transaction và kiểm tra integrity/FK. DB local thực tế vẫn v5, chỉ được kiểm tra read-only; chưa chạy updater trên file local.
- `DichVuPhongService` tập trung quy tắc kỳ Việt Nam, áp dụng cho cả màn hình phòng, form hóa đơn và lúc phát hành để chống gửi form cũ. Hóa đơn chỉ đọc dòng `chi_tiet_hoa_don` snapshot khi xem lại. Có thể phát hành hóa đơn chỉ gồm tiền phòng khi phòng không còn dịch vụ trong kỳ.
- Xác minh thực tế: **40/40 xUnit PASS**, gồm hóa đơn cũ, kỳ hiện tại/kỳ sau, hai phòng độc lập, form HTTP, quyền/CSRF, giá riêng, migration v7→v8 và ranh giới cuối tháng/múi giờ Việt Nam. `python verification/s201_database.py` PASS trên bản sao v5→v8, giữ dữ liệu/hash nguồn, backup, integrity/FK, chạy lặp và khởi tạo mới không ghi đè. Build PASS; cảnh báo ImageSharp và CS8601 cũ vẫn có.
- Demo mới `verification/ServiceRemovalDemo`: chỉ tạo dữ liệu giả trong `data/service-removal-demo/` (ignore), có `DEMO-A`, `DEMO-B`, Gửi xe 100.000đ và hóa đơn kỳ trước; README có cách chạy. Không commit credential/demo DB. Chưa nghiệm thu trực quan bằng browser hoặc SMTP thật.
- File thay đổi chính: RoomServiceRemovalSchema/model/SQL v8, service/controller/view dịch vụ phòng và hóa đơn, test `RoomServicesTests.Invoices.cs`, verification và README/tài liệu CSDL. Không commit/push. File project test có thay đổi sẵn trước task, giữ riêng khi stage.

## S2-01 — đơn giá riêng dịch vụ phòng (01/10/2026)

- Bổ sung `don_gia_rieng` nullable vào từng liên kết phòng/dịch vụ; khi chưa đặt giá riêng dùng giá chung. Chủ nhà nhập giá lúc gán dịch vụ, cập nhật hoặc xóa giá riêng để quay về giá chung.
- Bảng dịch vụ phòng hiển thị giá chung, giá áp dụng, cảnh báo khi giá riêng khác giá chung và tổng dịch vụ cố định dự kiến theo giá áp dụng. Audit ghi nhận thay đổi giá riêng; quyền chủ nhà, CSRF và kiểm tra tòa/phòng tiếp tục ở backend.
- Migration v7: `RoomServicePriceSchema`, `docs/sql/S2-01-gia-rieng-dich-vu-phong.sql`; nâng cấp theo updater có backup/transaction, không đổi các lựa chọn hiện hữu hoặc hóa đơn. Không mở rộng hành vi hóa đơn khi bỏ dịch vụ giữa kỳ.
- Xác minh lần này: `dotnet restore` và `dotnet build QL_PhongTro/QL_PhongTro.csproj --no-restore` PASS. Theo yêu cầu, không chạy test. Không mở hoặc nâng cấp DB local; cần cập nhật bằng `--update-database` trước khi chạy web dùng schema v7.
- Còn lại: demo trực tiếp kịch bản giá riêng gửi xe và các phòng dùng giá chung; nghiệm thu ảnh hưởng lên hóa đơn giữa kỳ chưa nằm trong phạm vi. Chưa commit/push.

## S2-01 — dịch vụ tòa/phòng (01/10/2026)

- Nhánh `feature/S2-01/building-room-services`. Hoàn thành cờ mặc định cấp tòa, gán khi tạo phòng đơn/hàng loạt, thêm/bỏ độc lập từng phòng, bảng dịch vụ và tổng cố định/tháng theo giá chung hiện hành. Giữ quyền sở hữu/PHONG_TRO, CSRF và audit; không thêm thư viện.
- Thành phần: model/EF/audit `DichVuToaNha`, `DichVuPhong`; `DichVuPhongService`, controller/ViewModel/View tương ứng; tích hợp `DichVuService`, `DichVuController`, `PhongTroController` và Razor hiện có. Tổng dùng decimal, chỉ cộng CO_DINH có giá đang áp dụng; chưa gồm tiền thuê/phí biến đổi/dịch vụ chưa chốt giá.
- Migration **v6**: `RoomServicesSchema`, `docs/sql/S2-01-dich-vu-phong.sql`, updater; kiểm tra chỉ đọc, backup, transaction, unique/FK và chặn dịch vụ khác tòa. Cài schema dịch vụ S1-09 nếu chưa có; không cài hợp đồng/hóa đơn. Backfill danh mục tòa từ giá cũ, năm mã gợi ý bật mặc định; phòng có trước migration chưa có lựa chọn mới.
- Giả định chờ PO: đổi/thêm mặc định **không tự gán phòng cũ**, tập trung tại `GanMacDinhChoPhongMoiAsync` với TODO. Không làm giá riêng hoặc tác động hóa đơn. Mô tả PR sẵn tại [pr-s2-01.md](pr-s2-01.md); chưa tạo PR từ xa, chưa commit/push.
- Máy mới: `--initialize-database` tạo file mới bằng baseline + updater, chỉ seed vai trò/quyền; từ chối ghi đè, không có ADMIN/mật khẩu cố định. Máy có DB: dừng app, `--update-database`, `--check-database`. Sau đó chạy web, Chủ nhà vào `/DichVu`, chọn **Dịch vụ** trên danh sách phòng. Chi tiết trong README và cap-nhat-csdl.md.
- Đã `git rm --cached` đúng `QL_PhongTro/Data/local-dev.sqlite`, giữ file trên máy; mở rộng ignore database/sidecar/backup/data local. **Đồng đội cần sao lưu DB ra ngoài repository trước lần pull nhận thay đổi bỏ theo dõi**, vì Git có thể xóa file từng được theo dõi.
- Xác minh thực tế: build thành công; **32/32 xUnit PASS** (7 test S2-01: mặc định, cô lập phòng, tổng/giá chung, quyền, CSRF, audit rollback, migration và HTTP tạo đơn/hàng loạt/hiển thị riêng). Fixture Auth/Permission nay tạo DB tạm từ script, không dùng DB cá nhân và không đặt DatabasePath toàn process. `python verification/s201_database.py` PASS: bản sao local v5 → v6, giữ toàn bộ dòng cũ, backup, integrity/FK, chạy lại không đổi, khởi tạo mới/từ chối ghi đè, hash nguồn không đổi.
- DB local thực tế vẫn **v5**, chưa nâng cấp; chỉ kiểm tra nguồn bằng kết nối read-only. Chưa nghiệm thu đồ họa trình duyệt, SMTP hoặc fixture S109 lịch sử; các công cụ demo/schema cũ cần rà soát trước khi dùng với v6. Build còn cảnh báo ImageSharp có sẵn; full build trước đó có CS8601 cũ trong AuthController.
- Lint thực tế: `dotnet format --verify-no-changes` PASS cho C# mới (app và RoomServicesTests); `dotnet format style`/`analyzers --severity warn` PASS trên các file C# ứng dụng thay đổi. `git diff --check` phần task PASS; toàn working tree còn dòng trống cuối project test có sẵn trước task. Đã kiểm tra status/staged và ignore: staged chỉ có việc bỏ theo dõi DB, không có database/backup/bí mật được thêm mới.
- Bước tiếp: PO chốt chính sách phòng cũ; review mô tả PR, thử nâng cấp trên bản sao máy đồng đội và nghiệm thu UI. File `Tests/QL_PhongTro.Tests/QL_PhongTro.Tests.csproj` có thay đổi sẵn trước task, giữ nguyên để chủ máy quyết định stage.

Các mục dưới đây là bàn giao trước S2-01; thông tin v6/quy trình database ở trên và README thay thế ghi chú cũ tương ứng.

Cập nhật: 29/09/2026. Tổng hợp tài liệu S1-03, S1-04, S1-05, S1-09 (chức năng/demo), S1-10 và tiến độ cũ. Kết quả kiểm thử từ các phiên trước được ghi riêng, không phải lần chạy mới khi biên tập tài liệu.

## Trạng thái hiện tại

### Bàn giao nhánh giới hạn sức chứa (02/10/2026)

- Theo yêu cầu push `feature/S2-06/validate-room-occupancy-limit`, commit AC sức chứa rồi hợp nhất nhánh đích `ca9c469`, giữ cả S2-06 và dịch vụ/phòng S2-01. Giải quyết xung đột DbContext, đăng ký service/CLI, initializer và tài liệu; giữ cấu hình ADMIN riêng thay vì mật khẩu cố định. Không force-push, không thay database local. Project test có thay đổi sẵn tiếp tục giữ local, không stage.
- Sau merge: build Debug PASS; toàn xUnit **44/44 PASS**, 0 lỗi/0 bỏ qua (`data/test-results/occupancy-merge-regression.trx`); HTTP S2-06 PASS trên DB giả mới `data/S2-06/20261002-024157-1f4175`, server tự dừng. Fixture Auth/Permission mới tự tạo DB tạm, không cần DB mặc định. DB mặc định vẫn chưa tồn tại. Chưa chạy lại UI/mobile hoặc nâng cấp DB nguồn trong lượt merge.
- Trạng thái schema sau merge: nền **v8** theo nhánh đích, rental_request_schema=1 cài riêng. Ghi chú v5 bên dưới thuộc lượt trước merge; máy có DB giữ dữ liệu dùng updater có backup theo README/cap-nhat-csdl.md khi được phép cập nhật. Không chạy updater trên DB thật trong task push. AC sức chứa không bổ sung bước schema.

### S2-06 — giới hạn số người dự kiến ở (02/10/2026)

- Hoàn tất AC sức chứa: service đọc `so_nguoi_toi_da` của phòng gắn với tin public trong transaction khóa ghi, từ chối trước khi tạo profile/cấp mã/lưu yêu cầu. Controller gắn lỗi vào `Form.SoNguoiDuKien`; form có max theo phòng, hướng dẫn và thông báo tiếng Việt “Phòng chỉ cho phép tối đa N người.”. Dữ liệu sức chứa gửi từ client không được dùng. Giữ kiểm tra số nguyên dương; chưa kiểm soát yêu cầu đang mở trùng.
- File: `Services/YeuCauThueService.cs`, `Controllers/TinDangController.cs`, `Views/TinDang/ChiTiet.cshtml`, `verification/s206_http.py`, README và tiến độ. Không đổi schema/quy trình DB; không có DatabasePath override, DB mặc định vẫn chưa tồn tại. Giữ thay đổi project test có sẵn; không commit/push.
- Xác minh thực tế: build Debug `--no-restore -o data/S2-06/runtime` PASS, 0 lỗi (còn ImageSharp/CS8601 cũ). HTTP PASS trên DB giả mới `data/S2-06/20261002-022828-906055`: 1/2/3/4 được lưu cho phòng tối đa 4; 5 bị từ chối cho XEM_PHONG và THUE_NGAY, lỗi đúng trường/giữ giá trị nhập. Thử giới hạn phòng 2 thì 3 bị từ chối với thông báo đúng 2. Sau các POST vượt giới hạn, yêu cầu/profile/counter/audit đều không đổi. Hồi quy ngày, CSRF/quyền/sở hữu, mã/đồng thời, audit/rollback/hết mã, tin public và integrity/FK PASS. Server kiểm thử đã dừng.
- Lượt sandbox đầu lỗi Event Log/Data Protection, đã dừng đúng tiến trình thử và chạy lại ngoài sandbox thành công. Không có DB nguồn nên không chạy nhánh thử nâng cấp bản sao nguồn; chưa nghiệm thu tương tác UI/mobile hoặc chạy lại toàn bộ xUnit.
- Kiểm thử lại theo yêu cầu người dùng: xUnit PASS **29/29**, 0 lỗi/0 bỏ qua; kết quả `data/test-results/capacity-regression.trx` (auth, quyền, tòa/phòng, hồ sơ và ngày). Fixture cũ đọc đường dẫn DB mặc định: chỉ khởi tạo nền rỗng khi file chưa tồn tại bằng CLI, các test chạy trên bản sao tạm, sau đó xóa đúng file nền vừa tạo; DB mặc định trước/sau đều không tồn tại. Không dùng dữ liệu cá nhân. HTTP S2-06 chạy lại PASS trên fixture mới `data/S2-06/20261002-023112-d66378`, cổng 5267, kiểm tra đầy đủ sức chứa và hồi quy như trên; nguồn kiểm thử được đặt tới đường dẫn chưa tồn tại để không đọc fixture nền của xUnit đang chạy.
- UI Browser thực tế PASS: đăng nhập khách giả, nhập 5 bị chặn tại ô số người với thông báo tối đa 4; sửa thành 4 xóa lỗi và gửi thành công, mã `YC-202610-0010`, trang xác nhận ghi số người 4. Ảnh `data/S2-06/capacity-validation-ui.png` và `capacity-success-ui.png`. Kết quả này thay thế giới hạn “chưa nghiệm thu UI/chạy toàn xUnit” ở lượt triển khai; chưa kiểm tra mobile hoặc SMTP thật. Server thử đã dừng; không commit/push. Lần này chỉ cập nhật tiến độ, không sửa mã ứng dụng.
- Chạy/test: build rồi `python verification/s206_http.py`; chạy demo qua `data/S2-06/latest.txt` theo README, phòng S206-101 tối đa 4 người. Không cần updater cho task này. Giả định giới hạn áp dụng cả hai loại yêu cầu, gồm người gửi trong tổng số dự kiến ở. Bước tiếp: nghiệm thu UI/mobile; kiểm soát yêu cầu mở trùng là task riêng.

### Xử lý xung đột PR #24 (02/10/2026)

- Nhánh nhận PR là main, đã revert tại 6d08d0f về trạng thái chỉ còn tài liệu. Theo yêu cầu xử lý xung đột và chỉ push feature/S2-06/validate-desired-date-range, merge lịch sử main vào feature bằng chiến lược ours, giữ toàn bộ cây mã/tài liệu feature đã kiểm thử; không đưa việc xóa ứng dụng vào feature.
- PR so với main sẽ khôi phục ứng dụng và các chức năng hiện có, không chỉ riêng giới hạn ngày. Không thay schema/DB, không đưa DB/backup/credential local vào commit. Project test khác do Tests/tests vẫn giữ local.
- Xác minh: cây mã ứng dụng/script/test trong index không đổi so với commit 0a3d675; build qua dotnet test và 4/4 DesiredDateTests PASS. Kết quả 29/29 và HTTP S2-06 PASS là lượt kiểm thử trước trên cùng mã; không chạy lại full suite trong lượt xử lý merge.
- Chỉ push feature; chưa merge PR vào main/dev. Người duyệt cần lưu ý phạm vi khôi phục mã trước khi merge PR.


### S2-06 — gửi yêu cầu, mã xác nhận và giới hạn ngày (02/10/2026)

- Hoàn tất AC giới hạn ngày: khoảng gồm cả hôm nay và ngày +60, theo Asia/Ho_Chi_Minh qua ITimeProvider. Controller trả lỗi tại `Form.NgayMongMuon`; service kiểm tra lại trước transaction/ghi dữ liệu. Form có min/max, hướng dẫn khoảng ngày và lỗi tiếng Việt tại trường ngày, kể cả khi nhập tay. Giữ loại yêu cầu, CSRF, quyền, mã theo tháng, audit/rollback hiện có; chưa kiểm tra sức chứa hoặc yêu cầu mở trùng.
- File thay đổi: `Services/YeuCauThueService.cs`, `Controllers/TinDangController.cs`, `ViewModels/GuiYeuCauViewModel.cs`, `Views/TinDang/ChiTiet.cshtml`; cập nhật `verification/s206_http.py`, thêm `Tests/QL_PhongTro.Tests/DesiredDateTests.cs`, README và ignore `/data/test-results/`. Giữ thay đổi project test có sẵn do Tests/tests trùng trên Windows; không commit/push.
- Kiểm tra cấu hình thực tế: appsettings và launch profile không đặt DatabasePath; không có override môi trường. Mặc định `QL_PhongTro/Data/local-dev.sqlite` hiện không tồn tại. Không tạo DB đang sử dụng, không đổi schema/quy trình DB, không sửa dữ liệu cũ. Schema nền v5 và rental_request_schema=1 giữ nguyên. Đã bỏ theo dõi DB trong task trước; đồng đội vẫn cần sao lưu ngoài repo trước pull thay đổi bỏ theo dõi.
- Xác minh thực tế: build Debug `data/S2-06/date-runtime` PASS, 0 lỗi; còn cảnh báo ImageSharp/CS8601 cũ. HTTP bản cuối cổng 5267 PASS trên fixture mới `data/S2-06/20261002-011420-6c62e3`: hôm nay/+60 được lưu, hôm qua/+61 bị từ chối cho cả hai loại, lỗi đúng trường/giữ giá trị nhập; dữ liệu yêu cầu/profile/counter/audit không đổi sau ngày sai. Toàn bộ hồi quy S2-06 cũng PASS (CSRF/role/ownership, mã/concurrency, audit/rollback/hết mã, tin public, integrity/FK). Nguồn local không tồn tại nên nhánh nâng cấp bản sao nguồn trong script không chạy; không khẳng định đã test nâng cấp DB cũ lần này. Chạy ngoài sandbox do Event Log/Data Protection.
- Unit test mới: `dotnet test Tests/QL_PhongTro.Tests/QL_PhongTro.Tests.csproj --no-restore --filter FullyQualifiedName~DesiredDateTests` PASS 4/4, kiểm tra biên chuyển ngày Việt Nam lúc 17:00 UTC và service từ chối trước khi truy cập DB. Lượt kiểm thử lại theo yêu cầu (02/10): toàn bộ xUnit PASS 29/29 (25 auth/quyền/tòa-phòng/hồ sơ + 4 ngày), kết quả `data/test-results/regression-final.trx`. Dùng DB trống tạo tạm bằng CLI tại đường dẫn fixture bộ test cũ và xóa khi xong; DB mặc định trước/sau không tồn tại. Build runtime PASS, HTTP S2-06 chạy lại cổng 5267 PASS trên fixture `data/S2-06/20261002-011757-219438`, server tự dừng. Không chạy lại UI lần này; dùng kết quả UI thực tế trong task triển khai bên dưới.
- UI Browser thực tế: ngày 02/10/2026 và 01/12/2026 hợp lệ; 01/10 và 02/12 không hợp lệ, có lỗi tiếng Việt ngay dưới ô ngày; bấm gửi với ngày +61 bị chặn tại form. Ảnh `data/S2-06/date-validation-ui.png`. HTTP đã xác minh lưu thành công các biên; UI lần này chỉ kiểm tra chọn ngày/chặn gửi, chưa kiểm tra mobile.
- Chạy/test: `dotnet build QL_PhongTro/QL_PhongTro.csproj -c Debug -o data/S2-06/runtime`, `python verification/s206_http.py`. Đọc access.json/latest.txt và khởi động demo theo README; không chia sẻ credential hoặc DB fake. Không cần updater cho thay đổi giới hạn ngày. Bộ HTTP tự dừng server.
- Giả định: “hôm nay” là ngày Việt Nam, 60 ngày lịch và gồm cả hai đầu. Bước tiếp: nghiệm thu mobile, tích hợp tin thật; sức chứa/trùng yêu cầu là task riêng. Lỗi đăng nhập đồng thời SQLite locked đã được ghi nhận trước đây, ngoài phạm vi này.
### Nhánh đăng ký, đăng nhập, phân quyền (30/09/2026)

- Đã tạo nhánh `Feature/fix/dang-ky-dang-nhap-phan-quyen` từ `dev`. Bổ sung schema v4: `email_confirmed`, `is_deleted`, `email_confirmation`; cập nhật có backup qua `--update-database`, không tự chạy khi web khởi động.
- Đăng ký khách thuê tạo tài khoản chờ xác nhận, gửi mã 6 số qua `IRegistrationEmailSender`, mã hết hạn sau 15 phút; form `/Account/ConfirmEmail` có Gửi lại mã. Mã hết hạn khi người dùng gửi form sẽ hủy mềm tài khoản chờ xác nhận; đăng nhập bị chặn trước xác nhận. SMTP thật dùng cấu hình `PasswordReset__*`; chưa có SMTP thật trong môi trường nên chưa xác minh thư đến inbox.
- Thêm nút hiện/ẩn mật khẩu ở đăng nhập/đổi mật khẩu, xác nhận khi đăng xuất, hiển thị vai trò cạnh tên tài khoản và thông báo hết phiên khi polling phát hiện cookie không còn. Cookie hiện hết hạn 30 phút theo cấu hình app; request backend vẫn là lớp bảo vệ chính.
- ADMIN đã lọc/thấy tài khoản KHACH_THUE; thêm xóa mềm tài khoản không phải ADMIN. Xóa QUAN_LY hủy kích hoạt, thu hồi phiên và chuyển các tòa đang giao về `Chưa phân công`; dữ liệu lịch sử vẫn giữ.
- Build Debug PASS, còn cảnh báo ImageSharp license và CS8601 cũ. Kiểm tra updater v4 trên bản sao bị Windows Application Control chặn khi chạy DLL trong môi trường này; chưa ghi DB gốc. Cần chạy `--update-database`/`--check-database` trên bản sao bằng terminal Windows bình thường trước khi dùng nhánh.

### Cập nhật giao diện dịch vụ/hóa đơn (30/09/2026)

- Danh sách dịch vụ có nút Sửa đơn giá và Xóa (xác nhận trước khi gửi), dùng Manage/Delete hiện có; giữ CSRF, quyền ghi và chặn xóa dịch vụ đã tham chiếu. Phạm vi sửa là đơn giá/lịch sử, chưa thêm sửa tên/cách tính.
- Hóa đơn gần đây thêm mã phòng/tên tòa qua ViewModel và truy vấn join, giữ lọc theo tòa/chủ nhà và 30 hóa đơn mới nhất; không thay schema/snapshot hóa đơn.
- File: Views/DichVu/Index.cshtml, Views/HoaDonDichVu/Index.cshtml, ViewModels/HoaDonDichVuViewModels.cs, Controllers/HoaDonDichVuController.cs. Build Debug PASS (ImageSharp và CS8601 cũ); HTTP đăng nhập Chủ nhà, form sửa/xóa có CSRF, hóa đơn có đúng DEMO-101/tên tòa PASS; diff phần mã sạch. Chưa thử xác nhận JavaScript bằng trình duyệt hoặc chạy lại thao tác ghi sửa/xóa.
- Demo cùng DB/tài khoản chạy tại localhost:5250, PID mới 20944; tải lại trang để dùng. Không đổi DB gốc, không commit/push. Bước tiếp: nghiệm thu giao diện và thao tác trên dịch vụ thử chưa sử dụng nếu cần.

- ASP.NET Core MVC/C#, .NET 10, EF Core SQLite; HTML/CSS trong Razor Views. MVC dùng cookie, API xác thực có JWT; mật khẩu BCrypt.
- Đã có đăng ký/đăng nhập, tài khoản quản lý, phân quyền, mật khẩu, hồ sơ, tòa/phòng, dịch vụ/hóa đơn tối thiểu và nhật ký. Trang danh mục module chưa có nghiệp vụ không được coi là đã hoàn thành backlog.
- Web yêu cầu SQLite đã tồn tại; máy mới khởi tạo rõ ràng bằng `--initialize-database`, không tự tạo khi startup. Schema nền **v5**, giữ ngoại lệ tự nâng v4 → v5 có backup; S2-06 là module tùy chọn version 1 cài qua CLI. Hợp đồng/dịch vụ/hóa đơn cũng là schema tùy chọn; bảng đã có phải đủ cột tương ứng.
- Staging/Docker, reset-seed và dashboard thử nghiệm đã gỡ. Các ghi chú staging cũ không còn áp dụng.
- Theo bàn giao local ngày 29/09: 6 tài khoản, 1 hồ sơ, 1 tòa, 4 phòng, 8 dòng nhật ký; chưa cài S1-09. Backup trước demo: `data/backups/local-dev.before-small-demo-20260929-074854.sqlite`. Đây là dữ liệu riêng trên máy, không bảo đảm có trên máy đồng đội; lần biên tập này không mở DB để kiểm tra lại.
- DB local đã được bỏ theo dõi bằng `git rm --cached`, giữ file trên máy. Đồng đội phải sao lưu DB ra ngoài repository trước lần pull nhận thay đổi này vì Git có thể xóa file đã theo dõi. Clone mới dùng quy trình khởi tạo rõ ràng trong README; không chia sẻ DB local.

## Chạy và dữ liệu

Hướng dẫn chung: [README](../README.md), [cập nhật SQLite](cap-nhat-csdl.md). Dừng đúng phiên server bằng Ctrl+C trước khi build, sao chép hoặc nâng cấp DB; không chạy updater/web đồng thời trên cùng file.

- Mặc định `QL_PhongTro/Data/local-dev.sqlite`; biến môi trường `DatabasePath` chọn file khác. Không chép đè DB đồng đội, không dùng EnsureDeleted/EnsureCreated để nâng cấp.
- `--check-database` chỉ kiểm tra; `--update-database` sao lưu `*.before-update-<id>.bak` và ghi `app_schema_version`. Chỉ nâng cấp DB đang dùng khi được yêu cầu. v1: hạ tầng nền; v2: must_change_password và unique email chuẩn hóa/điện thoại; v3: nhật ký, snapshot tên, trigger bảo vệ. Dữ liệu tài khoản trùng làm v2 dừng, không tự gộp/xóa.
- Bước v3 có transaction, kiểm tra schema/integrity/FK; các bước v1/v2 cũ không phải một transaction chung cho toàn updater. Khi lỗi cần xem backup và trạng thái thực tế; không tự phục hồi đè dữ liệu. Check không chứng minh mọi constraint/nghiệp vụ đều đúng.
- ADMIN local: cấu hình `LocalAdmin__Email`, `LocalAdmin__Password`, `LocalAdmin__Phone` rồi chạy `--create-local-admin` trong Development trên đúng DatabasePath đã cập nhật. Lệnh có backup, từ chối email/điện thoại trùng. Không đưa credential, DB, backup hoặc email local vào Git.

Ví dụ chạy trên **bản sao mới** từ gốc repo, sau khi dừng app dùng DB nguồn:

```powershell
New-Item -ItemType Directory -Force data | Out-Null
if (Test-Path data/review.sqlite) { throw 'Chọn tên bản sao mới, không ghi đè.' }
Copy-Item -LiteralPath QL_PhongTro/Data/local-dev.sqlite -Destination data/review.sqlite
$env:DatabasePath = (Resolve-Path data/review.sqlite).Path
dotnet restore QL_PhongTro/QL_PhongTro.csproj
dotnet build QL_PhongTro/QL_PhongTro.csproj -c Debug --no-restore
dotnet run --project QL_PhongTro --no-build -- --update-database
dotnet run --project QL_PhongTro --no-build -- --check-database
$env:PasswordReset__PublicBaseUrl = 'http://localhost:5247'
$env:PasswordReset__PickupDirectory = Join-Path $env:TEMP 'qlphongtro-mail-preview'
dotnet run --project QL_PhongTro --no-build --launch-profile http
```

Mở `http://localhost:5247/Account/Login`. Sau khi dừng demo, xóa biến DatabasePath khỏi terminal nếu muốn về DB mặc định. Biến môi trường chỉ có hiệu lực trong terminal đã đặt.

## Tài khoản, phân quyền và mật khẩu (S1-01–S1-05)

- Đăng ký chỉ tạo KHACH_THUE. Đăng nhập bằng email/điện thoại; xác thực có khóa tạm khi sai mật khẩu, JWT access 30 phút và refresh 7 ngày. Quyền và trạng thái tài khoản kiểm tra ở backend.
- `/ManagedAccounts`: chỉ ADMIN tạo/khóa/mở khóa Chủ nhà và Quản lý; không tự khóa hoặc khóa ADMIN/Khách thuê tại chức năng này. Danh sách mọi vai trò, lọc vai trò/trạng thái, 20 dòng/trang.
- Mật khẩu tạm 16 ký tự ngẫu nhiên đủ bốn nhóm, gửi email và buộc đổi trước khi dùng chức năng khác. API không cấp token khi còn chờ đổi. Gửi lỗi vẫn giữ tài khoản chờ đổi; gửi lại thay mật khẩu tạm cũ. SMTP và DB không cùng transaction.
- Khóa/đổi mật khẩu thu hồi cookie/JWT/refresh cũ. Trang polling 20 giây; tab treo/mất mạng không bảo đảm tự chuyển đúng hạn, nhưng request tiếp theo vẫn bị chặn.
- `/Account/ChangePassword`: mọi vai trò đã đăng nhập; kiểm tra mật khẩu hiện tại, mới khác cũ, ít nhất 8 ký tự có chữ/số, xác nhận khớp và CSRF. Thành công phải đăng nhập lại. Ghi chú cũ “chỉ khách thuê, không thu hồi phiên” đã hết hiệu lực.
- ForgotPassword/ResetPassword hiện chỉ cho KHACH_THUE đang hoạt động. Token 256 bit, DB lưu SHA-256, hạn 30 phút/dùng một lần; tối đa 3 yêu cầu/email/giờ, lần 4 trả 429. Email không tồn tại trả thông báo trung tính; gửi lỗi vẫn tính lượt và xóa token gửi lỗi. Reset vô hiệu hóa token/phiên/refresh và bỏ khóa đăng nhập tạm trong transaction.
- `/Permissions`, `/api/permissions`: ADMIN/CHU_NHA xem bốn vai trò, KHACH_THUE chỉ vai trò mình. Quyền đọc từ app_role/app_module/role_permission; seed tại `Data/permissions.seed.json` gồm 4 vai trò/9 module/36 cặp, không cấp lại quyền đã thu hồi khi chạy lại.
- Quyết định PO khác Excel: **QUAN_LY không truy cập TAI_CHINH**. Ma trận còn lại theo yêu cầu và seed. Thiếu cặp quyền thì từ chối; OwnDataOnly không thay kiểm tra sở hữu ở nghiệp vụ mới.
- MVC chưa đăng nhập chuyển Login; MVC trái quyền trả trực tiếp 403; API của cơ chế quyền trả JSON 401/403. Cookie/Data Protection tách theo đường dẫn DB. Ghi chú cũ “không dùng JWT” hoặc “trái quyền luôn redirect” không còn đúng.
- Thành phần: `Controllers/AccountController*.cs`, AuthController, ManagedAccountsController, PermissionsController; `Authorization/*`; Services AuthService, TokenService, SessionVersionStore, PasswordResetService, PasswordEmailSender và ViewModels/Razor tương ứng.

Email local là file .eml trong pickup, không gửi inbox. SMTP cần các khóa `PasswordReset__Host`, Port, EnableSsl, From, Username, Password, PublicBaseUrl; đặt `PasswordReset__PickupDirectory = ' '` để ghi đè pickup Development. Production yêu cầu URL HTTPS; SMTP dùng STARTTLS (thường 587). Không commit bí mật; máy chủ nhận thư chưa chứng minh thư tới inbox.

Demo sau build: `python verification/prepare_s103_demo.py data/s103-demo-new.sqlite`. Cần DB nguồn tồn tại; script tạo bản sao mới, in credential ngẫu nhiên, thêm 45 Quản lý hoạt động + 8 khóa, từ chối ghi đè. Đặt DatabasePath bản sao rồi chạy web. Nghiệm thu: ADMIN tạo/trùng/validation → đọc .eml → đăng nhập/ép đổi → gửi lại → khóa/mở khóa ở phiên thứ hai → lọc/phân trang và thử trái quyền.

## Hồ sơ và tòa/phòng (S1-06–S1-08)

- `/HoSo`: tạo/sửa hồ sơ mình; căn cước 9/12 chữ số, để trống khi sửa giữ giá trị cũ. `/HoSo/Xem?id=<id>`: tra cứu theo quyền. Server che chỉ còn 4 số cuối; ADMIN/chủ nhà của phòng đang thuê mới thấy đầy đủ. Query/form không nâng quyền.
- Quyền chủ nhà dựa hợp đồng hiệu lực, kỳ thuê, người đứng tên/người ở ghép và ngày ở thực tế tại Việt Nam; thiếu schema/quan hệ thì không mở dữ liệu nhạy cảm. Người không liên quan chỉ nhận thông tin giới hạn.
- Ảnh JPG/PNG tối đa 5MiB mỗi ảnh, resize rộng tối đa 1600px, giữ tỷ lệ; giới hạn 50 megapixel, frame đầu, request tổng 16MiB. Cho bổ sung từng mặt để tương thích hồ sơ cũ. Lưu ngoài wwwroot; endpoint kiểm tra quyền/no-store; chủ hồ sơ vẫn xem ảnh mình.
- IdentityImagePath mặc định `QL_PhongTro/App_Data/identity-images`; sao lưu DB và ảnh. Giấy tờ/ảnh chưa mã hóa trên đĩa; chưa xử lý hoàn chỉnh tệp mồ côi sau crash.
- Tòa/phòng: tạo tòa, gán quản lý, sửa/ngừng/xóa theo ràng buộc; tạo phòng đơn/hàng loạt, lọc trạng thái, mã không trùng trong tòa, giá tối thiểu 500.000đ. Quy tắc mã tầng + 2 chữ số, giới hạn 1..99 còn cần PO xác nhận.
- Thành phần: HoSoController, PhongTroController, Services HoSoAccess/GiayToImageStore, Models/ViewModels/Views tương ứng và SQL hồ sơ/quan hệ thuê trong `docs/sql`. Schema quan hệ thuê để kiểm tra quyền không đồng nghĩa đã có CRUD hợp đồng đầy đủ.

## Dịch vụ và hóa đơn tối thiểu (S1-09)

- `/DichVu?toaNhaId=<id>`: dịch vụ theo tòa của chủ nhà, ba cách tính (chỉ số/người/cố định), validation/CSRF. Khởi tạo DIEN, NUOC, RAC, GUI_XE, INTERNET bằng POST; marker/transaction/unique chống trùng; không tạo lại dịch vụ đã chủ động xóa.
- Giá chưa chốt hiển thị “Chưa thiết lập”, không được chọn lập hóa đơn. Điện/nước mặc định >0, dịch vụ khác cho phép 0. `DichVuMacDinh:DanhSach` cấu hình Ma/Ten/CachTinh/DonVi/DonGia chỉ áp dụng khi khởi tạo mới.
- `/DichVu/Manage`: giá đầu chỉ sửa trực tiếp khi chưa dùng/chưa nhiều phiên bản. Giá mới đóng khoảng cũ trước ngày hiệu lực, trùng ngày bị từ chối. Ngừng/kích hoạt từ kỳ sau và sau phiên bản mới nhất; cách triển khai hiện tại không hồi tố/chèn trước phiên bản mới nhất.
- Chặn xóa nếu có cấu hình phòng riêng, tham chiếu hợp đồng/hóa đơn hoặc snapshot không rõ cấu trúc. Đọc hop_dong_dich_vu, dòng hóa đơn và ky_hop_dong.thong_tin_chot; JSON nhận diện: `{"dich_vu":[{"dich_vu_id":123}]}`.
- `/HoaDonDichVu`: chỉ hợp đồng hiệu lực thuê **trọn tháng với một giá phòng**; từ chối kỳ lẻ, gia hạn/trả phòng giữa tháng và cấu hình phòng riêng. Tiền phòng lấy từ kỳ thuê; số người/chỉ số do chủ nhà nhập, chưa tích hợp chỉ số/người ở ghép. Cuối ≥ đầu, không âm, tối đa 3 chữ số thập phân.
- Tính decimal, làm tròn từng dòng tới đồng bằng AwayFromZero, kiểm tra tràn số. Snapshot tên/cách tính/đơn vị/giá/chỉ số/số lượng/thành tiền; unique chống lặp hợp đồng/tháng, trigger khóa hóa đơn/dòng đã phát hành; hạn trả 7 ngày từ ngày phát hành tại Việt Nam.
- `HoaDonDichVuService.GanVaoHopDongAsync` tích hợp snapshot hợp đồng. Chưa làm đầy đủ nháp/chỉnh sửa/hủy/thay thế, thanh toán, email, tiền phòng theo ngày hoặc CRUD hợp đồng.
- PO đã chốt trùng ngày từ chối, giữ snapshot, ngừng/kích hoạt kỳ sau. Giá thương mại và đơn vị/cách tính còn chờ: đề xuất điện kWh, nước m³ theo chỉ số; rác theo người; gửi xe/internet theo phòng. Không tự dùng giá demo cho DB thật.
- Thành phần: Models/ViewModels/Controllers/Views DichVu và HoaDonDichVu; Services DichVuService*, DichVuThamChieu, HoaDonDichVuService; Data/DichVuSchemaInitializer; SQL `S1-09-dich-vu.sql`, `S1-09-hoa-don.sql`.

Chỉ cài module trên DB thử khi chưa được yêu cầu cài DB thật. Lệnh demo: `dotnet run --project verification/S109/S109.csproj -- . --prepare-demo`; tạo fixture riêng và ghi credential vào demo-access.txt được ignore. Công cụ cũ chưa được chạy lại với v3 trong lần tổng hợp; kiểm tra tương thích trước khi dùng. Các lệnh `--initialize-services`, `--initialize-service-invoices` cần schema tiên quyết đã kiểm tra, có backup; không tự tạo bảng tiên quyết. Không chạy lại SQL tổng hợp trên schema AC1 cũ.

Demo lịch sử 28/09: `data/S1-09-verification/20260928102752-c3ffa2/test.sqlite` (nếu còn trên máy), phòng DEMO-101 giá 2.000.000đ; điện 3.000 → 3.500 từ 01/10, nước 15.000, rác 20.000/người, gửi xe 50.000/phòng, internet 100.000/phòng. Tháng 9 chỉ chọn điện 100→110 cho tổng 2.030.000đ; tháng 10 dự kiến 2.035.000đ, hóa đơn cũ không đổi. Điện chặn xóa do hóa đơn, nước do hợp đồng; dịch vụ chưa dùng xóa được; dịch vụ trạng thái ngừng 01/10, bật lại 01/11. Đây là số liệu/kịch bản thử, không khẳng định demo đang chạy hoặc tháng 10 chưa có hóa đơn.

## Nhật ký hoạt động (S1-10)

- `/NhatKy` chỉ ADMIN, xác minh role/trạng thái từ DB; 50 dòng/trang, mới nhất trước, lọc kết hợp ngày/người/loại, xem JSON trước/sau. Lưu UTC, hiển thị UTC+7; lọc ngày Việt Nam theo [đầu ngày từ, đầu ngày sau ngày đến), năm 1900–9998.
- Audit cùng transaction cho tài khoản tạo/khóa/mở khóa/gửi lại mật khẩu tạm/tự đăng ký; tòa/phòng; dịch vụ/giá/trạng thái; hóa đơn/dòng/phát hành và gán dịch vụ hợp đồng qua SaveChanges. Chưa có CRUD hợp đồng, chỉ số độc lập, thanh toán nên chưa có luồng audit nghiệp vụ tương ứng.
- `Data/AppDbContext.Audit.cs` dùng allowlist, snapshot tên/role truy vấn từ DB. Không ghi mật khẩu/hash/token, email/điện thoại tài khoản, hồ sơ/giấy tờ/ảnh hoặc ghi chú tự do. Sửa chỉ ghi trường đổi, no-op không ghi; gửi lại mật khẩu có hành động riêng dù JSON rỗng, không chứng minh email đã đến.
- SaveChanges dùng transaction hoặc savepoint khi có transaction ngoài; lỗi audit rollback nghiệp vụ. EF chặn thêm tay/sửa/xóa entity nhật ký; trigger audit_no_update/audit_no_delete chặn sửa/xóa SQL. Không có endpoint ghi nhật ký. Người quản trị file SQLite vẫn có thể bỏ trigger.
- v3 giữ nhật ký cũ; thiếu snapshot tên thì NULL, không suy đoán từ tên hiện tại. ghi_chu giữ tương thích schema, không ghi text tự do từ request.
- Không audit hạ tầng login/refresh/reset/đổi mật khẩu thường, quyền module, hồ sơ nhạy cảm hoặc CLI. Bulk SQL/ExecuteUpdate của nghiệp vụ mới cần tích hợp riêng; ChangeTracker không tự bắt. Nhật ký theo đối tượng/mỗi SaveChanges, không gộp toàn request.
- Thành phần: Models/NhatKyHoatDong, Data/AppDbContext.Audit, AuditSchema, DatabaseUpdates; NhatKyController, NhatKyViewModel, Views/NhatKy/Index; tích hợp Account/ManagedAccounts và layout.

Nghiệm thu: ADMIN/non-ADMIN/anonymous; tạo/khóa/mở khóa/gửi lại tài khoản; tạo/sửa tòa/phòng; trên fixture S1-09 thử đổi giá/phát hành. Kiểm tra snapshot không chứa bí mật, lọc biên ngày/kết hợp/phân trang; no-op/form lỗi không thêm audit. Thử rollback/SQL bất biến chỉ trên DB kiểm thử.

## Xác minh và giới hạn

Kết quả ghi nhận trong các phiên 27–29/09/2026, chưa chạy lại khi chỉnh tài liệu:

| Phạm vi | Kết quả bàn giao | Công cụ hiện có |
| --- | --- | --- |
| S1-03 | HTTP PASS tạo/trùng/validation/CSRF/email pickup/lỗi gửi/ép đổi/khóa/cookie-JWT-refresh/lọc/phân trang; hash DB gốc không đổi | `verification/s103_http.py` |
| S1-04 | 7 test quyền và HTTP bốn vai trò từng đạt; bản merge sau đó từng lỗi fixture thiếu toa_nha; chưa xác nhận lại toàn suite hiện tại | `Tests/QL_PhongTro.Tests/PermissionTests.cs` |
| S1-05 | Bộ riêng từng PASS đổi/reset/hết hạn/giới hạn/đồng thời/thu hồi phiên trên bản sao và email pickup | `verification/S105` được nhắc trong tài liệu cũ nhưng **không có trong checkout hiện tại** |
| S1-06 | HTTP hồ sơ/ảnh/quyền PASS; từng xem che/đầy đủ trong trình duyệt | `Tests/s1_06_http.py`, `s1_06_images.py`, `s1_06_permissions.py` |
| S1-09 | Build và các luồng HTTP demo đạt; console suite từng bị Windows Application Control chặn, chưa nghiệm thu đầy đủ | `verification/S109/` |
| Updater/S1-10 | HTTP, schema lỗi/lặp/bảo toàn, console EF/savepoint/rollback và hồi quy S1-03 PASS; integrity/FK đạt, hash DB nguồn không đổi | `verification/database_updates.py`, `s110_schema.py`, `s110_http.py`, `verification/S110/` |

- Sau build, các lệnh kiểm chứng: `python verification/database_updates.py`, `python verification/s103_http.py`, `python verification/s110_schema.py`, `python verification/s110_http.py`. Đọc fixture trước khi chạy, chỉ dùng bản sao. Sau HTTP S1-10 có thể build S110.csproj rồi chạy DLL với đường dẫn gốc repo và audit.sqlite do HTTP suite tạo.
- Python S1-06 nhận URL app đang chạy và DB thử; script ảnh cần Pillow và thư mục ảnh riêng. Các giả định đăng ký/redirect có từ trước merge, cần cập nhật fixture theo auth/CSRF hiện tại trước khi chạy lại.
- Không chạy toàn bộ AuthTests vào DB mặc định: fixture cũ có EnsureDeleted/EnsureCreated. Chưa xử lý va chạm Git Tests/tests; giữ thay đổi local project test.
- Build Debug từng đạt; còn ImageSharp license, CS8601 ở AuthController và NU1900 khi không truy cập NuGet. Release cần license ImageSharp hợp lệ; NU1900 không chứng minh đã kiểm tra lỗ hổng.
- Chưa xác minh SMTP thật, UI đồ họa/responsive S1-03/S1-09/S1-10 hoặc tải đồng thời/crash toàn hệ thống. Test đồng thời S1-05 không thay thế concurrency các module khác.

## Việc tiếp theo và lần cập nhật này

### Demo dịch vụ/hóa đơn đã chuẩn bị (29/09/2026)

- Thêm `verification/prepare_service_demo.py`, hướng dẫn README và ignore `/data/service-demo/`. Tạo bản sao mới từ schema nguồn đã đọc chỉ đọc, không ghi DB gốc; công cụ không ghi đè demo, từ chối nguồn đã có schema tùy chọn cần rà soát.
- Demo thành công tại `data/service-demo/20260929-231718-28a08b/demo.sqlite`, server nền localhost:5250, PID lúc tạo 8944. Có bốn tài khoản mới, tòa ID 4, ba phòng DEMO-101/102/103, hai hợp đồng HD-DEMO-1/2 (24 tháng từ 01/09/2026), năm dịch vụ có giá. DEMO-101 có hóa đơn tháng 09/2026 tổng 2.030.000đ; DEMO-102 để người dùng tự lập. Giá chỉ là dữ liệu thử. Credential thật ở access.json cạnh DB, latest.txt trỏ demo mới nhất; không ghi mật khẩu vào tài liệu Git.
- Hợp đồng/hồ sơ/role demo được chuẩn bị bằng SQL/CLI chỉ trên bản sao; tòa/phòng/dịch vụ/hóa đơn qua HTTP thật và có audit. Bản sao giữ dữ liệu nguồn nên vẫn có thể chứa dữ liệu local cũ, không chia sẻ DB.
- Xác minh thực tế: build Debug thành công (cảnh báo ImageSharp); cả bốn tài khoản đăng nhập được, ADMIN vào nhật ký, Quản lý bị chặn dịch vụ, Chủ nhà thiết lập giá và phát hành hóa đơn đúng tổng; phát hành trùng bị từ chối; trang chi tiết HTTP 200; check-database, integrity=ok/FK rỗng, SHA-256 DB nguồn trước/sau giống nhau. Chưa nghiệm thu UI đồ họa, SMTP hoặc toàn bộ suite.
- Lần đầu trong sandbox lỗi Data Protection/Event Log, script đã dừng server lỗi; chạy lại ngoài sandbox thành công, giữ bản lỗi riêng không sử dụng. App gốc cổng 5247 không bị dừng. Dùng cổng 5250 và tài khoản Chủ nhà của demo; xem cách chạy lại/đọc credential ở README.

1. Khởi tạo DB và bỏ theo dõi DB đã triển khai trong S2-06; xem hướng dẫn README và bàn giao mới nhất. Đồng đội cần sao lưu ra ngoài repo trước lần pull nhận thay đổi bỏ theo dõi DB.
2. Chốt giá/cách tính dịch vụ, mã phòng, nội dung email reset còn mở; kiểm tra SMTP/UI trên bản sao trước nghiệm thu.
3. Rà soát fixture sau merge, khôi phục/chuyển bộ S1-05 nếu cần. Tích hợp quyền/phạm vi dữ liệu/audit khi triển khai nghiệp vụ mới, không tự làm toàn backlog.
4. Lần này: viết lại tien-do.md; gộp và xóa sáu tài liệu chức năng/demo được thay thế; sửa liên kết README/cap-nhat-csdl.md và mô tả phiên bản updater. Giữ hướng dẫn, yêu cầu, mô hình và SQL.
5. Xác minh lần này: đối chiếu mã startup/updater/auth/email, kiểm tra file test hiện có, liên kết nội bộ và diff tài liệu. Không build/chạy test, không mở/ghi database, không sửa mã ứng dụng, không commit/push.

### Khắc phục xóa tài khoản (30/09/2026)
- Kiểm tra DB local ở chế độ chỉ đọc: schema version 4; `abc@gmail.com` có `is_deleted=1`, nên xóa đã được ghi nhận (soft-delete) và dữ liệu lịch sử vẫn giữ.
- Cập nhật `Views/ManagedAccounts/Index.cshtml` để không hiện nút Xóa lại cho tài khoản đã xóa.
- Cập nhật `AccountController.ConfirmEmail` dùng SQL trực tiếp cho bước xác nhận ẩn danh, tránh yêu cầu actor audit.
- Đã dừng tiến trình cũ và build Debug thành công, 0 lỗi (còn cảnh báo license ImageSharp).
- Chưa thực hiện kiểm thử SMTP thật hoặc thao tác UI sau khi khởi động lại.

Cách chạy lại: `dotnet run --project .\QL_PhongTro\QL_PhongTro.csproj --launch-profile http`, đăng nhập ADMIN rồi tải lại `/ManagedAccounts`. Hành vi hiển thị tài khoản đã xóa trong ghi chú cũ được thay thế bởi bản sửa dưới đây.

### Hoàn thiện xóa và tái sử dụng tài khoản tại ổ D (30/09/2026)

- Làm việc tại `D:\DEV\TTCS_T926_K12C3_N1`, nhánh `Feature/fix/dang-ky-dang-nhap-phan-quyen`. Giữ các thay đổi chưa commit có sẵn, bao gồm DB local và project test; không reset, commit/push hoặc sửa DB nguồn trong quá trình kiểm thử.
- Kiểm tra SQLite nguồn chỉ đọc: v4, hai UNIQUE index `ux_account_email_normalized` trên `lower(trim(email))` và `ux_account_phone` trên số điện thoại, chưa có điều kiện loại tài khoản đã xóa. Có sáu tài khoản đã xóa. FK tham chiếu theo ID từ tòa, hồ sơ, nhật ký, reset/phiên/xác nhận; FK check không lỗi. DB local chưa cài bảng hợp đồng/hóa đơn đầy đủ như mô hình tham chiếu.
- Thêm `Data/AccountReuseSchema.cs`, nâng phiên bản hiện tại lên v5. Web tự chạy bước v4 → v5 sau kiểm tra schema, có backup nhất quán cạnh DB và transaction. Chỉ thay hai index thành UNIQUE `WHERE is_deleted = 0`; không đổi ID/email/số điện thoại cũ, không dựng lại bảng, không xóa dây chuyền. Schema UNIQUE không nhận diện được bị từ chối để tránh làm hỏng ràng buộc. Updater cũng dùng cùng bước này.
- v5 xử lý tài khoản đã xóa từ trước: khóa, thu hồi phiên/refresh/reset, bỏ mã xác nhận và bỏ phân công tòa của Quản lý. Giữ nguyên hồ sơ và nhật ký; không giả lập người thực hiện cho sửa dữ liệu hạ tầng. Các schema trước v4 vẫn cần quy trình nâng cấp một lần như tài liệu CSDL.
- `/ManagedAccounts` loại `IsDeleted` trước đếm/lọc/phân trang. ADMIN bấm Xóa và xác nhận là tài khoản biến mất, email/số điện thoại dùng lại được. Tài khoản chỉ khóa vẫn giữ chỗ thông tin. Xóa mới giữ audit, bỏ phân công và thu hồi phiên trong cùng transaction; mở khóa/gửi lại mật khẩu/xóa lại ID đã xóa trả 404, không phục hồi tài khoản cũ. Vẫn cấm tự xóa/xóa ADMIN, kiểm tra quyền backend và CSRF.
- Đồng bộ kiểm tra trùng ở tự đăng ký và tạo bởi ADMIN; đăng nhập cookie/API, refresh, xác nhận/gửi lại email, đổi/reset mật khẩu, kiểm tra phiên đều loại tài khoản đã xóa. Tài khoản mới nhận ID riêng; token cũ không tác động tài khoản mới. Không áp dụng global query filter lên quan hệ lịch sử. Xử lý hết hạn xác nhận ẩn danh bằng transaction SQL để không lỗi do thiếu actor audit.
- Thêm `verification/account_delete_http.py`: tự tạo bản sao và tài khoản kiểm thử, email pickup, không dùng SQL sửa dữ liệu thật. PASS HTTP xóa/đăng ký lại/tạo lại, xác nhận, đăng nhập email/SĐT, reset đúng ID mới, quyền/CSRF, cookie/JWT/refresh cũ, tài khoản khóa, bỏ phân công Quản lý, rollback khi audit lỗi, UNIQUE SQLite từng trường, lọc/phân trang, backup v4, từ chối UNIQUE lạ không đổi DB, integrity/FK và chạy lại v5. Đối chiếu hồ sơ/nhật ký cũ giữ nguyên; kiểm thử hợp đồng/hóa đơn có điều kiện nếu DB nguồn có các bảng đó, chưa phải nghiệm thu module hợp đồng đầy đủ.
- Xác minh: `dotnet build QL_PhongTro/QL_PhongTro.csproj --no-restore` thành công, 0 lỗi; còn cảnh báo ImageSharp license và CS8601 có sẵn. Bộ HTTP chạy ngoài sandbox do Windows Event Log/Data Protection bị chặn trong sandbox; không phải thuộc tính chỉ đọc của ổ D. SHA-256 DB nguồn trước/sau kiểm thử giống nhau. Chưa chạy toàn bộ suite cũ hoặc nghiệm thu UI đồ họa/SMTP thật. `git diff --check` chỉ còn dòng trống cuối project test từ thay đổi có sẵn, không sửa phần đó.
- Sử dụng: chạy lại app với mã mới như lệnh trên; DB v4 tự nâng lên v5 và tạo backup, không cần người dùng chạy SQL/lệnh sửa dữ liệu mỗi lần xóa. Phiên làm việc này chỉ nâng cấp DB tạm; DB local thật sẽ được xử lý khi app mới khởi động.

### Đăng ký lại khi tài khoản mới đang chờ xác nhận (30/09/2026)

- Đối chiếu ảnh báo trùng với DB thật ở chế độ chỉ đọc: schema đã lên v5 và có đúng hai partial UNIQUE index. Bản ghi cũ ID 12 đã xóa; lần đăng ký sau tạo ID 13, chưa xóa và chưa xác nhận email. Vì vậy lỗi trùng đến từ tài khoản mới đang chờ mã, không phải bản ghi đã xóa giữ chỗ.
- `AccountController.Register` chuyển về trang xác nhận khi cả email và SĐT khớp cùng tài khoản Khách thuê đang hoạt động/chưa xác nhận/chưa xóa. Không tạo thêm ID, đổi mật khẩu hoặc gửi email tự động trong lần thử lại. Chỉ trùng một trường, tài khoản đã xác nhận hoặc bị khóa vẫn được xử lý kiểm tra trùng như cũ.
- Trang xác nhận hiển thị lỗi gửi mã từ lần đăng ký trước (`RegisterError`), thay vì bỏ qua thông báo. Người dùng có thể tiếp tục nhập mã hoặc Gửi lại mã, không cần xóa tài khoản rồi đăng ký liên tục.
- Build `--no-restore -p:UseAppHost=false` PASS, giữ tiến trình web hiện tại để không mất cấu hình SMTP trong terminal của người dùng. Bộ `verification/account_delete_http.py` PASS trên bản sao, bổ sung kiểm tra retry chuyển đúng trang, không đổi ID/hash mật khẩu và không nối nhầm khi chỉ trùng email. Fixture hỗ trợ DB nguồn v5 đã có email/SĐT tái sử dụng. SHA-256 DB nguồn không đổi; không commit/push. Cần khởi động lại app trong cùng terminal cấu hình SMTP để nạp bản sửa.

### Phân biệt gửi email thật, pickup và lỗi gửi mã (30/09/2026)

- Phát hiện `ResendConfirmation` nuốt lỗi gửi thư và luôn hiện thông báo đã gửi. Đã bỏ hành vi này: đăng ký/gửi lại mã báo riêng kết quả pickup, SMTP nhận thư hoặc lỗi cấu hình/SMTP/quá thời gian. Không khẳng định SMTP nhận thư đồng nghĩa đã tới inbox. Trang xác nhận không còn mặc định khẳng định mã đã gửi.
- `IRegistrationEmailSender` trả về chế độ giao thư sau khi gửi thành công. Giới hạn chờ async 15 giây bằng cancellation token; các luồng gửi mật khẩu tạm/reset cũng xử lý lỗi timeout thay vì trả 500.
- Thêm `--check-email-config`: chỉ in chế độ gửi, host/port/TLS và boolean có cấu hình username/password/from; không in thông tin đăng nhập và không gửi email. README bổ sung hướng dẫn Gmail và yêu cầu chạy trong cùng terminal đã đặt biến môi trường.
- Build riêng tại `D:\DEV\.tmp-email-verification` PASS vì app hiện tại khóa DLL đầu ra mặc định; không dừng app hoặc làm mất môi trường SMTP của người dùng. Bộ HTTP trên bản sao PASS, gồm mô phỏng lỗi thư mục gửi, kiểm tra thông báo lỗi không bị thay thành thành công và gửi lại được sau khi khôi phục. Hash DB nguồn không đổi. Chưa kiểm thử Gmail thật hoặc tình huống máy chủ SMTP treo; không đọc được biến môi trường riêng trong terminal chạy app của người dùng.
- Lệnh kiểm tra trong môi trường agent trả PICKUP, SMTP host rỗng, username/password chưa cấu hình; kết quả này không chứng minh cấu hình của tiến trình web hiện đang chạy. Cần người dùng chạy lệnh kiểm tra ngay tại terminal của app và cung cấp đầu ra đã che thông tin đăng nhập sẵn. App phải khởi động lại để nạp mã mới. Không commit/push.

### Chẩn đoán lỗi Windows chặn DLL (30/09/2026)

- Lệnh chạy của người dùng bị `FileLoadException 0x800711C7` trước khi app khởi động. Đã đọc CodeIntegrity/Operational: các sự kiện 3077 lúc 06:26 chỉ đúng `QL_PhongTro.dll`, policy `VerifiedAndReputableDesktop`, GUID `{0283ac0f-fff1-49ae-ada1-8a933130cad6}` (Smart App Control). DLL chưa ký số và không có stream Zone.Identifier. Đây là chặn thực thi của Windows, không phải lỗi SMTP, cảnh báo license ImageSharp hay thuộc tính chỉ đọc ổ D.
- Chỉ chẩn đoán, không thay đổi chính sách bảo mật, không xóa/rebuild DB hoặc tạo lại ADMIN. App chưa chạy nên không thể gửi mã. Cần chủ máy quyết định cấu hình Smart App Control phù hợp môi trường phát triển hoặc dùng bản build ký số được tin cậy; với máy được quản lý cần quản trị viên xử lý. Không khẳng định chạy Administrator hoặc Unblock-File sẽ giải quyết chặn này.

### Ẩn quản lý tòa/phòng khỏi Khách thuê (30/09/2026)

- Menu trước đây lấy quyền READ của module PHONG_TRO để dẫn Khách thuê vào controller quản lý dành cho nhân sự. `PermissionService.MenuAsync` nay loại module này khỏi menu Khách thuê, gồm hai liên kết Tòa nhà/phòng/bảng giá và Quản lý tòa nhà. Giữ các chức năng hồ sơ và module khác; không sửa quyền trong DB hoặc tự triển khai trang phòng đang thuê.
- `PhongTroController` yêu cầu vai trò CHU_NHA/QUAN_LY/ADMIN ở backend, vẫn kiểm tra quyền module và phạm vi dữ liệu hiện có. Khách thuê gõ URL trực tiếp hoặc gửi POST tạo tòa bị từ chối. Không dùng riêng ẩn menu làm kiểm soát quyền.
- Build Debug ở thư mục kiểm thử riêng thành công. Bổ sung HTTP kiểm tra menu Khách thuê, hồ sơ vẫn hiện, GET `/PhongTro` và `/PhongTro/ToaNha` trả 403, POST `/PhongTro/TaoToaNha` trả 403 và ADMIN vẫn GET 200 trên DB bản sao. Cập nhật kỳ vọng tương ứng trong PermissionTests; chưa chạy lại toàn bộ xUnit suite cũ. Không sửa DB thật, không commit/push. Cần khởi động lại app để nạp bản sửa.

### Nút con mắt cho mật khẩu (30/09/2026)

- Thay nút chữ Hiện rộng cả dòng ở Đăng nhập và Đổi mật khẩu bằng SVG con mắt nhỏ nằm bên phải bên trong ô nhập, dùng partial `_PasswordToggle`. Khi hiện mật khẩu, biểu tượng có gạch chéo; giữ thao tác bàn phím, nhãn trợ năng và `aria-pressed`. CSS riêng tránh quy tắc `.auth-card .btn` làm nút chiếm cả dòng.
- Build Debug vào thư mục riêng PASS, không ảnh hưởng tiến trình app đang chạy; chưa kiểm tra trực quan trên trình duyệt. Không sửa DB, chưa commit/push thay đổi giao diện này.
### S2-07 — Danh sách yêu cầu của chủ nhà (02/10/2026)

- Đã thêm bảng `yeu_cau` ở schema v9 với mã yêu cầu, khách, phòng, tòa nhà, loại yêu cầu, ngày mong muốn, trạng thái và thời điểm tạo; không sửa database local.
- Đã thêm `YeuCauController.Index` và `/YeuCau`: lọc theo tòa thuộc chủ nhà, trả đủ thông tin yêu cầu và sắp xếp mới nhất trước; có trạng thái danh sách rỗng.
- Dữ liệu mẫu giả nằm ở `docs/sql/S2-07-yeu-cau.sql`, chỉ dùng trên database demo/bản sao.
- File thay đổi: model, EF mapping, updater v9, controller, view model, Razor View, SQL mẫu, README và tài liệu tiến độ.
- Xác minh: build project chính với output riêng `S207Check` PASS, 0 lỗi; còn cảnh báo ImageSharp license và CS8601 có sẵn. Chưa chạy updater, test HTTP/UI hoặc script dữ liệu mẫu.
- Chưa làm: đánh dấu quá 24 giờ, số lượng chưa xử lý trên menu, tạo/sửa yêu cầu. Bộ lọc được bổ sung tại AC2 bên dưới.

### S2-07 AC2 — Lọc yêu cầu (02/10/2026)

- PO chốt 5 trạng thái: Mới, Đã hẹn lịch, Đã duyệt, Từ chối, Đã huỷ. `/YeuCau` nhận `trangThai` và `toaNhaId`, lấy danh sách tòa đang hoạt động của chủ nhà, hỗ trợ lọc riêng hoặc kết hợp; nút bỏ bộ lọc đưa về danh sách rộng hơn.
- Thứ tự `ngay_tao DESC, id DESC` được giữ sau mọi bộ lọc. Không triển khai cảnh báo quá 24 giờ hoặc số lượng chưa xử lý trên menu.
- Xác minh cần thực hiện: build, test từng trạng thái/tòa nhà/kết hợp/bỏ lọc/không có kết quả/thứ tự. Chưa chạy kiểm thử trong lượt này.

### S2-07 AC3 — Nhận biết yêu cầu chưa xử lý quá 24 giờ (02/10/2026)

- Giả định PO cần xác nhận: `Mới` và `Đã hẹn lịch` là chưa xử lý; `Đã duyệt`, `Từ chối`, `Đã huỷ` là đã có kết quả. Mốc bắt đầu tính là `ngay_tao`; đúng 24 giờ đã được đánh dấu, tính theo UTC.
- Controller trả thêm `QuaHanChuaXuLy`; view giữ nguyên toàn bộ thông tin và tô nổi bật dòng, kèm nhãn “Quá 24 giờ, chưa xử lý”. Vì tính trên từng dòng sau truy vấn, dấu hiệu vẫn đúng khi lọc trạng thái hoặc tòa nhà.
- Chưa có số lượng yêu cầu chưa xử lý trên menu. Build/test thực tế cần thực hiện trước nghiệm thu; database local không bị ghi.
## Local email pickup file names (05/10/2026)

- Cap nhat che do email pickup local: khi chua dung SMTP, app tu ghi file `.txt` de doc voi tien to danh so `0001-...`, chua gui vao inbox. Noi dung file co thoi gian, nguoi gui, nguoi nhan, tieu de va body co ma xac nhan.
- Trang `/Account/ConfirmEmail` hien them thu muc pickup va file moi nhat sau khi dang ky/gui lai ma. `--check-email-config` in duong dan pickup tuyet doi; `run.bat` in `Email pickup local=...` khi cau hinh pickup den tu `.env.local`/bien moi truong.
- File thay doi: `QL_PhongTro/Services/PasswordEmailSender.cs`, `QL_PhongTro/Controllers/AccountController.cs`, `QL_PhongTro/Views/Account/ConfirmEmail.cshtml`, `QL_PhongTro/Program.cs`, `run.bat`.
- Xac minh thuc te: `dotnet build .\QL_PhongTro\QL_PhongTro.csproj --no-restore` PASS; `dotnet run --project .\QL_PhongTro\QL_PhongTro.csproj --no-build --launch-profile http -- --check-email-config` PASS va in `Pickup directory: C:\Users\cter4\AppData\Local\Temp\s105-mail-preview`. Con can restart app va gui thu ma tren UI de thay file pickup moi sinh. Khong doi schema/database, khong ghi SMTP secret.
