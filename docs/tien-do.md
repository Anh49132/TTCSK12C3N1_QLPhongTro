# Tiến độ dự án

## Tinh gọn cấu hình dự án (28/09/2026)

- Loại bỏ môi trường staging thử nghiệm gồm Docker/Compose, reset-seed, trang tổng quan và liên kết giao diện; đây không phải phần triển khai Nhật ký hoạt động S1-10 theo backlog.
- Giữ nguyên sửa lỗi phân quyền MVC trả 403, đăng nhập, cookie/JWT, cập nhật CSDL và các chức năng nghiệp vụ đang có. Không sửa SQLite, backup, dữ liệu demo hoặc project test local.
- Build Debug thành công (0 lỗi, 3 cảnh báo cũ); `--check-database` báo schema sẵn sàng. Chạy local và xác minh GET `/`, `/Account/Login` đều HTTP 200 rồi dừng server sạch. Build Release vẫn cần license ImageSharp hợp lệ.

## Dọn môi trường và xác minh trước báo cáo (28/09/2026)

- Dừng server và dọn các cache/build bị Git ignore: `.vs`, `bin`, `obj` của app, S109 và project test. Giữ nguyên SQLite, backup, dữ liệu demo và `App_Data`; các output cần thiết đã được tạo lại khi build.
- Loại thông tin đăng nhập cụ thể không khớp CSDL khỏi README, giữ biến môi trường ở dạng mẫu. README bổ sung cách tránh server cũ khóa file và dùng build Debug cho chạy local.
- Restore và build Debug thành công (0 lỗi, còn cảnh báo license ImageSharp và 2 cảnh báo nullable cũ); `--check-database` báo schema sẵn sàng. Chạy app Development tại `http://localhost:5247`, GET `/` và `/Account/Login` đều HTTP 200, rồi dừng server sạch.
- Build Release chưa đạt: ImageSharp 4.1.2 yêu cầu license hợp lệ và dừng build Release. Chưa thay package hoặc bỏ kiểm tra license; Docker dùng Release nên cần cấu hình license qua secret trước khi dùng. Không đưa SQLite và thay đổi project test local vào commit.

## Hướng dẫn tránh khóa file khi chạy local (28/09/2026)

- Cập nhật `README.md` để dừng tiến trình `QL_PhongTro` cũ trước khi restore/build/update DB, giữ đúng terminal có dòng `Now listening` và dùng Ctrl+C tại terminal đó.
- Bổ sung cách xử lý lỗi `MSB3021`/`MSB3027` do `QL_PhongTro.exe` đang bị server cũ khóa; phân biệt với lỗi thiếu bảng SQLite và dẫn lại quy trình cập nhật CSDL.
- Chỉ sửa tài liệu; không thay đổi mã nguồn hoặc schema. Đã kiểm tra diff và nội dung lệnh PowerShell, không chạy lại build vì thay đổi không ảnh hưởng ứng dụng.

## Sửa startup sau merge S1-03/S1-09 (28/09/2026)

- Nguyên nhân app không chạy: `DatabaseUpdates.Check` duyệt mọi entity EF nên yêu cầu cả schema S1-09 dù S1-09 được bàn giao là module tùy chọn chỉ dùng DB kiểm thử. `--update-database`, tạo ADMIN và startup đều dừng khi DB local thiếu dịch vụ/hợp đồng/hóa đơn.
- Sửa check theo nhóm: schema lõi luôn bắt buộc; hợp đồng, dịch vụ và hóa đơn chỉ được kiểm tra khi nhóm tương ứng đã bắt đầu tồn tại. Nếu hóa đơn tồn tại thì kiểm tra cả hợp đồng + dịch vụ. Schema tùy chọn tồn tại một phần vẫn bị chặn; DB không có S1-09 được phép chạy và console báo module chưa cài.
- Không tạo bảng S1-09 hoặc sửa dữ liệu nghiệp vụ. CSDL local hiện integrity=ok, 0 lỗi FK, schema S1-03 v2; backup từ lần update lỗi vẫn được giữ. Cần dùng fixture theo `s1-09-dich-vu.md` nếu muốn demo module dịch vụ/hóa đơn.

## Gộp S1-03 vào dev (28/09/2026)

