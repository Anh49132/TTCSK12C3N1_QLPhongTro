# S1-10 EP-01 — Nhật ký hoạt động

## Phạm vi

Triển khai trên `feature/s1-10-quan-ly-nhat-ky-fix`, trong clone `feature10`.

- Tài khoản: ADMIN tạo, khóa, mở khóa, thay mật khẩu tạm khi gửi lại. Một lần tạo tài khoản có đúng một nhật ký. Tự đăng ký khách thuê cũng ghi một dòng, xác minh tài khoản vừa tạo trong transaction trước khi ghi người thực hiện.
- Tòa nhà: tạo, sửa các trường được phép, ngừng hoạt động, xóa khi nghiệp vụ cho phép. Phòng: tạo đơn lẻ/hàng loạt qua các chức năng hiện có; cơ chế ghi cũng hỗ trợ sửa/xóa entity nếu sau này có luồng tương ứng.
- Dịch vụ: danh mục, khởi tạo mặc định, đơn giá ban đầu, đóng phiên bản cũ/tạo phiên bản giá mới, ngừng/kích hoạt, xóa. Một thao tác tác động nhiều đối tượng có nhiều dòng nhật ký tương ứng, trong cùng transaction.
- Hóa đơn S1-09 có luồng ghi thật: ghi tạo hóa đơn/dòng hóa đơn và chuyển `NHAP` sang `DA_PHAT_HANH`. Chỉ số đầu/cuối được lưu trong dòng hóa đơn nên được ghi ở loại `chi_tiet_hoa_don`. Service gán dịch vụ hợp đồng cũng được bao phủ tại SaveChanges.
- Chưa có CRUD hợp đồng, bảng chỉ số điện nước độc lập hoặc thanh toán trong controller/service nghiệp vụ; không triển khai thêm. Các loại đối tượng đã có trong danh mục để mở rộng. Hợp đồng/kỳ thuê trong test chỉ là fixture SQL.
- `/NhatKy` chỉ ADMIN: xác thực cookie và kiểm tra tài khoản hoạt động/vai trò trong DB; query/form không được cấp quyền. Danh sách mới nhất trước, 50 dòng/trang, mở chi tiết JSON trước/sau, lọc ngày/người/loại riêng hoặc đồng thời. Menu ở thanh điều hướng ADMIN.

## Schema và bảo mật

Phiên bản 3 trong `DatabaseUpdates`, bảng `nhat_ky_hoat_dong` gồm `id`, `nguoi_thuc_hien_id`, `ten_nguoi_thuc_hien`, `vai_tro_luc_thuc_hien`, `loai_doi_tuong`, `doi_tuong_id`, `hanh_dong`, `du_lieu_truoc`, `du_lieu_sau`, `ghi_chu`, `thoi_diem`. `ghi_chu` giữ tương thích đặc tả, không ghi dữ liệu tự do từ request.

