# S1-04 — Phân quyền theo module

## Quyết định đã được người dùng chốt

Giữ 4 vai trò và 9 module trong trang “User Roles” của `01-yeu-cau-he-thong.md`.
Riêng QUAN_LY không truy cập toàn bộ module “Hoá đơn, thanh toán và công nợ”.
Bảng dưới là quyết định hiện hành, thay ô R của Manager ở dòng 18 trong bản trích Excel.

| Module (mã) | Khách thuê | Chủ nhà | Quản lý tòa nhà | Quản trị hệ thống |
| --- | --- | --- | --- | --- |
| Tài khoản và phân quyền (TAI_KHOAN) | R* | R | — | F |
| Tòa nhà, phòng và bảng giá (PHONG_TRO) | R | F | R | R |
| Tin đăng cho thuê (TIN_DANG) | R | F | R | R |
| Yêu cầu thuê và lịch xem phòng (YEU_CAU_THUE) | W* | F | R | R |
| Hợp đồng thuê và người ở ghép (HOP_DONG) | R* | F | R | R |
| Chỉ số điện nước (DIEN_NUOC) | R* | F | W | R |
| Hoá đơn, thanh toán và công nợ (TAI_CHINH) | R* | F | — | R |
| Báo hỏng và bảo trì (BAO_HONG) | W* | F | W | R |
| Báo cáo doanh thu và tỉ lệ lấp đầy (BAO_CAO) | — | F | R | R |

F = FULL, W = WRITE, R = READ, — = NONE; * = OwnDataOnly.
Quyền xem được cấp cho READ/WRITE/FULL, quyền ghi chỉ WRITE/FULL.
Không có cặp quyền hoặc vai trò không xác định thì từ chối.

## Lưu trữ và khởi tạo

Đã kiểm tra SQLite thực tế trước thay đổi: có `tai_khoan`, `toa_nha`, `phong_tro`; chưa có bảng quyền.
Thêm ba bảng:
- `app_role`: Code (PK), Name, SortOrder.
- `app_module`: Code (PK), Name, GroupName, SortOrder.
- `role_permission`: PK ghép RoleCode/ModuleCode, FK tới hai bảng trên; AccessLevel và OwnDataOnly có CHECK.

`Data/permissions.seed.json` chứa 4 vai trò, 9 module, đủ 36 cặp, kể cả NONE.
Quyền khi xử lý request được đọc từ SQLite, không lấy lại từ JSON.
Lệnh khởi tạo chỉ nhập seed nếu cả ba bảng chưa có dữ liệu; chạy lại không cấp lại quyền đã thu hồi.
Không có màn hình sửa/cấp vai trò tài khoản trong S1-04 này.

Chạy từ thư mục `QL_PhongTro`:

```powershell
dotnet restore --source https://api.nuget.org/v3/index.json
dotnet run --no-restore -- --initialize-permissions
dotnet run --no-restore --launch-profile http
```

Lệnh khởi tạo tạo bản sao `local-dev.sqlite.before-s1-04-<UTC>.bak` trước giao dịch.
Không tạo lại bảng nghiệp vụ, không sửa tài khoản. Bình thường khởi động web không còn gọi
`RoomSchemaInitializer.EnsureSchema`; cũng không tự chạy migration/seed.
`DatabasePath` phải trỏ tới SQLite hiện có, mặc định `Data/local-dev.sqlite`.
Giữ bản sao lưu ngoài Git; không thay file SQLite trong lúc ứng dụng đang chạy.

## Màn hình và kiểm tra quyền

- `/Account/Login`: đăng nhập email hoặc điện thoại với BCrypt; `/Account/Logout`: POST có CSRF.
- `/Permissions`: bảng quyền hiện tại. ADMIN/CHU_NHA xem 4 vai trò; KHACH_THUE có R* chỉ xem vai trò của mình; QUAN_LY bị chặn.
- Menu trái nhóm theo module, lấy cùng PermissionService với bộ kiểm tra server. Nhóm rỗng không render.
- `ModuleAccessAttribute` là authorization filter MVC, chặn trước action/render Razor.
  Không cần frontend Route Guard của SPA vì dự án dùng server-rendered MVC.
- `/api/permissions`: JSON cấu hình quyền, cùng giới hạn với màn hình.
- `/Modules/{code}`, `/api/modules/{code}`: trang/dữ liệu danh mục module để demo quyền.
  Module chưa triển khai chỉ có thông tin tên và thông báo; không có dữ liệu hay CRUD nghiệp vụ giả.
- `PhongTroController`: kiểm tra quyền xem; các action Create/CreateBulk/TaoToaNha kiểm tra thêm quyền ghi.
  Bộ lọc sở hữu tòa/phòng hiện có được giữ nguyên, không tự mở quyền xem dữ liệu của chủ khác.