- Gộp commit S1-03 `730c063` vào `dev` mới nhất sau S1-09 (`97d01d2`); giữ bàn giao và ignore của cả hai nhánh. Xung đột chỉ ở `.gitignore` và tài liệu tiến độ.
- Build và kết quả merge được xác minh trong worktree riêng để không ảnh hưởng SQLite và project test đang sửa cục bộ ở checkout S1-03. Không đưa CSDL, backup hoặc credential vào merge.

## Gộp S1-09 vào dev (28/09/2026)

- Gộp commit S1-09 `7216a8b` vào dev sau `a45b14b`; giữ phần staging S1-10, xử lý xung đột tài liệu bằng cách giữ bàn giao cả hai nhánh.
- Đã restore và build bản gộp: 0 lỗi, còn cảnh báo ImageSharp và CS8601 cũ. Chưa chạy lại kiểm thử runtime trên bản gộp; bước tiếp theo là demo S1-09 trên DB kiểm thử theo hướng dẫn. Schema staging vẫn là fixture riêng, chưa xác minh tương thích với nghiệp vụ hóa đơn S1-09.
- Không chạy ứng dụng hoặc script DB trong lần merge; không đưa thay đổi local project test, mật khẩu hay DB demo vào commit.

## Tài khoản ADMIN local và README (28/09/2026)

- Đã tạo tài khoản ADMIN phát triển do người dùng yêu cầu trong `QL_PhongTro/Data/local-dev.sqlite` và `data/s103-demo-ready.sqlite`; mật khẩu được băm BCrypt, tài khoản hoạt động và không bị buộc đổi mật khẩu lần đầu. Thông tin rõ chỉ còn trong CSDL local, không đưa lên Git.
- Trước khi ghi, đã kiểm tra email chưa tồn tại và tạo backup cạnh từng CSDL với hậu tố `.before-admin-<thời gian>.bak`. Đối chiếu xác nhận các tài khoản cũ giữ nguyên, BCrypt xác minh đúng mật khẩu và `integrity_check=ok` trên cả hai CSDL.
- Cập nhật `README.md`: thêm bước nâng cấp schema trước khi chạy, sửa mô tả CSDL không còn ghi tự tạo, ghi thông tin ADMIN local và liên kết tài liệu. Thông tin đăng nhập này chỉ dành cho phát triển/kiểm thử, cần đổi hoặc loại bỏ trước triển khai thật.
- Đã đăng nhập thực tế bằng ADMIN local trên bản demo đang chạy: chuyển về trang chủ với HTTP 200; truy cập `/ManagedAccounts` và `/Permissions` đều HTTP 200. Thêm ignore cho backup `*.sqlite.before-admin-*.bak*` để tránh đưa bản sao CSDL lên Git.
- Thêm lệnh Development `--create-local-admin` nhận email/mật khẩu/số điện thoại qua `LocalAdmin__Email`, `LocalAdmin__Password`, `LocalAdmin__Phone`; không còn credential cố định trong code/tài liệu. Lệnh kiểm tra schema v2, không ghi đè email đã có, từ chối xung đột số điện thoại và sao lưu trước khi tạo.

## Thứ tự menu (28/09/2026)

- Sửa `Views/Shared/_Layout.cshtml`: chuyển nhóm Hệ thống xuống cuối sidebar, giữ thứ tự các nhóm còn lại. Không đổi CSDL hay quyền truy cập.
- Khởi động lại app để Razor biên dịch lại, mở trang có menu để xem. Kiểm tra diff; chưa xác minh bằng trình duyệt.

## S1-03 – Tạo, bắt buộc đổi mật khẩu, khoá và danh sách (28/09/2026)

