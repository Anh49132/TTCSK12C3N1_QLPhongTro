# Tiến độ dự án

## Cập nhật gần nhất — S1-04

- Đã làm nền tảng phân quyền DB, màn hình bảng quyền, menu trái theo quyền và chặn MVC/API trước khi xử lý/render.
- Người dùng chốt: 4 vai trò và 9 module theo tài liệu; QUAN_LY không truy cập toàn bộ “Hoá đơn, thanh toán và công nợ”. Bảng hiện hành và hướng dẫn: [s1-04-phan-quyen.md](s1-04-phan-quyen.md).
- Thêm 3 bảng app_role/app_module/role_permission và đủ 36 cặp từ JSON seed. Đã áp dụng vào SQLite hiện có bằng lệnh riêng có sao lưu; giữ nguyên nội dung/schema của tai_khoan/toa_nha/phong_tro.
- Đăng nhập/đăng xuất tối thiểu bằng cookie; xác minh trạng thái/role hiện tại mỗi request. API trả JSON 401 hoặc 403 thống nhất; trang bị cấm redirect tới AccessDenied trước render.
- Tạo bản sao demo riêng và 4 tài khoản theo lựa chọn người dùng; không nâng quyền tài khoản thật.

## Thành phần thay đổi

- Models/PermissionModels, Data/AppDbContext, PermissionSchemaInitializer, permissions.seed.json, PermissionDemo.
- Authorization/PermissionService, ModuleAccessAttribute, AppCookieEvents; Program đăng ký dịch vụ và lệnh CLI, bỏ tự tạo bảng/index khi startup.
- Controllers/Permissions, Modules, Account; thêm guard vào PhongTro và Home/ToggleRegistration.
- Views/Permissions, Modules, Account; Shared/_Layout menu trái, PhongTro/Index ẩn nút ghi; CSS và CSRF form đăng ký.
- tests/QL_PhongTro.Tests; .gitignore bỏ qua backup SQLite. Không commit hoặc push.

## Chạy và demo

- Trong QL_PhongTro: `dotnet restore --source https://api.nuget.org/v3/index.json`.
- CSDL khác chưa có quyền: `dotnet run --no-restore -- --initialize-permissions`; lệnh tự sao lưu, không ghi đè quyền đã có.
- Chạy thật: `dotnet run --no-restore --launch-profile http`, http://localhost:5247.
- Demo đã tạo: http://localhost:5250/Account/Login, DB `D:\TTCS\s104-validation\demo.sqlite`.
- 4 email/mật khẩu demo nằm tại `D:\TTCS\s104-validation\demo-access.txt`, ngoài repository.
- ADMIN xem bảng đủ 4 vai trò × 9 module tại /Permissions. KHACH_THUE chỉ xem cấu hình của vai trò mình.
- Backup dữ liệu thật: `QL_PhongTro/Data/local-dev.sqlite.before-s1-04-20260927103129533.bak`.
- Hướng dẫn chạy lại demo, test và response API chi tiết trong tài liệu S1-04.

## Xác minh thực tế

- Build thành công, 0 warning/0 error.
- 7 test tích hợp đã pass: ma trận 36 ô, menu/route/API cả 4 vai trò, cookie thiếu/hỏng/hết hạn,
  thu hồi quyền/đổi role/khóa tài khoản, quyền chỉ xem không tạo phòng, và bản sao demo.
- Test dùng bản sao SQLite; đối chiếu dữ liệu/schema cũ trước/sau khởi tạo quyền.
- Đối chiếu DB thật với backup: 3 bảng nghiệp vụ không đổi, 36 cặp quyền, PRAGMA integrity_check = ok.
- HTTP thật: đăng nhập 4 vai trò; menu lần lượt 8/9/7/9 mục; ADMIN thấy đủ 36 ô;
  QUAN_LY bị chặn tài chính cả trang và API; trang chủ bản thật trả 200.
- Kiểm tra HTTP sau khi tách phiên: bảng quyền demo trả 200; cookie demo dù đổi tên gửi sang bản thật vẫn trả 401. `git diff --check` sạch.
- Chưa kiểm tra bố cục bằng trình duyệt đồ họa; không ghi nhận các test lịch sử S1-08 là đã chạy lại.

## Giới hạn và việc tiếp theo

- Các module chưa có nghiệp vụ chỉ hiển thị danh mục/trang “chưa triển khai”, không tự làm backlog.
  Khi thêm route nghiệp vụ mới phải gắn guard và kiểm tra phạm vi dữ liệu; giữ giới hạn sở hữu tòa/phòng hiện có.
- Bảng quyền hiện chỉ xem, chưa có UI sửa/cấp role tài khoản. Quyền lấy trực tiếp từ DB trên request kế tiếp.
- Đăng nhập tối thiểu chưa hoàn thành S1-02/S1-03; cần bổ sung khóa do sai mật khẩu và quản lý phiên ở các task đó.
- S1-08 cũ vẫn có tạo phòng/tạo lô, lọc/đếm trạng thái, giá tối thiểu 500.000; nay chỉ vai trò có quyền ghi mới tạo được.
  Quy tắc mã tầng+2 chữ số, giới hạn 1..99 tầng/phòng và CRUD tòa nhà đầy đủ vẫn chờ task sau.