- Snapshot tên/vai trò lấy bằng truy vấn `AsNoTracking` vào tài khoản đã xác thực ngay trong transaction; không lấy tên/vai trò từ form hoặc tin claim cũ. Dòng lịch sử không đổi khi tài khoản đổi tên/vai trò.
- JSON dùng khóa tên cột, sắp xếp ordinal. Tạo/xóa ghi các trường cho phép; sửa chỉ ghi trường thực sự đổi. Không ghi lại no-op. Trước tạo/sau xóa là NULL.
- Allowlist chính xác nằm ở `Data/AppDbContext.Audit.cs`: tài khoản chỉ tên/vai trò/hoạt động/cờ buộc đổi mật khẩu; tòa nhà và phòng chỉ thông tin quản lý, địa chỉ, diện tích/giá/trạng thái; dịch vụ/giá/hóa đơn chỉ trường tính toán, mã liên kết và trạng thái. Loại bỏ email/điện thoại tài khoản, mật khẩu/hash/token/cookie, hồ sơ/ảnh/giấy tờ và các ghi chú/mô tả tự do.
- `must_change_password` là boolean trạng thái, không phải giá trị mật khẩu. Gửi lại mật khẩu tạm ghi hành động `GUI_LAI_MAT_KHAU_TAM`, JSON rỗng nếu không có trường công khai thay đổi. Đây là việc đổi mật khẩu tạm trong DB; không khẳng định email đã đến hộp thư. Gửi email vẫn diễn ra sau commit như S1-03.
- SaveChanges bắt đầu transaction khi chưa có; nếu đã có transaction, dùng savepoint. Lưu nghiệp vụ, lấy ID thật, chèn audit có tham số rồi mới commit/nhả savepoint. Lỗi bất kỳ rollback; context đã lỗi không được dùng lại để lưu. Transaction ngoài của dịch vụ giữ nguyên, kể cả luồng nhiều SaveChanges.
- EF từ chối Added/Modified/Deleted trực tiếp với entity nhật ký. Trigger `audit_no_update`, `audit_no_delete` chặn UPDATE/DELETE kể cả SQL trực tiếp qua kết nối ứng dụng. Không có endpoint tạo/sửa/xóa. Người có quyền quản trị file SQLite có thể bỏ trigger; đó là giới hạn ngoài quyền ứng dụng.
- Lưu UTC như mã hiện có, hiển thị UTC+7. Ngày lọc là ngày Việt Nam, chuyển thành khoảng UTC `[đầu ngày từ, đầu ngày sau ngày đến)`; hỗ trợ năm 1900–9998. Khóa phụ ID bảo đảm thứ tự ổn định khi trùng thời điểm.
- Updater sao lưu trước thay đổi. Bước v3 kiểm tra schema lõi, tạo/thêm cột tên cho schema audit cũ, kiểm tra trigger, integrity/FK và ghi phiên bản trong cùng transaction. Lỗi không ghi v3. Không đoán tên quá khứ: nhật ký cũ thiếu tên được giữ NULL, UI ghi “Chưa có tên lịch sử”. Bước v1/v2 đã phát hành giữ nguyên.

## Thành phần thay đổi

- Mới: `Models/NhatKyHoatDong.cs`, `Data/AppDbContext.Audit.cs`, `Data/AuditSchema.cs`, `Controllers/NhatKyController.cs`, `ViewModels/NhatKyViewModel.cs`, `Views/NhatKy/Index.cshtml`.
- Cập nhật `AppDbContext` (DbSet, HTTP accessor), `DatabaseUpdates` (v3/check), `AccountController` (tự đăng ký), `ManagedAccountsController` (ý định gửi lại mật khẩu tạm), `_Layout.cshtml` (liên kết ADMIN).
- Kiểm thử: `verification/s110_http.py`, `verification/s110_schema.py`, `verification/S110/`. Ignore toàn bộ fixture/log/mail trong `data/S1-10-verification/`.
- Không sửa project test bị va chạm `Tests/`/`tests/`; không tạo staging/Docker; không commit/merge/push.

## Nâng cấp và chạy trên bản sao để xem diff

DB gốc được kiểm tra chỉ đọc: clone này chỉ có `tai_khoan` và `sqlite_sequence`. Không chạy updater vào `QL_PhongTro/Data/local-dev.sqlite`. Các xác minh bên dưới đều kiểm tra SHA-256 DB gốc không đổi.

Từ thư mục gốc repo, dừng đúng phiên ứng dụng dùng file nguồn trước khi sao chép (không dừng app của clone khác). Chọn tên bản sao mới:

```powershell
New-Item -ItemType Directory -Force data | Out-Null
if (Test-Path data/s110-review.sqlite) { throw 'Chọn tên bản sao mới, không ghi đè.' }
Copy-Item -LiteralPath QL_PhongTro/Data/local-dev.sqlite -Destination data/s110-review.sqlite
$env:DatabasePath = (Resolve-Path data/s110-review.sqlite).Path
dotnet build QL_PhongTro/QL_PhongTro.csproj -c Debug
dotnet run --project QL_PhongTro --no-build -- --update-database
dotnet run --project QL_PhongTro --no-build -- --check-database
```