- Đã triển khai ba lát AC1–AC4: ADMIN tạo Chủ nhà/Quản lý, email mật khẩu tạm, trạng thái chờ đổi; chặn chức năng khác tới khi đổi xong; khoá/mở khoá thu hồi cookie/JWT/refresh; lọc vai trò/trạng thái và 20 dòng/trang. PO đã đồng ý ADMIN-only/phạm vi hai vai trò, mật khẩu tạm 16 ký tự đủ 4 nhóm, mật khẩu mới >=8 có chữ/số và email văn bản theo mẫu. Không tự khoá.
- Thành phần: `ManagedAccountsController`, ViewModels/Razor, email sender dùng chung SMTP/pickup, middleware bắt buộc đổi, AccountController/ChangePassword, cookie/JWT/AuthService, layout + polling 20 giây. Gửi email lỗi vẫn giữ tài khoản chờ đổi, có gửi lại và thay mật khẩu tạm cũ; không lưu mật khẩu rõ trong DB. Đổi mật khẩu nay dùng được cho mọi vai trò và thu hồi mọi phiên.
- Schema v2 trong `DatabaseUpdates`: cột must_change_password + unique index email chuẩn hoá/điện thoại; backup trước, không gộp/xoá dữ liệu trùng. Chỉ áp dụng vào bản sao thử/demo; CSDL chính giữ nguyên v1 trong task này. Startup báo cần updater trước khi chạy bản code mới với DB v1.
- Build PASS; `verification/s103_http.py` PASS HTTP tạo/trùng/validation/CSRF/email pickup/lỗi gửi và gửi lại/ép đổi/chặn MVC+API/login mới/khoá Chủ nhà+Quản lý/cookie-JWT-refresh-unlock/lọc+3 trang/integrity/FK. `verification/database_updates.py` PASS hồi quy nâng cấp và bảo toàn dữ liệu. Test đối chiếu SHA-256 DB gốc không đổi. Còn warning ImageSharp + 2 CS8601 cũ; không chạy bộ test cũ có EnsureDeleted.
- Demo đang chạy localhost:5247 với `data/s103-demo-ready.sqlite`, ADMIN ngẫu nhiên và 45 Quản lý hoạt động + 8 khoá thêm vào bản sao. Script `verification/prepare_s103_demo.py` tạo bản sao mới và in thông tin đăng nhập; không tự cấp ADMIN vào DB thật. Lần tạo demo đầu đã xong dữ liệu nhưng lỗi in đường dẫn Unicode; đã sửa encoding, chạy lại với file đích mới thành công.
- Hướng dẫn chạy và từng bước test: [s1-03-tai-khoan.md](s1-03-tai-khoan.md). Mail demo ở `%TEMP%/s103-mail-preview`, chưa gửi tới inbox. Chưa xác minh SMTP thật/UI đồ hoạ hoặc đo polling trong trình duyệt; HTTP đã xác minh khoá có hiệu lực ngay. Cần SMTP và kiểm thử demo theo tài liệu trước nghiệm thu vận hành.
- Giới hạn: gửi SMTP không cùng transaction với DB; retry thay mật khẩu tạm, có thể cần kiểm tra thư mới nhất. Tab bị treo/mất mạng không tự chuyển đúng thời gian, server vẫn chặn mọi yêu cầu. Không commit/push; giữ nguyên thay đổi test project/DB/backup từ trước. Ghi chú auth/schema startup ở các mục cũ bên dưới đã được thay thế bởi updater hiện tại.

## Đồng bộ schema và sửa lỗi app_module (28/09/2026)