## Cập nhật gần nhất

- Hoàn thành S1-07: màn hình tổng hợp tòa nhà có tìm kiếm theo tên/địa chỉ, tổng số phòng và số phòng trống; form tạo/sửa đầy đủ địa chỉ, số tầng, ghi chú và người quản lý; hỗ trợ một quản lý được gán nhiều tòa; chặn xóa tòa đang có phòng và cho chuyển ngừng hoạt động; xóa được tòa không còn phòng.
- Giữ nguyên SQLite hiện có, không migration và không tạo lại database. Trạng thái phòng hợp lệ hiện dùng `TRONG`, `DA_DAT_COC`, `DANG_THUE`, `NGUNG_CHO_THUE`; phòng `TRONG` được tô nổi bật trong danh sách.
- Đã sửa lỗi render form phòng do `[Range(typeof(decimal), "0.01", ...)]` không parse được theo văn hóa `vi-VN`; dùng overload số của `RangeAttribute` cho cả tạo phòng đơn và tạo phòng hàng loạt.
- Nút xóa tòa nhà đã có hộp thoại xác nhận; chọn Hủy sẽ không gửi yêu cầu xóa.

- Hoàn thành S1-08 (quản lý phòng): tạo phòng đơn, chống trùng mã trong cùng tòa, kiểm tra giá thuê, tạo nhanh phòng theo tầng, lọc trạng thái và đếm phòng từng trạng thái.
- Đã thêm unique index SQLite trên `(toa_nha_id, ma_phong)`. Trước khi tạo index, ứng dụng kiểm tra dữ liệu trùng và dừng có thông báo nếu cần xử lý dữ liệu cũ; không xóa/sửa phòng tự động.
- Giá thuê được parse và xác thực phía server là số nguyên VND, tối thiểu 500.000; nhập/xuất dùng dấu chấm nhóm nghìn. Không rebuild bảng SQLite hiện có, nên CHECK vật lý trong bảng cũ vẫn chỉ là `gia_thue > 0`; quy tắc 500.000 được bảo vệ qua luồng ứng dụng.
- Tạo nhanh sinh mã theo quy ước tạm: tầng + số thứ tự phòng 2 chữ số (`101`, `102`, `201`...). Nếu lô có mã đã tồn tại trong cùng tòa, từ chối toàn bộ lô. Cần PO xác nhận quy tắc sinh mã và chính sách va chạm này.
- DB dự án ban đầu có 2 tài khoản và chưa có tòa/phòng. Không có bản ghi test được ghi vào DB dự án; test tích hợp dùng bản sao SQLite.

## Chạy

- Từ `QL_PhongTro`, chạy `dotnet restore --source https://api.nuget.org/v3/index.json` nếu nguồn NuGet mặc định đang tắt, sau đó `dotnet run`.
- Mở `/Account/Register`, đăng ký, vào **Quản lý phòng**; khai báo tòa nhà nếu chưa có. Dùng **Thêm phòng** hoặc **Tạo nhanh**.
- Có thể đặt biến môi trường `DatabasePath` tới một file SQLite hiện có; mặc định là `QL_PhongTro/Data/local-dev.sqlite`.

## Xác minh

- `dotnet build .\QL_PhongTro\QL_PhongTro.csproj --no-restore`: thành công sau thay đổi S1-07.
- Chưa thực hiện smoke test trình duyệt cho các luồng tạo/sửa/xóa tòa nhà trong task này.

- `dotnet build --no-restore`: thành công, không có warning.
- Smoke test trình duyệt trên DB tạm: mã mới, mã trùng cùng tòa bị chặn tại trường, cùng mã ở tòa khác được lưu; giá 500.000/1.000.000/10.500.000 được định dạng đúng; giá dưới ngưỡng, 0, âm và thập phân bị từ chối tại trường.
- Tạo lô 2×2 sinh đúng `101`, `102`, `201`, `202` và áp dụng cùng diện tích/giá/sức chứa/trạng thái. Lô va chạm không thêm phòng; tầng và số phòng bằng 0 bị chặn.
- Đã lọc lần lượt cả bốn trạng thái và đối chiếu số dòng với bộ đếm. `git diff --check` sạch.

## Giới hạn và bàn giao

- Luồng đăng ký hiện chỉ tạo `KHACH_THUE`, nên danh sách chọn người quản lý chỉ hiển thị các tài khoản đã có `vai_tro = QUAN_LY` và đang hoạt động; cần luồng cấp vai trò quản lý trước khi demo gán quản lý trên dữ liệu mới.

- Chưa có xác nhận PO cho quy tắc mã/số tầng thực tế (ví dụ tầng trệt/đánh số vượt 99 phòng mỗi tầng); hiện giới hạn đầu vào 1..99 tầng và 1..99 phòng/tầng.
- Màn hình tòa nhà hiện chỉ tạo tên và địa chỉ; chưa triển khai CRUD quản lý tòa nhà đầy đủ.
- Đăng ký hiện chỉ tạo vai trò `KHACH_THUE`; chưa có luồng cấp vai trò `CHU_NHA`/đăng nhập chủ nhà. Cần hoàn thiện xác thực vai trò trước production.
 dev
