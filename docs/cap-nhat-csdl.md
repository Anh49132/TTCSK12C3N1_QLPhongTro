# Đồng bộ cấu trúc SQLite trong team

Mỗi người giữ dữ liệu SQLite riêng. Pull mã nguồn không cập nhật file SQLite của máy khác. Không chép đè CSDL của đồng đội và không dùng EnsureDeleted/EnsureCreated để nâng cấp dữ liệu cũ.

**S2-01 bỏ database khỏi Git:** trước lần pull nhận thay đổi này, dừng app và sao lưu database local ra ngoài repository, vì Git có thể xóa file trước đây được theo dõi. Khôi phục bản của chính mình nếu cần, không ghi đè file đang tồn tại. Máy mới dùng `--initialize-database` (từ chối file đã tồn tại), rồi tạo ADMIN qua cấu hình riêng; xem [README](../README.md).

## Sau khi pull

Dừng ứng dụng trước khi cập nhật. Chạy từ thư mục gốc repository:

```powershell
dotnet restore QL_PhongTro
dotnet run --project QL_PhongTro -- --update-database
dotnet run --project QL_PhongTro -- --check-database
dotnet run --project QL_PhongTro --launch-profile http
```

Mở http://localhost:5247/Account/Login. Nếu dùng file khác, đặt `$env:DatabasePath = 'C:\duong-dan\database.sqlite'` trong cùng terminal trước các lệnh trên. Đường dẫn phải trỏ file đã tồn tại; công cụ không tạo lại CSDL nền.

`--check-database` chỉ đọc schema. Web kiểm tra bảng/cột trước khi nhận request; nhánh S2-01 yêu cầu cập nhật v6 bằng updater trước khi chạy web. Không cần chạy SQL/lệnh sửa dữ liệu mỗi lần xóa tài khoản.

## Các phiên bản cập nhật

Phiên bản hiện tại là **6**. `RoomServicesSchema` và `sql/S2-01-dich-vu-phong.sql` bổ sung danh mục tòa có cờ mặc định (`dich_vu_toa_nha`) và lựa chọn phòng (`dich_vu_phong`), khóa ngoại/unique và trigger chặn gán dịch vụ khác tòa. Không lưu giá riêng. Nếu chưa có S1-09, cài ba bảng dịch vụ từ script đã có trong cùng transaction; schema S1-09 không đầy đủ bị từ chối. Updater kiểm tra chỉ đọc trước khi ghi, tạo backup, kiểm tra integrity/FK và ghi version trong transaction. Chạy lại không đổi dữ liệu.

Các dịch vụ cấp tòa đã có được đưa vào danh mục mới; năm mã gợi ý DIEN/NUOC/RAC/GUI_XE/INTERNET được bật mặc định, dịch vụ khác tắt. Phòng cũ không tự nhận lựa chọn; chủ nhà cấu hình trên trang dịch vụ phòng. Giá/lịch sử cũ và hóa đơn không đổi. Mặc định chỉ được lấy khi tạo phòng mới. Không cài thêm schema hợp đồng/hóa đơn.

v5: `AccountReuseSchema` kiểm tra phiên bản, định nghĩa hai index UNIQUE và khóa ngoại; sao lưu `*.before-account-reuse-<id>.bak`, rồi thay hai index bằng UNIQUE có điều kiện `WHERE is_deleted = 0`. Chỉ tài khoản chưa xóa (kể cả đang khóa) giữ chỗ email/số điện thoại. ID và thông tin liên hệ cũ được giữ nguyên. Updater giữ nguyên bước này; ở v5/v6 chỉ kiểm tra. Schema/index lạ bị từ chối, không tự dựng lại bảng. Cần quyền ghi DB và tạo backup cạnh DB.

v5 đồng thời vô hiệu hóa phiên/reset của tài khoản đã xóa từ trước và chuyển tòa của Quản lý đã xóa về chưa phân công. Toàn bộ bước nâng cấp nằm trong transaction; không xóa tài khoản, hồ sơ, hợp đồng hoặc nhật ký. Đây là sửa dữ liệu hạ tầng khi nâng cấp, không giả lập một ADMIN để ghi nhật ký nghiệp vụ. Xóa mới qua web vẫn ghi audit cùng transaction với bỏ phân công và thu hồi phiên.