Cấu hình ADMIN thử theo mục LocalAdmin trong README bằng biến môi trường do người kiểm thử tự chọn; chạy `--create-local-admin` trong Development trên **cùng DatabasePath bản sao**. Không đưa thông tin đó vào Git. Sau đó:

```powershell
dotnet run --project QL_PhongTro --no-build --launch-profile http
```

Mở `http://localhost:5247`. Dùng Ctrl+C tại terminal đó để dừng. Khi xong, `Remove-Item Env:DatabasePath` tránh dùng nhầm ở lệnh sau. DB thật chỉ nâng cấp khi chủ dự án yêu cầu rõ sau khi xem kết quả bản sao.

S1-09 vẫn tùy chọn. Fixture đầy đủ được script `s110_http.py` tạo trên bản sao riêng; không cài schema hợp đồng/hóa đơn thử vào DB thật. Xem thêm `s1-09-dich-vu.md` để chuẩn bị dữ liệu demo thủ công.

## Test bằng giao diện

1. Đăng nhập ADMIN trên bản sao. Mở **Nhật ký hoạt động** hoặc `/NhatKy`, thấy danh sách và bộ lọc. Mở bằng khách thuê/chủ nhà/quản lý: backend trả 403; chưa đăng nhập chuyển tới Login.
2. ADMIN vào **Quản lý tài khoản**, tạo Chủ nhà/Quản lý hợp lệ. Quay lại nhật ký, chọn loại Tài khoản: có đúng một dòng TAO cho ID mới, tên/vai trò ADMIN và dữ liệu sau không có bí mật.
3. Khóa rồi mở khóa tài khoản đó; xem hai dòng KHOA/MO_KHOA với `dang_hoat_dong` đảo đúng chiều. Gửi lại mật khẩu tạm khi tài khoản còn chờ đổi: có hành động tương ứng, không có mật khẩu trong JSON.
4. Thử email trùng hoặc form không hợp lệ: số dòng nhật ký không tăng. Kiểm tra đổi mật khẩu/đăng nhập vẫn theo S1-03; không gửi mail thật khi đang dùng pickup thử.
5. Đăng nhập Chủ nhà đã đổi mật khẩu. Tạo tòa nhà, sửa tên, tạo phòng. Trở lại bằng ADMIN và lọc người thực hiện/loại; kiểm tra ID, tên, vai trò và trước/sau. Có thể dùng hai trình duyệt/phiên riêng.
6. Trên bản sao có S1-09, khởi tạo dịch vụ hoặc thêm dịch vụ mới, sửa giá ban đầu/tạo giá tương lai, ngừng/kích hoạt. Mở nhật ký loại Dịch vụ/Đơn giá. Phiên bản mới có ID mới; đóng ngày của phiên bản cũ có dòng sửa riêng.
7. Với hợp đồng fixture hợp lệ, phát hành hóa đơn ở `/HoaDonDichVu?toaNhaId=<ID>`. Nhật ký có hóa đơn tạo, các dòng và đổi trạng thái phát hành; phát hành trùng bị từ chối, không tăng audit.
8. Chọn ngày từ/đến, người và loại cùng lúc; kết quả thỏa cả ba. Ngày kết thúc bao gồm hết ngày Việt Nam. Đảo ngày hoặc nhập query ngày sai nhận 400. Mở trước/sau để xem JSON; đổi trang giữ bộ lọc.
9. Gọi POST `/NhatKy`, `/NhatKy/Edit/1`, `/NhatKy/Delete/1`: 404/405, không thay đổi nhật ký. Kiểm tra SQL/rollback trên bản thử bằng các script bên dưới, không làm trên DB thật.

## Xác minh thực tế

Đã chạy ngày 29/09/2026:

