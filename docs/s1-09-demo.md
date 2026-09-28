# Demo S1-09 — dữ liệu ngày 28/09/2026

## Môi trường

- App đang chạy: `http://localhost:5247`.
- DB demo: `data/S1-09-verification/20260928102752-c3ffa2/test.sqlite`; tài khoản trong `demo-access.txt` cùng thư mục.
- Tòa ID 1, phòng `DEMO-101`, giá thuê 2.000.000đ/tháng, sức chứa 4, hợp đồng ID 1.
- Chỉ ghi DB demo; giá dưới đây là số liệu kiểm thử, chưa phải giá thương mại PO chốt.

## Dữ liệu sẵn có

| Dịch vụ | Cách tính | Đơn vị | Giá |
|---|---|---|---:|
| Điện | Theo chỉ số | kWh | 3.000; từ 01/10/2026 là 3.500 |
| Nước | Theo chỉ số | m³ | 15.000 |
| Rác | Theo đầu người | người/tháng | 20.000 |
| Gửi xe | Cố định theo phòng | phòng/tháng | 50.000 |
| Internet | Cố định theo phòng | phòng/tháng | 100.000 |
| DEMO - Vệ sinh bổ sung | Cố định theo phòng | phòng/tháng | 30.000 |
| DEMO - Trạng thái | Cố định theo phòng | phòng/tháng | 10.000 |

- Điện ID 1 đã có hóa đơn tham chiếu và lịch sử giá.
- Nước có snapshot trong `hop_dong_dich_vu` của hợp đồng 1, giá 15.000; dùng thử chặn xóa theo hợp đồng.
- `DEMO - Vệ sinh bổ sung` chưa sử dụng, dành cho người dùng thử xóa.
- `DEMO - Trạng thái` ID 7: hiện áp dụng, ngừng từ 01/10, kích hoạt lại từ 01/11. Danh sách chính hiển thị trạng thái hôm nay; lịch sử hiển thị các thay đổi đã lên lịch.
- Hóa đơn ID 1 kỳ 09/2026: phòng 2.000.000 + điện (110−100)×3.000 = **2.030.000đ**. Chưa tạo hóa đơn tháng 10 để người dùng thao tác.

## Thứ tự test thủ công

1. Mở `/DichVu?toaNhaId=1`, tải lại: năm dịch vụ mặc định cộng hai dịch vụ demo, không trùng.
2. Chọn Điện: thấy 3.000 đến 30/09, 3.500 từ 01/10. Thử thêm giá với ngày 01/10/2026: bị từ chối do trùng ngày. Giá trên danh sách hiện tại vẫn 3.000 vì hôm nay là 28/09.
3. Xem `/HoaDonDichVu/Details/1`: tháng 9 vẫn 2.030.000đ, điện 3.000đ. Không phát hành lại tháng 9 vì đã có hóa đơn.
4. Mở `/HoaDonDichVu?toaNhaId=1&ngayApDung=2026-10-01`. Chọn hợp đồng demo, số người 2, **chỉ chọn điện**, chỉ số 100 → 110. Phát hành: dự kiến **2.035.000đ**. Nếu đã làm bước này thì xem hóa đơn tháng 10 có sẵn, không tạo lại.
5. Mở lại hóa đơn tháng 9: vẫn giữ nguyên. Thử xóa Điện: bị chặn do có hóa đơn.
6. Thử xóa Nước: bị chặn do có hợp đồng; snapshot hợp đồng vẫn giữ giá 15.000.
7. Xóa `DEMO - Vệ sinh bổ sung`: thành công vì chưa dùng. Tải lại, không tự xuất hiện lại.
8. Mở `/DichVu/Manage?toaNhaId=1&dichVuId=7` xem lịch sử. Form hóa đơn ngày 01/10 không có `DEMO - Trạng thái`; đổi ngày sang 01/11 sẽ thấy lại. Không cần phát hành hóa đơn để kiểm tra việc chọn dịch vụ.
9. Thêm dịch vụ tùy chỉnh lần lượt theo chỉ số/theo người/cố định; kiểm tra danh sách. Bỏ tên/đơn vị/giá hoặc nhập giá âm: không lưu được. Giá điện/nước mặc định bằng 0 cũng bị chặn.

## Xác minh thực tế

Đã kiểm tra qua HTTP thật với cookie đăng nhập và anti-forgery:

- Đăng nhập, khởi tạo năm dịch vụ, lưu giá của cả năm, khởi tạo lại không trùng.
- Giá âm/thiếu tên bị từ chối, dịch vụ tùy chỉnh lưu và hiển thị được.
- Phát hành tháng 9 đúng 2.030.000; đặt giá 3.500 từ tháng 10; trùng ngày bị từ chối; hóa đơn cũ giữ nguyên; form tháng 10 hiển thị giá mới.
- Xóa Điện bị chặn theo hóa đơn, xóa Nước bị chặn theo hợp đồng. Tạo rồi xóa `DEMO - Kiểm thử xóa` thành công; dịch vụ này không còn trong bộ dữ liệu bàn giao.
- Ngừng/kích hoạt dịch vụ mẫu đúng lịch; form hóa đơn ẩn/hiện theo ngày áp dụng; hóa đơn cũ không đổi.
- Schema demo đã được đọc trước khi ghi. Tham chiếu Nước bổ sung trực tiếp chỉ vào DB demo sau sao lưu `test.sqlite.before-contract-demo-20260928103841.bak`; integrity_check trả `ok` tại bước này.
- Chưa thao tác trình duyệt đồ họa/responsive; chưa chạy toàn bộ console suite hoặc concurrency. Kết quả HTTP này không thay thế toàn bộ bộ test.
