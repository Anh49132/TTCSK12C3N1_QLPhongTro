# S3-07 — Chi tiết hóa đơn cho khách thuê

## Quy ước PO

- Tiền phòng là khoản cố định theo hợp đồng.
- Điện và nước tính theo chỉ số; giá điện phẳng, không chia bậc.
- Internet, phí dịch vụ và các dịch vụ cấu hình khác lấy mức cố định đã chốt cho phòng. Không dùng khoản khoán theo người trong danh sách khoản mục của AC này.
- Giữ lượng tiêu thụ tối đa ba chữ số thập phân. Thành tiền làm tròn từng dòng tới đồng bằng `AwayFromZero`.
- Tiền hiển thị theo VND, dấu chấm phân tách hàng nghìn và hậu tố `đ`.
- Khoản không có chỉ số hiển thị `—` ở hai cột chỉ số.

Các dòng đã lưu tiếp tục là snapshot bất biến của hóa đơn. Không đổi schema: kỳ, hợp đồng, tên khoản, chỉ số, lượng, đơn giá và thành tiền đã có trong `hoa_don` / `chi_tiet_hoa_don`.

### Bổ sung quyết định PO: không phát hành dịch vụ khoán theo người

- Không tạo mới hoặc phát hành hóa đơn nếu dịch vụ áp dụng cho phòng đang có cách tính `THEO_NGUOI`. Quy tắc áp dụng cho xem trước, tạo Nháp tháng, phát hành đơn lẻ/hàng loạt và phát hành Nháp đã tồn tại.
- Bản xem trước đánh dấu phòng cần xử lý, nêu tên dịch vụ và yêu cầu đổi sang cố định (`CO_DINH`). Các API ghi hóa đơn từ chối với cùng hướng dẫn; không tạo hóa đơn một phần.
- Nháp cũ cũng không thể phát hành nếu snapshot hoặc cấu hình dịch vụ liên quan vẫn là `THEO_NGUOI`. Dịch vụ cần được đổi sang cố định trước khi lập/phát hành lại hóa đơn.

## Truy cập và giao diện

- Mở `/ThongBao/ChiTiet?maHoaDon=<mã>` để xem trang chi tiết. Trang lấy dữ liệu từ `/ThongBao/ChiTietDuLieu?maHoaDon=<mã>`.
- Chỉ tài khoản `KHACH_THUE` có hồ sơ đứng tên hợp đồng được truy vấn hóa đơn đã phát hành. Hóa đơn của tài khoản khác, mã không tồn tại, bản Nháp hoặc module chưa cài trả cùng HTTP 404 và thông báo “Không tìm thấy hóa đơn.”
- Trang có trạng thái đang tải, lỗi tải lại được thông báo rõ, kỳ hóa đơn, phòng và bảng khoản mục theo `so_thu_tu`. Hai cột chỉ số để `—` với khoản cố định. Không hiển thị tổng, thanh toán, dư nợ, hạn thanh toán hay danh sách hóa đơn.
- Dữ liệu cá nhân của khách khác và các trường quản trị nội bộ không được trả về.

## Demo trên database demo S3-08

Chạy `.\verification\Start-S308Demo.ps1` để cập nhật và chuẩn bị dữ liệu trên bản sao nằm trong `data/s308-demo/`; script không dùng database mặc định. Bộ demo tạo một hóa đơn đã phát hành với tiền phòng, điện, nước, Internet và phí dịch vụ; giá điện/nước trong mẫu chỉ để minh họa, không phải bảng giá dùng cho nghiệp vụ thật.

Đăng nhập bằng tài khoản khách thuê giả trong `access.json` của bộ Sprint 2 nguồn, đọc mã ở `tenant-invoice-code.txt` trong thư mục database demo, rồi mở:

```text
/ThongBao/ChiTiet?maHoaDon=<mã trong tenant-invoice-code.txt>
```

Không đưa database, file mã, credential hoặc backup demo vào Git.

## Kiểm thử

```powershell
dotnet test Tests\QL_PhongTro.Tests\QL_PhongTro.Tests.csproj --filter FullyQualifiedName~TenantCanOpenOnlyTheirPublishedInvoiceByCodeAndSeeAllOrderedLines
```

Kiểm thử dùng SQLite tạm mới; xác nhận quyền sở hữu, 404 đồng nhất, thứ tự và số dòng, chỉ số/tiêu thụ, phép nhân và làm tròn tiền, khoản cố định không có chỉ số, cùng HTML trạng thái ban đầu và các cột.
