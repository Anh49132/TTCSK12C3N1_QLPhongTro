# Tiến độ dự án

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