v4 bổ sung `email_confirmed`, `is_deleted` và `email_confirmation`. v3 (S1-10) bổ sung/kiểm tra nhật ký hoạt động, snapshot tên người thực hiện và trigger chặn sửa/xóa. Bước v3 kiểm tra schema, integrity/FK và ghi phiên bản trong cùng transaction; không suy đoán tên cho nhật ký cũ thiếu snapshot.

Phiên bản 2 (S1-03) bổ sung `tai_khoan.must_change_password` mặc định false và unique index email chuẩn hoá/số điện thoại. Các tài khoản cũ không tự bị ép đổi mật khẩu. Nếu dữ liệu trùng, phiên bản 2 rollback và báo lỗi để kiểm tra; không tự gộp/xoá. Phạm vi chức năng và kiểm thử được tổng hợp trong [tiến độ dự án](tien-do.md).

`Data/DatabaseUpdates.cs` gom các bước auth, phòng, phân quyền, mật khẩu/phiên và bảng hồ sơ đã có trong code/SQL hiện tại. Không triển khai toàn bộ 22 bảng tham chiếu. Quan hệ thuê chưa có vẫn được xử lý theo cơ chế giới hạn quyền của HoSoAccess.

- Backup SQLite nhất quán được lưu cạnh DB dưới tên `*.before-update-<id>.bak`; console in đường dẫn. Các initializer cũ còn tạo backup riêng.
- Bảng `app_schema_version` ghi phiên bản sau khi tất cả bước và kiểm tra hoàn tất. Chạy lại phiên bản đã hoàn thành không sửa dữ liệu hoặc cấp lại quyền đã thu hồi.
- Không xóa tài khoản/dữ liệu cũ. Bảng đã tồn tại nhưng thiếu cột ngoài auth không được tự sửa: phải kiểm tra và viết bản nâng cấp phù hợp.
- Các bước cũ chưa nằm trong một transaction chung. Nếu lỗi giữa chừng, một số bước có thể đã hoàn tất; phiên bản chưa được ghi nhận. Dừng app, đọc lỗi, đối chiếu backup rồi xử lý và chạy lại. Không tự phục hồi đè lên dữ liệu mới phát sinh.
- Không chạy đồng thời ứng dụng và updater. Khóa `.update.lock` chỉ ngăn hai updater chạy cùng lúc; nó không khóa các công cụ quản trị/phiên web khác.
- Kiểm tra hiện tại xác nhận bảng/cột cần cho model và các bảng mật khẩu, cùng sự hiện diện dữ liệu phân quyền; không chứng minh toàn bộ kiểu dữ liệu, FK/index hoặc toàn bộ nghiệp vụ đã đúng.

## Quy tắc cho sprint sau

PR thay đổi schema phải kèm bước cập nhật mới, tăng phiên bản và tài liệu sử dụng. Giữ nguyên các bước đã phát hành; bổ sung bước theo thứ tự, chỉ áp dụng phiên bản chưa chạy. Bản nâng cấp phải kiểm tra schema cũ, sao lưu và bảo toàn dữ liệu; thay đổi phá hủy cần quyết định riêng.

Trước khi merge: build, kiểm thử nâng cấp từ CSDL phiên bản trước trên bản sao, đối chiếu dữ liệu cũ, kiểm tra chạy lặp lại và chạy web. Một thành viên khác cần thử quy trình pull/cập nhật trên máy của họ. Không commit DB, backup hoặc dữ liệu cá nhân; DB đã được Git theo dõi từ trước vẫn cần kiểm tra `git status` trước khi commit.

Kiểm chứng tự động hiện tại (Python 3, .NET 10, sau build):

```powershell
python verification/s201_database.py
```

Script chỉ thao tác bản sao tạm: backup, giữ dữ liệu, integrity/FK, chạy lại không đổi dữ liệu; khởi tạo file mới không có tài khoản, từ chối ghi đè và xác nhận hash nguồn không đổi. Bộ xUnit hiện dùng database tạm khởi tạo từ script, không phụ thuộc database local. Công cụ `verification/database_updates.py` và fixture S109 lịch sử chưa được cập nhật/nghiệm thu cho v6.