- Theo yêu cầu sửa quy trình CSDL: thêm `Data/DatabaseUpdates.cs`, lệnh `--update-database` (backup + phiên bản 1) và `--check-database`; sửa `Program.cs` để web chỉ kiểm tra schema, bỏ tự chạy auth/room/EnsureCreated. Bổ sung bảng hồ sơ theo SQL đã có; không triển khai backlog hay toàn bộ 22 bảng.
- Thêm `verification/database_updates.py`, `docs/cap-nhat-csdl.md`; cập nhật hướng dẫn và ignore backup/lock. Quy trình team và cách chạy: [cap-nhat-csdl.md](cap-nhat-csdl.md). Sau pull: dừng app, `dotnet run --project QL_PhongTro -- --update-database`, rồi chạy profile `http`.
- Đã build thành công; chạy script kiểm chứng trên bản sao PASS: thiếu schema không ghi DB, backup, giữ dữ liệu cũ, integrity/FK, cập nhật lặp lại không đổi quyền đã thu hồi, từ chối phiên bản tương lai/file chưa tồn tại. Lần thử đầu script lỗi dọn file tạm do connection Python chưa đóng; đã sửa và chạy lại đạt. Không chạy bộ test nghiệp vụ cũ.
- Đã áp dụng updater và check vào DB local sau khi người dùng yêu cầu sửa. Backup toàn bộ: `QL_PhongTro/Data/local-dev.sqlite.before-update-d29bcc86691b4f7aa043da213bcfb6ef.bak`. Tại thời điểm cập nhật, dữ liệu phân quyền đã tồn tại nên initializer giữ nguyên, không seed lại. Đối chiếu tất cả bảng cũ với backup: dữ liệu giữ nguyên, integrity=ok, 0 lỗi FK.
- Web đang chạy `http://localhost:5247`; HTTP GET `/`, `/Account/Login`, `/Account/Register` đều 200, không còn lỗi app_module. Phiên sandbox lỗi DPAPI nên đã dừng và chạy lại dưới tài khoản Windows bình thường. Chưa xác minh đăng nhập bằng tài khoản thật hoặc toàn bộ nghiệp vụ/UI trình duyệt.
- Build còn cảnh báo ImageSharp; compile đầy đủ cũng có 2 CS8601 cũ. Diff của các file task không lỗi khoảng trắng; project test có lỗi whitespace từ trước, giữ nguyên. Không commit/push; file SQLite vốn được Git theo dõi vẫn hiện modified, không đưa dữ liệu/backup vào PR.
- Giới hạn: cập nhật hiện tại gồm nhiều bước idempotent, chưa có transaction chung; thất bại giữa chừng không ghi phiên bản, cần xem lỗi/backup rồi chạy lại. Check kiểm tra bảng/cột và dữ liệu quyền cơ bản, chưa so sánh toàn bộ constraint/index. Sprint sau thêm bước phiên bản mới và kiểm thử từ DB cũ; không sửa bước đã phát hành.

## S4-10 staging và nghiệm thu (28/09/2026)

- Thêm staging Compose riêng, volume SQLite riêng, reset/seed có xác nhận Staging + `AllowReset`, chặn DB local, trang admin tổng hợp, và tài khoản mẫu bốn vai trò. Hướng dẫn: [staging.md](staging.md).
- Seed tạo 2 tòa, 30 phòng, 20 hợp đồng, 60 hóa đơn/3 kỳ; 20 trả đủ, 20 trả một phần còn hạn, 20 quá hạn chưa trả. Thử trên SQLite tạm hai lần: 4.37 giây lần tạo schema đầu, 0.80 giây lần reset sau; integration test xác nhận dữ liệu cũ bị xóa và dashboard/login mẫu hoạt động.
- Sửa cookie authorization để route MVC trái quyền trả trực tiếp 403; cập nhật test và fixture auth. Cả lớp `PermissionTests`: 8/8 pass (7 test quyền + 1 staging integration).
- Chưa chạy Docker Compose do máy hiện tại không có Docker CLI. Chưa có hợp đồng/hóa đơn/thanh toán nghiệp vụ trong app; schema staging chỉ phục vụ dữ liệu tổng hợp/dashboard, không phải migration production. Chưa xác minh demo end-to-end hoặc quyền từng chức năng cho module placeholder.
- Không đọc/ghi `local-dev.sqlite`; các thay đổi DB/test/backup có sẵn của người dùng được giữ nguyên. Hai cảnh báo CS8601 ở AuthController và cảnh báo giấy phép ImageSharp vẫn còn.
## Đang thực hiện — S1-09 AC1–AC4 (28/09/2026)

