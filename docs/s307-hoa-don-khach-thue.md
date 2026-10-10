# S3-07 — Chi tiết hóa đơn cho khách thuê

## Quy ước PO

- Tiền phòng là khoản cố định theo hợp đồng.
- Điện và nước tính theo chỉ số; giá điện phẳng, không chia bậc.
- Internet, phí dịch vụ và các dịch vụ cấu hình khác lấy mức cố định đã chốt cho phòng. Không dùng khoản khoán theo người trong danh sách khoản mục của AC này.
- Giữ lượng tiêu thụ tối đa ba chữ số thập phân. Thành tiền làm tròn từng dòng tới đồng bằng `AwayFromZero`.
- Tiền hiển thị theo VND, dấu chấm phân tách hàng nghìn và hậu tố `đ`.
- Khoản không có chỉ số hiển thị `—` ở hai cột chỉ số.

Các dòng đã lưu tiếp tục là snapshot bất biến của hóa đơn. Không đổi schema: kỳ, hợp đồng, tên khoản, chỉ số, lượng, đơn giá và thành tiền đã có trong `hoa_don` / `chi_tiet_hoa_don`.

## AC2–AC3: tổng kết, thanh toán và quá hạn

- Tổng cộng bằng tổng thành tiền các dòng: `GIAM_TRU` được lưu số dương nhưng trừ khỏi tổng; các dòng phí cộng vào. Cọc và công nợ kỳ trước không gộp vào hóa đơn.
- Hạn thanh toán được lưu trên hóa đơn; mặc định bằng ngày phát hành + 7 ngày. Chỉ khoản thanh toán `DA_XAC_NHAN` được cộng vào số đã thanh toán; khoản chờ xác nhận/từ chối/đã hủy không giảm nợ.
- Số còn phải trả là `max(tổng cộng - đã thanh toán, 0)`. Khi trả dư, hiển thị số đã thanh toán thực tế và số còn phải trả bằng 0.
- Quá hạn khi hóa đơn đang `DA_PHAT_HANH`, số còn phải trả > 0 và ngày hiện tại tại Việt Nam lớn hơn ngày hạn. Ngày hạn chưa quá hạn; ngày lịch đầu tiên sau hạn là trễ 1 ngày. Hóa đơn đã trả đủ không có nhãn dù thanh toán sau hạn.
- Nhãn dùng nền đỏ nhạt/chữ đỏ với nội dung `Quá hạn N ngày`. Hạn hiển thị `dd/MM/yyyy`. `Asia/Ho_Chi_Minh` được tính bằng UTC+7 trước khi so ngày lịch.
- Bảng thanh toán được thêm bằng updater schema v23 (bảng `thanh_toan`), FK hóa đơn `ON DELETE RESTRICT`; các schema đã cài và dữ liệu cũ được giữ nguyên. Đây chỉ là lưu trữ/đọc khoản thanh toán, chưa triển khai luồng khách báo trả hoặc chủ nhà xác nhận thu của S4.

### Bổ sung quyết định PO: không phát hành dịch vụ khoán theo người

- Không tạo mới hoặc phát hành hóa đơn nếu dịch vụ áp dụng cho phòng đang có cách tính `THEO_NGUOI`. Quy tắc áp dụng cho xem trước, tạo Nháp tháng, phát hành đơn lẻ/hàng loạt và phát hành Nháp đã tồn tại.
- Bản xem trước đánh dấu phòng cần xử lý, nêu tên dịch vụ và yêu cầu đổi sang cố định (`CO_DINH`). Các API ghi hóa đơn từ chối với cùng hướng dẫn; không tạo hóa đơn một phần.
- Nháp cũ cũng không thể phát hành nếu snapshot hoặc cấu hình dịch vụ liên quan vẫn là `THEO_NGUOI`. Dịch vụ cần được đổi sang cố định trước khi lập/phát hành lại hóa đơn.

## Truy cập và giao diện

- Mở `/ThongBao/ChiTiet?maHoaDon=<mã>` để xem trang chi tiết. Trang lấy dữ liệu từ `/ThongBao/ChiTietDuLieu?maHoaDon=<mã>`.
- Chỉ tài khoản `KHACH_THUE` có hồ sơ đứng tên hợp đồng được truy vấn hóa đơn đã phát hành. Hóa đơn của tài khoản khác, mã không tồn tại, bản Nháp hoặc module chưa cài trả cùng HTTP 404 và thông báo “Không tìm thấy hóa đơn.”
- Trang có trạng thái đang tải/lỗi, kỳ hóa đơn, phòng, bảng khoản mục theo `so_thu_tu` và bốn dòng tổng kết: tổng cộng, đã thanh toán, còn phải trả, hạn thanh toán. Hai cột chỉ số để `—` với khoản cố định. Nhãn quá hạn nằm gần hạn và chỉ hiện khi còn nợ, sau ngày hạn.
- Cả `/ThongBao/ChiTiet` và chi tiết từ thông báo (`/ThongBao/HoaDon/{id}`) hiển thị phần tổng kết. Chưa có danh sách hóa đơn, lọc theo kỳ hoặc lọc trạng thái thanh toán.
- Dữ liệu cá nhân của khách khác và các trường quản trị nội bộ không được trả về.

## Demo trên database demo S3-08

Chạy `.\verification\Start-S308Demo.ps1` để cập nhật và chuẩn bị dữ liệu trên bản sao nằm trong `data/s308-demo/`; script không dùng database mặc định. Bộ demo giữ hóa đơn chi tiết AC1 (giá dịch vụ chỉ minh họa) và tạo các hóa đơn mẫu thanh toán/quá hạn riêng. Mã mẫu được ghi trong `tenant-invoice-samples.txt`.

Đăng nhập bằng tài khoản khách thuê giả trong `access.json` của bộ Sprint 2 nguồn, chọn mã trong `tenant-invoice-samples.txt` hoặc `tenant-invoice-code.txt` trong thư mục database demo, rồi mở:

```text
/ThongBao/ChiTiet?maHoaDon=<mã trong tenant-invoice-code.txt>
```

Không đưa database, file mã, credential hoặc backup demo vào Git.

## Kiểm thử

```powershell
dotnet test Tests\QL_PhongTro.Tests\QL_PhongTro.Tests.csproj --filter "FullyQualifiedName~TenantCanOpenOnlyTheirPublishedInvoiceByCodeAndSeeAllOrderedLines|FullyQualifiedName~TenantInvoiceSummaryReflectsPaymentsDiscountsAndVietnamDueDates|FullyQualifiedName~Version23AddsPaymentHistoryWithoutChangingExistingInvoiceData"
```

Kiểm thử dùng SQLite tạm; xác nhận quyền sở hữu, 404 đồng nhất, phép tính tổng/giảm trừ, nhiều khoản thanh toán và các trạng thái tiền, hạn/biên ngày Việt Nam qua tháng/năm, không hiện nhãn khi đủ tiền, HTML tổng kết/nhãn, và updater v23 bảo toàn hóa đơn cùng FK.