- `Home/ToggleRegistration` cũng được bảo vệ bởi quyền ghi TAI_KHOAN và CSRF.
- Cookie được đối chiếu tài khoản/role/trạng thái trong DB mỗi request. Thu hồi quyền, đổi role,
  khóa tài khoản có hiệu lực ở request tiếp theo. Cache quyền chỉ tồn tại trong một request.
- Cookie và Data Protection application scope tách theo đường dẫn CSDL để bản demo không dùng lẫn phiên bản thật.

API không có phiên/cookie hỏng/hết hạn trả 401:
```json
{"code":"UNAUTHENTICATED","message":"Phiên đăng nhập không hợp lệ hoặc đã hết hạn."}
```

API có phiên nhưng thiếu quyền trả 403:
```json
{"code":"FORBIDDEN","message":"Bạn không có quyền truy cập chức năng này."}
```

Trang bị cấm chuyển hướng tới `/Account/AccessDenied`, trang đích trả 403.
Response chuyển hướng không render nội dung trang bị cấm.
Trang chưa đăng nhập chuyển tới Login; API luôn trả JSON 401, không redirect.
Dùng cookie đăng nhập thực khi thử cURL/Postman; ứng dụng không dùng JWT bearer token.

## Demo riêng

Người dùng đã chọn demo trên bản sao SQLite, không nâng quyền tài khoản thật.

Tạo bản sao mới (đường dẫn đích phải chưa tồn tại):

```powershell
dotnet run --no-restore -- --create-permission-demo D:\TTCS\s104-validation\demo-moi.sqlite
dotnet run --no-build --no-launch-profile -- --urls http://localhost:5250 --DatabasePath D:\TTCS\s104-validation\demo-moi.sqlite
```

Lệnh in 4 email demo và mật khẩu ngẫu nhiên dùng chung cho lần tạo đó.
Tài khoản demo chỉ được thêm vào bản sao; đăng ký thông thường vẫn chỉ tạo KHACH_THUE.
Không đưa mật khẩu demo vào mã nguồn hoặc seed.

Bản demo đã tạo trong phiên này:
- URL: http://localhost:5250/Account/Login
- SQLite: `D:\TTCS\s104-validation\demo.sqlite`.
- Thông tin đăng nhập: `D:\TTCS\s104-validation\demo-access.txt` (ngoài repository).
- Bản dữ liệu thật: http://localhost:5247.

Demo: đăng nhập ADMIN xem bảng 4×9; đăng xuất và đăng nhập QUAN_LY,
xác nhận không thấy tài chính; mở `/Modules/TAI_CHINH` bị chuyển tới trang thiếu quyền.
Gọi `/api/modules/TAI_CHINH` bằng cookie quản lý nhận 403; không cookie nhận 401.

## Kiểm thử và giới hạn

Từ gốc repository:

```powershell
dotnet restore tests/QL_PhongTro.Tests/QL_PhongTro.Tests.csproj --source https://api.nuget.org/v3/index.json
dotnet test tests/QL_PhongTro.Tests/QL_PhongTro.Tests.csproj --no-restore -p:UseAppHost=false
```

7 test tích hợp dùng WebApplicationFactory và SQLite sao chép vào thư mục tạm:
đối chiếu độc lập 36 ô seed; không ghi đè quyền khi khởi tạo lại; bảng Razor đủ ô;
menu và đường dẫn/API cho cả 4 vai trò; cookie thiếu/hỏng/hết hạn 401; thu hồi quyền,
đổi role, thiếu cặp quyền, khóa tài khoản; chặn tạo phòng với quyền chỉ xem;
tạo demo giữ nguyên nguồn và từ chối ghi đè file có sẵn.
Fixture đối chiếu schema/nội dung 3 bảng nghiệp vụ trước/sau khởi tạo.
Đã kiểm tra HTTP thật cho 4 tài khoản demo; cookie demo (kể cả đổi tên cookie) bị bản thật từ chối 401.
Chưa kiểm tra bố cục bằng trình duyệt đồ họa.

S1-04 bảo vệ các route hiện có và cung cấp cơ chế chung. API/nghiệp vụ các module còn thiếu
phải gắn ModuleAccess và lọc phạm vi dữ liệu khi được triển khai ở task sau.
OwnDataOnly được lưu và áp dụng cho màn hình quyền; không thay thế kiểm tra sở hữu dữ liệu
của các nghiệp vụ tương lai. Đăng nhập bổ sung ở đây là mức tối thiểu để dùng quyền;
chưa hoàn thành S1-02 (khóa do sai mật khẩu, quản lý/thu hồi mọi phiên), S1-03 (cấp tài khoản)
hay giao diện chỉnh sửa ma trận quyền.