- Đã viết năm dịch vụ mặc định chống khởi tạo trùng, sửa giá ban đầu, lịch sử giá/ngày hiệu lực, ngừng/kích hoạt lại, xóa có kiểm tra tham chiếu và giao diện tương ứng. Thêm hóa đơn tối thiểu cho kỳ thuê trọn tháng, lưu snapshot dòng hóa đơn và khóa sau phát hành; chưa làm toàn bộ backlog tài chính.
- Người dùng chốt: trùng ngày từ chối, hóa đơn giữ snapshot, ngừng/kích hoạt từ kỳ sau; **chỉ mã/script và DB kiểm thử, không ghi DB gốc**. Giá mặc định và bảng đơn vị/cách tính chưa chốt; mặc định hiển thị “Chưa thiết lập”, chưa cho lập hóa đơn với giá chưa chốt.
- Build app/bộ kiểm thử thành công, còn cảnh báo ImageSharp/CS8601 cũ. Console suite trong môi trường agent vẫn bị Application Control chặn; người dùng đã chạy app thành công. Đã xác minh HTTP thực tế các luồng mặc định/giá đầu vào/hóa đơn snapshot/giá hiệu lực/chặn xóa/ngừng và kích hoạt; chưa chạy toàn bộ suite, concurrency hoặc trình duyệt đồ họa, chưa nghiệm thu toàn bộ AC.
- DB gốc vẫn chỉ có schema nền đã kiểm tra; giữ nguyên SHA-256, không áp dụng initializer hoặc chạy app vào DB gốc. Mã kiểm thử chuẩn bị fixture riêng khi môi trường cho chạy.
- Thành phần, cách chạy, giả định/giới hạn và bước tiếp theo: [s1-09-dich-vu.md](s1-09-dich-vu.md). Bước kế tiếp: chốt giá, xử lý quyền chạy assembly, kiểm thử/demo trên bản sao; giữ nguyên thay đổi local project test từ trước.
- DB demo đang dùng: `data/S1-09-verification/20260928102752-c3ffa2/test.sqlite`, app `http://localhost:5247`. Đã tạo năm giá thử, hóa đơn tháng 9 = 2.030.000đ, giá điện tháng 10 = 3.500đ, tham chiếu hợp đồng của Nước, dịch vụ thử xóa và lịch trạng thái. Hướng dẫn: [s1-09-demo.md](s1-09-demo.md). Chưa tạo hóa đơn tháng 10 để người dùng thử.

## Gộp S1-06 vào dev (28/09/2026)

- Giữ đăng nhập cookie/JWT, phân quyền, mật khẩu, khởi tạo auth/phòng và giao diện hiện tại của dev; bổ sung model/service hồ sơ, ImageSharp và hai liên kết Hồ sơ cá nhân/Xem hồ sơ.
- Build bản gộp thành công. Còn cảnh báo ImageSharp chưa cấu hình giấy phép và hai cảnh báo CS8601 tại AuthController.
- Chạy riêng PermissionTests: 7 ca lỗi ở bước chuẩn bị do CSDL mẫu trong Git thiếu bảng toa_nha; chưa xác minh hồi quy phân quyền. Không chạy AuthTests vì fixture gọi EnsureDeleted trên DB cấu hình mặc định.
- Các script S1-06 bên dưới được viết trước luồng đăng nhập/CSRF của dev; cần cập nhật fixture đăng ký và kỳ vọng chuyển hướng trước khi chạy lại trên bản gộp.
- Các ghi chú S1-06 bên dưới là lịch sử nhánh. Bản gộp đã có đăng nhập và khởi tạo auth/phòng ở startup. Schema hồ sơ/quan hệ thuê vẫn cần áp dụng riêng sau kiểm tra DB và sao lưu; không chạy nguyên SQL tạo toa_nha/phong_tro nếu các bảng này đã tồn tại.
- Không đưa dữ liệu SQLite local vào bản gộp.

## Cập nhật gần nhất — S1-05 (28/09/2026)

- Đã triển khai lần lượt cả 4 phần: đổi mật khẩu; email reset 30 phút/dùng một lần; giới hạn 3 yêu cầu/email/giờ; vô hiệu hóa cookie/JWT/refresh token sau reset.
- Bộ xác minh riêng tại `verification/S105` đã pass qua từng phần và toàn luồng trên SQLite bản sao. Email kiểm thử là `.eml` cục bộ; chưa xác minh SMTP đến hộp thư thật hoặc UI bằng trình duyệt đồ họa.
- Đã thêm ba bảng `password_reset_token`, `password_reset_request`, `account_session_version` và index giới hạn vào DB hiện có qua lệnh riêng có backup. Không đổi schema/nội dung các bảng cũ; đối chiếu backup đạt, integrity_check=ok.
- Backup: `QL_PhongTro/Data/local-dev.sqlite.before-s105-20260927234116421.bak`.
- Thành phần: partial AccountController cho password; ViewModels/Razor; PasswordResetService, PasswordEmailSender, SessionVersionStore, PasswordSchemaInitializer; sửa tối thiểu Program/AuthService/TokenService/AppCookieEvents để thu hồi mọi loại phiên. Không đổi ma trận quyền.
- Build chính thành công. Rebuild cuối có 2 warning CS8601 cũ ở AuthController, 0 error. Restore đầu gặp NU1900, chưa xác minh audit dependency.
- Không chạy test suite cũ có EnsureDeleted. Không sửa/thêm file trong Tests/tests; không stage, commit, push. Git vẫn có thay đổi project test và backup before-auth từ trước; giữ nguyên.
- Hướng dẫn build/chạy/demo, cấu hình SMTP, danh sách file và các xác minh chi tiết: [s1-05-mat-khau.md](s1-05-mat-khau.md).

