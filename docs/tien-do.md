# Tiến độ dự án

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