- Build Debug app và console S110 thành công, 0 lỗi.
- `python verification/database_updates.py`: PASS hồi quy nâng cấp, bảo toàn dữ liệu, backup, lặp lại, quyền bị thu hồi, version tương lai, file không tồn tại.
- `python verification/s110_schema.py`: PASS v2/schema không đổi khi v3 lỗi do thiếu cột hoặc schema tùy chọn dang dở; giữ log cũ, tên NULL; từ chối trigger cùng tên nhưng yếu hơn và startup thiếu trigger.
- `python verification/s110_http.py`: PASS quyền ADMIN/non-ADMIN/anonymous và thu hồi vai trò; tài khoản tạo đúng một dòng, khóa/mở khóa/gửi lại; validation/trùng; tòa nhà/phòng; dịch vụ/phiên bản giá; hóa đơn phát hành/trùng; rollback khi audit lỗi và khi bước sau trong transaction nghiệp vụ lỗi; snapshot tên; HTTP/SQLite bất biến; lọc riêng/kết hợp/biên ngày UTC+7; không chứa secrets; integrity=ok, FK rỗng. Khởi động thật tại localhost:5247 rồi dừng server; DB nguồn giữ nguyên.
- Console EF: PASS chặn thêm tay/sửa/xóa nhật ký; SaveChanges đồng bộ; đọc lại tên/role trong DB thay claim giả; no-op; xóa ghi before/after; thiếu actor rollback; savepoint rollback vẫn an toàn nếu caller bắt lỗi rồi commit transaction ngoài.
- `python verification/s103_http.py`: PASS hồi quy tạo/validation/email pickup/ép đổi mật khẩu/cookie/JWT/refresh/khóa/mở khóa/lọc/phân trang.
- HTTP test cần chạy dưới tài khoản Windows bình thường nếu sandbox không truy cập DPAPI/Event Log; không thay đổi cấu hình bảo mật ứng dụng để né lỗi môi trường.
- `git diff --check` toàn repo báo dòng trống cuối `Tests/QL_PhongTro.Tests/QL_PhongTro.Tests.csproj` có sẵn sau clone. Giữ nguyên theo yêu cầu. Phần thay đổi task được kiểm tra riêng.

Chạy lại console sau HTTP suite (console tự tạo thêm bản sao):

```powershell
dotnet build verification/S110/S110.csproj
$auditFixture = Get-ChildItem data/S1-10-verification -Directory | Where-Object Name -Match '^\d+$' | Sort-Object Name -Descending | Select-Object -First 1
dotnet verification/S110/bin/Debug/net10.0/S110.dll (Get-Location).Path (Join-Path $auditFixture.FullName 'audit.sqlite')
```

Cảnh báo còn lại: `CS8601` tại `AuthController.cs:34,52`; thiếu giấy phép Six Labors ImageSharp 4.1.2; `NU1900` vì không truy cập được nguồn NuGet để lấy dữ liệu lỗ hổng trong sandbox. Không tuyên bố đã kiểm tra lỗ hổng package. Release chưa chạy lại, vẫn cần license theo README.

## Giới hạn

- Chưa nghiệm thu giao diện bằng trình duyệt đồ họa; đã kiểm tra Razor build và HTML/HTTP thật. Chưa thử SMTP thật hoặc tải đồng thời/crash.
- Phạm vi audit là các entity nghiệp vụ trong allowlist và các luồng SaveChanges hiện có. Các thao tác hạ tầng đăng nhập/refresh/reset/đổi mật khẩu thường, quyền module, hồ sơ nhạy cảm và CLI bảo trì không thuộc nhật ký nghiệp vụ này. Bulk SQL/ExecuteUpdate của chức năng mới phải tích hợp audit trong transaction trước khi phát hành; không tự được bắt bởi ChangeTracker.
- Nhật ký ghi theo thay đổi mỗi đối tượng/mỗi SaveChanges, không gộp toàn bộ request vào một dòng. Giá và hóa đơn nhiều bước vẫn cùng transaction nghiệp vụ. Không chụp lại log cũ từ dữ liệu hiện tại.
- Không tự tạo CRUD hợp đồng/thanh toán hoặc bảng chỉ số chưa có. Không nâng cấp DB local thật trong lần phát triển này.