## Giả định / việc tiếp theo

- Nội dung email/thông báo tối thiểu chưa có xác nhận PO. Gửi lỗi vẫn tính một yêu cầu; email không tồn tại cũng được đếm, trả thông báo trung tính.
- Cần cấu hình `PasswordReset` (PublicBaseUrl, Host, Port, EnableSsl, From, Username, Password), xác minh nhận email thật và demo hai trình duyệt trước khi chốt nghiệm thu end-to-end.
- DB khác phải chạy `--initialize-password-security` trước khi dùng code mới; initializer có backup, không tự chạy ở startup.
- Giữ nguyên initializer auth/phòng có sẵn ở startup; đây là khác biệt với ghi chú S1-04 cũ. Không tự chạy app/test trên DB thật để thử nghiệp vụ.
- Chưa xử lý collision Git Tests/tests, chưa thêm job dọn lịch sử reset.

## Nền tảng đã có từ task trước

- S1-04: 4 vai trò/9 module, guard backend và menu theo DB; QUAN_LY không truy cập tài chính. Chi tiết: [s1-04-phan-quyen.md](s1-04-phan-quyen.md). Tài liệu này có mô tả auth lịch sử; mã nguồn hiện đã có cả cookie MVC và API JWT.
- S1-07/S1-08: quản lý tòa/phòng, tạo đơn/tạo lô, lọc trạng thái; giá phòng tối thiểu 500.000 ở ứng dụng. Quy tắc mã tầng + 2 chữ số, giới hạn 1..99 còn cần PO xác nhận.
- Các module placeholder chưa triển khai nghiệp vụ; không tự làm backlog. Đăng ký vẫn chỉ tạo KHACH_THUE.
## Bàn giao nhánh GitHub

- Nhánh `feature/S1-06-ho-so-ca-nhan` chứa mã nguồn, SQL và kiểm thử; thay đổi tệp SQLite chỉ giữ local, không đưa vào commit S1-06.
- Với checkout mới có CSDL nền chỉ gồm `tai_khoan`: kiểm tra schema, sao lưu và duyệt trước khi áp dụng lần lượt `docs/sql/S1-06-khach-thue.sql`, `docs/sql/S1-06-quan-he-thue.sql`. Không chạy lại trên DB đã có các bảng; ứng dụng không tự tạo schema.

## Kiểm thử lại theo yêu cầu (27/09/2026)

- Chạy lại `dotnet build --no-restore`: thành công, 0 lỗi, 1 cảnh báo giấy phép ImageSharp.
- Chạy đủ `tests/s1_06_http.py`, `tests/s1_06_images.py`, `tests/s1_06_permissions.py` (có kiểm tra quyền truy cập ảnh): cả 3 PASS, exit code 0.
- Môi trường riêng: `data/S1-06-images/retest-20260927/test.sqlite`, ảnh trong thư mục `images` bên cạnh. `PRAGMA integrity_check` trả `ok`, `foreign_key_check` không có lỗi. SHA-256 CSDL chính trước/sau giống nhau.
- `git diff --check` không có lỗi khoảng trắng. Lần này kiểm thử tích hợp HTTP, không chạy lại thao tác trình duyệt, không chụp ảnh. Không phát hiện lỗi trong các kịch bản đã chạy; không sửa mã ứng dụng. Đã dừng ứng dụng thử sau kiểm tra.

