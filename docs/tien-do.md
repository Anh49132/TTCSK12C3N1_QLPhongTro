# Tiến độ và bàn giao dự án

Cập nhật: 29/09/2026. Tổng hợp tài liệu S1-03, S1-04, S1-05, S1-09 (chức năng/demo), S1-10 và tiến độ cũ. Kết quả kiểm thử từ các phiên trước được ghi riêng, không phải lần chạy mới khi biên tập tài liệu.

## Trạng thái hiện tại

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
- `Program.cs` yêu cầu SQLite đã tồn tại; web chỉ kiểm tra schema, không tự tạo/seed/nâng cấp. `DatabaseUpdates` hiện là **v3**. Hợp đồng/dịch vụ/hóa đơn là các nhóm schema tùy chọn; khi đã có bảng liên quan thì phải đủ schema tương ứng.
- Staging/Docker, reset-seed và dashboard thử nghiệm đã gỡ. Các ghi chú staging cũ không còn áp dụng.
- Theo bàn giao local ngày 29/09: 6 tài khoản, 1 hồ sơ, 1 tòa, 4 phòng, 8 dòng nhật ký; chưa cài S1-09. Backup trước demo: `data/backups/local-dev.before-small-demo-20260929-074854.sqlite`. Đây là dữ liệu riêng trên máy, không bảo đảm có trên máy đồng đội; lần biên tập này không mở DB để kiểm tra lại.
- DB local vẫn được Git theo dõi. Quy trình tạo DB khi thiếu và bỏ theo dõi DB mới được thảo luận trong prompt bàn giao, **chưa triển khai vào mã/Git**; không giả định clone mới tự tạo được database.

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

1. Nếu được giao, triển khai khởi tạo DB local khi thiếu và bỏ theo dõi DB trong Git; hướng dẫn đồng đội sao lưu ra ngoài repo trước lần pull nhận commit loại bỏ DB. Chưa thực hiện trong task tài liệu.
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
