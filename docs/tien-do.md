# Tiến độ dự án

## Cập nhật gần nhất

- Workspace đang ở nhánh `Feature_S1-01_SignIn`.
- Đã triển khai S1-01 trong phạm vi development/test với SQLite cục bộ dự kiến tại `data/local-dev.sqlite`; ứng dụng sẽ tạo file khi chạy lần đầu. File này đã được thêm vào `.gitignore` và không dùng cho production.
- Đã thêm model `TaiKhoan`, `AppDbContext`, ánh xạ bảng `tai_khoan`, cookie authentication và khởi tạo DB bằng `EnsureCreated()` khi chạy ứng dụng.
- Đã thêm biểu mẫu đăng ký gồm họ tên, email, số điện thoại và mật khẩu tại `/Account/Register`.
- Đã triển khai kiểm tra số điện thoại `^0[0-9]{9}$`, mật khẩu tối thiểu 8 ký tự có chữ cái và chữ số; kiểm tra phía client khi rời trường và khi gửi, đồng thời kiểm tra lại phía server.
- Mật khẩu được băm bằng BCrypt trước khi lưu; vai trò được gán cứng `KHACH_THUE` ở server; đăng ký thành công sẽ tạo cookie đăng nhập và chuyển về trang Home.
- Đã thêm các package EF Core SQLite, cookie authentication và BCrypt.Net-Next.
- Chưa kiểm tra trùng email/số điện thoại theo phạm vi hiện tại của story.
- Chưa xác nhận build/runtime bằng công cụ môi trường hiện tại vì lệnh build không khả dụng; cần build và chạy thử trực tiếp trong Visual Studio.

## Phạm vi và lưu ý

- Task: S1-01 — đăng ký tài khoản Khách thuê và tự đăng nhập sau khi đăng ký.
- Trạng thái: đã triển khai mã nguồn; đang chờ build và kiểm thử thực tế.
- Giả định UI nếu triển khai: kiểm tra số điện thoại/mật khẩu khi rời trường và khi gửi biểu mẫu; hiển thị lỗi ngay dưới trường tương ứng. Nội dung cần thống nhất với PO.
- Ngoài phạm vi hiện tại: kiểm tra trùng email/số điện thoại theo yêu cầu demo; không tạo bảng hồ sơ `khach_thue`.
- Còn lại: build project, chạy ứng dụng, kiểm tra luồng đăng ký hợp lệ/không hợp lệ và xác nhận cột `mat_khau` trong SQLite chứa BCrypt hash thay vì mật khẩu rõ.