## Mới nhất — S1-06 phần 3 (27/09/2026)

- Hoàn thành che căn cước ở server cho trang xem `/HoSo/Xem?id=<mã hồ sơ>` và trang sửa `/HoSo`. PO đã chốt giữ đúng độ dài: 9 số → `*****5678`, 12 số → `********5678`.
- Đọc vai trò và trạng thái hoạt động từ CSDL mỗi request, không lấy quyền từ query/form hoặc chỉ dựa cookie cũ. `ADMIN` xem đầy đủ. `CHU_NHA` chỉ xem đầy đủ nếu là chủ tòa chứa phòng khách đang thuê; các vai trò/trường hợp khác nhận chuỗi đã che. Người chưa đăng nhập hoặc bị khóa bị chặn.
- Quan hệ đang thuê: hợp đồng `DANG_HIEU_LUC`, có kỳ bao gồm ngày hiện tại ở Việt Nam, chưa trả phòng trước ngày hiện tại; người đứng tên hoặc người ở ghép đã vào và chưa chuyển đi. Ngày bắt đầu/kết thúc kỳ tính bao gồm hai đầu; người ở ghép hết quyền từ ngày chuyển đi. Thiếu schema/bằng chứng quan hệ thì trả dạng che.
- Người dùng đã duyệt bổ sung 5 bảng; đã sao lưu `data/backups/before-S1-06-permissions-20260927-214322.sqlite`, áp dụng `docs/sql/S1-06-quan-he-thue.sql` tạo `toa_nha`, `phong_tro`, `hop_dong`, `ky_hop_dong`, `nguoi_o_ghep`. Tài khoản/hồ sơ cũ giữ nguyên; không thêm dữ liệu demo vào CSDL chính; không tự chạy schema khi khởi động.

## Hành vi giao diện và dữ liệu

- Menu **Xem hồ sơ**, nhập mã hồ sơ. Người đã đăng nhập có thể xem tên và căn cước đã phân quyền của mã đó; người không liên quan không nhận ngày sinh, quê quán, nghề nghiệp hay ảnh. Chủ hồ sơ/admin/chủ nhà đúng quan hệ xem được các thông tin này.
- Khách thuê vẫn sửa thông tin và ảnh của chính mình. Ô căn cước không điền sẵn số đầy đủ; số đã lưu hiển thị che. Để trống giữ nguyên, nhập số 9/12 chữ số để thay mới; hồ sơ mới vẫn bắt buộc căn cước. Form lỗi không phản chiếu số đầy đủ qua ModelState/HTML, cần nhập lại số mới nếu muốn đổi.
- Ảnh trước/sau vẫn tải JPG/PNG, tối đa 5MiB từng ảnh, thu nhỏ chiều rộng tối đa 1600px, giữ tỷ lệ. Ảnh ngoài `wwwroot`, endpoint kiểm tra quyền và no-store. Chủ hồ sơ giữ quyền xem ảnh gốc từ phần 2; admin/chủ nhà đúng quan hệ được xem ảnh của hồ sơ, người khác nhận 404. Che ở đây áp dụng trường số căn cước, không sửa nội dung ảnh giấy tờ gốc.

## Thành phần thay đổi phần 3

- Thêm `Services/HoSoAccess.cs`, `ViewModels/XemHoSoViewModel.cs`, `Views/HoSo/Xem.cshtml`, SQL quan hệ thuê và `tests/s1_06_permissions.py`.
- Cập nhật `HoSoController`, `HoSoViewModel`, `Views/HoSo/Index.cshtml`, menu `_Layout.cshtml`, đăng ký service trong `Program.cs`; điều chỉnh test phần 1 theo hành vi che/để trống giữ nguyên.
- SQL là phần dữ liệu cần cho xác định quyền, chưa là module quản lý hợp đồng đầy đủ; chưa có liên kết `yeu_cau_thue_id` vì module yêu cầu thuê chưa tồn tại. Khi triển khai hợp đồng, đối chiếu DBML và schema thực tế trước khi bổ sung.

## Chạy và demo

