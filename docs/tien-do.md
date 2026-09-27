# Tiến độ dự án

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