- `dotnet build QL_PhongTro/QL_PhongTro.csproj --source https://api.nuget.org/v3/index.json`
- `dotnet run --project QL_PhongTro/QL_PhongTro.csproj`; mở `http://localhost:5247/Account/Register`, đăng ký và khai báo hồ sơ ở `/HoSo`; chọn **Xem hồ sơ đã lưu** để thấy mã hồ sơ và số đã che. Menu **Xem hồ sơ** cho phép nhập mã.
- DB mặc định `QL_PhongTro/Data/local-dev.sqlite`; override bằng `DatabasePath`. Ảnh mặc định `QL_PhongTro/App_Data/identity-images`, override `IdentityImagePath` phải là thư mục riêng tư ngoài `wwwroot`. Sao lưu cả DB và thư mục ảnh.
- Demo đủ vai trò bằng script tích hợp trên bản sao DB: khởi động app với `DatabasePath` trỏ bản sao và `IdentityImagePath` trỏ thư mục ảnh thử, rồi `python tests/s1_06_permissions.py http://localhost:5247 data/S1-06-permissions.sqlite data/S1-06-images`. Script dùng đăng ký để tạo các phiên, gán vai trò và quan hệ thuê trực tiếp trong DB thử. Không dùng DB thật. Chưa có giao diện đăng nhập/cấp vai trò riêng trong dự án; task này không triển khai S1-02/S1-03.
- Hồi quy: `python tests/s1_06_http.py http://localhost:5247 data/S1-06-permissions.sqlite`; `python tests/s1_06_images.py http://localhost:5247 data/S1-06-permissions.sqlite data/S1-06-images` (ảnh cần Pillow).

## Xác minh thực tế phần 3

- Build thành công, 0 lỗi; vẫn có cảnh báo giấy phép ImageSharp 4.1.2 từ phần 2. `git diff --check` không có lỗi khoảng trắng.
- HTTP PASS với cả 9/12 chữ số: chủ hồ sơ, khách khác, quản lý, chủ nhà sai quan hệ nhận chuỗi che; admin/chủ nhà đúng quan hệ nhận đầy đủ. Kiểm tra toàn bộ HTML không có số đầy đủ ở trang xem/sửa và form lỗi của người không đủ quyền; query giả role không nâng quyền.
- PASS hợp đồng nháp/chờ/kết thúc/hủy, kỳ chưa bắt đầu/hết hạn, ngày biên kỳ, trả phòng sớm, chuyển chủ nhà, người ở ghép/chuyển đi, thiếu bảng hợp đồng; đổi vai trò và khóa tài khoản có hiệu lực ngay. Ảnh thật trong thư mục thử: chủ hồ sơ/admin/chủ nhà đúng được đọc, người khác bị từ chối.
- Hai script hồi quy hồ sơ và ảnh đều PASS; lưu mới/cập nhật/giữ căn cước khi để trống, upload/resize/thay ảnh vẫn hoạt động.
- Trình duyệt: khách lưu hồ sơ thấy dạng che ở trang sửa và trang xem; đổi vai trò trong DB thử sang ADMIN, tải lại cùng hồ sơ thấy số đầy đủ. Không chụp ảnh màn hình.
- Test chỉ ghi `data/S1-06-permissions.sqlite` và thư mục ảnh thử; dữ liệu DB chính được đối chiếu giữ nguyên sau bổ sung schema.

## Giới hạn còn lại ngoài phạm vi lát này

- Dữ liệu căn cước/ảnh chưa mã hóa trên đĩa. Cần cấu hình giấy phép Six Labors trước triển khai. Chưa kiểm thử crash hoặc nhiều cập nhật đồng thời; có thể cần dọn tệp ảnh không được tham chiếu sau crash.
- Ảnh cho phép bổ sung từng mặt để tương thích hồ sơ cũ. Quê quán dùng `dia_chi_thuong_tru`; họ tên hồ sơ độc lập tên tài khoản. Giới hạn ảnh 50 megapixel, frame đầu, request tổng 16MiB giữ từ phần 2.
- Bước tiếp theo: triển khai đăng nhập/cấp vai trò và luồng hợp đồng có ghi dữ liệu thật theo task riêng; không thêm cơ chế chọn vai trò hoặc giả quan hệ thuê vào ứng dụng để demo.
