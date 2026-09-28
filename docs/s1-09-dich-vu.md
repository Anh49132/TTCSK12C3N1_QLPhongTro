# S1-09 — dịch vụ, lịch sử đơn giá và trạng thái

## Trạng thái bàn giao (28/09/2026)

Đã viết mã AC1–AC4 và build thành công. Người dùng đã chạy app với DB demo; agent đã xác minh thành công các luồng chính qua HTTP thật, xem [dữ liệu demo và kết quả](s1-09-demo.md). Chưa nghiệm thu toàn bộ: console suite vẫn bị Application Control chặn trong môi trường agent; chưa kiểm tra concurrency hoặc trình duyệt đồ họa. Giá thương mại và bảng đơn vị/cách tính vẫn chờ PO chốt.

Người dùng đã chốt:

- Trùng ngày hiệu lực thì từ chối; giá mới áp dụng từ ngày chọn; hóa đơn giữ snapshot.
- Ngừng/kích hoạt lại từ kỳ tháng sau, giữ nguyên thông tin hợp đồng/hóa đơn cũ.
- **Chỉ chuẩn bị mã/script và dùng DB kiểm thử. Không áp dụng schema hoặc ghi vào SQLite gốc.**

## Chức năng đã viết

- `/DichVu`: thêm dịch vụ với ba cách tính, danh sách theo tòa của chủ nhà, định dạng tiền Việt Nam, kiểm tra thiếu dữ liệu/đơn giá âm, phân quyền backend và anti-forgery.
- Lần mở danh sách đầu tiên tự gửi POST có anti-forgery để khởi tạo năm dịch vụ. Trình duyệt không có JavaScript hiển thị nút khởi tạo. Marker theo tòa và transaction/unique ngăn khởi tạo lặp; tòa khác có bảng giá độc lập. Dịch vụ mặc định đã chủ động xóa không tự tạo lại.
- Năm mã mặc định: `DIEN`, `NUOC`, `RAC`, `GUI_XE`, `INTERNET`. Bảng giá chưa được chốt hiển thị **Chưa thiết lập**, bị loại khỏi danh sách chọn khi lập hóa đơn; không coi giá nội bộ 0 là dịch vụ miễn phí đã được xác nhận.
- `/DichVu/Manage`: sửa trực tiếp giá ban đầu của dịch vụ mặc định chỉ khi chưa có nhiều phiên bản và chưa được hợp đồng/hóa đơn tham chiếu. Giá điện/nước mặc định phải >0; các dịch vụ khác cho phép 0.
- Thêm giá mới cùng ngày hiệu lực, đóng khoảng cũ tại ngày trước đó; lịch sử hiển thị giá, khoảng ngày và trạng thái. Trùng ngày bị từ chối. Bản ghi lịch sử cũ không bị ghi đè đơn giá.
- Ngừng/kích hoạt lại tạo phiên bản trạng thái từ ngày đầu tháng, sớm nhất kỳ sau và sau phiên bản mới nhất. Giá và thông tin hợp đồng/hóa đơn đã lưu vẫn giữ nguyên.
- Xóa chỉ trong phạm vi tòa của chủ nhà, khi không có cấu hình phòng riêng hoặc tham chiếu hợp đồng/hóa đơn. Dịch vụ dùng rồi được hướng dẫn ngừng áp dụng. Danh mục mặc định dùng chung được giữ, chỉ xóa cấu hình của tòa đang chọn.

## Hóa đơn và tham chiếu hợp đồng

- Từ danh sách dịch vụ chọn **Lập và xem hóa đơn**, vào `/HoaDonDichVu?toaNhaId=<id>`; yêu cầu quyền tài chính trong DB.
- Chọn ngày áp dụng để xem đơn giá tương ứng, chọn hợp đồng, số người, các dịch vụ và chỉ số đầu/cuối, rồi phát hành.
- Luồng tối thiểu chỉ hỗ trợ hợp đồng đang hiệu lực, thuê **trọn tháng với một giá phòng**. Tự thêm tiền phòng từ kỳ hợp đồng, cộng các dịch vụ được chọn. Từ chối kỳ lẻ, trả phòng/gia hạn giữa tháng hoặc phòng có cấu hình giá riêng. Chưa làm toàn bộ backlog hóa đơn, thanh toán, email, hủy/thay thế hoặc tiền phòng theo ngày.
- Theo chỉ số: cuối − đầu; theo người: số người nhập (không vượt sức chứa); cố định: 1 phòng. Số người/chỉ số cần chủ nhà đối chiếu thực tế; chưa tự lấy từ module chốt chỉ số/người ở ghép. Chỉ số tối đa 3 chữ số thập phân, không âm, cuối ≥ đầu.
- Mỗi dòng dùng decimal, làm tròn tới đồng theo `AwayFromZero`; cộng với kiểm tra tràn số. Ngày áp dụng giá lưu ở `ngay_chot`, ngày thao tác lưu UTC ở `ngay_lap`/`ngay_phat_hanh`; hạn thanh toán 7 ngày từ ngày phát hành thực tế tại Việt Nam.
- Snapshot tên, cách tính, đơn vị, đơn giá, số lượng, chỉ số và thành tiền nằm ở `chi_tiet_hoa_don`. Trang chi tiết đọc snapshot, không lấy lại giá hiện hành. Trigger bảo vệ hóa đơn/dòng đã phát hành; unique ngăn hóa đơn lặp hợp đồng/tháng. Giao dịch phát hành kiểm tra lại giá đang áp dụng và giá đã hiển thị trên form.
- `HoaDonDichVuService.GanVaoHopDongAsync(...)` là điểm tích hợp cho module hợp đồng, ghi snapshot ở `hop_dong_dich_vu`. Chưa có màn hình quản lý toàn bộ hợp đồng; dữ liệu hợp đồng demo chỉ nằm trong fixture kiểm thử.
- Kiểm tra xóa đọc cả `hop_dong_dich_vu`, `chi_tiet_hoa_don` và snapshot cũ ở `ky_hop_dong.thong_tin_chot`. Nhận diện JSON `{"dich_vu":[{"dich_vu_id":123}]}`. Snapshot không rõ cấu trúc hoặc schema không tương thích sẽ từ chối xóa/sửa trực tiếp để chờ đối chiếu, thay vì đoán là chưa tham chiếu.

## Quy tắc cần PO xác nhận thêm

- Phạm vi cấu hình theo tòa; giá mới theo thứ tự thời gian tăng dần, không hồi tố và không chèn trước phiên bản mới nhất. Ngày trùng luôn bị từ chối. Ngày Việt Nam dùng UTC+7.
- Bảng đơn vị/cách tính sau là **đề xuất**, chưa phải PO đã chốt:

| Dịch vụ | Cách tính | Đơn vị | Giá khởi tạo |
|---|---|---|---|
| Điện | Theo chỉ số | kWh | Chờ PO |
| Nước | Theo chỉ số | m³ | Chờ PO |
| Rác | Theo đầu người | người/tháng | Chờ PO |
| Gửi xe | Cố định theo phòng | phòng/tháng | Chờ PO |
| Internet | Cố định theo phòng | phòng/tháng | Chờ PO |

- Người quản trị có thể cấu hình đủ năm phần tử tại `DichVuMacDinh:DanhSach` (các thuộc tính `Ma`, `Ten`, `CachTinh`, `DonVi`, `DonGia`). Không có cấu hình thì dùng đề xuất trên với `DonGia=null`. Chỉ áp dụng khi khởi tạo tòa mới, không ghi đè giá chủ nhà đã sửa. Ví dụ giá 3.000→3.500 chỉ là dữ liệu kiểm thử, không tự gán vào DB gốc.
- Chưa thêm UI đổi cách tính/đơn vị của dịch vụ đã dùng; cần chốt quy tắc riêng nếu mở rộng.

## Thành phần đã thay đổi

- Dịch vụ: `Models/DichVu.cs`, `ViewModels/DichVuViewModels.cs`, `QuanLyDichVuViewModels.cs`, `Services/DichVuService*.cs`, `DichVuMacDinhOptions.cs`, `DichVuThamChieu.cs`, `Controllers/DichVuController.cs`, `Views/DichVu/*`.
- Hóa đơn: `Models/HoaDonDichVu.cs`, `ViewModels/HoaDonDichVuViewModels.cs`, `Services/HoaDonDichVuService.cs`, `Controllers/HoaDonDichVuController.cs`, `Views/HoaDonDichVu/*`.
- Tích hợp: `Data/AppDbContext.cs`, `Data/DichVuSchemaInitializer.cs`, `Program.cs`, project app, menu `_Layout.cshtml`, `.gitignore`.
- SQL: `docs/sql/S1-09-dich-vu.sql`, `S1-09-hoa-don.sql`. Bộ kiểm thử riêng: `verification/S109/`.
- Giữ nguyên thay đổi local có sẵn ở `Tests/QL_PhongTro.Tests/QL_PhongTro.Tests.csproj`; không commit/push.

## CSDL và cách chạy

### Chuẩn bị demo thủ công

Từ gốc repo chạy `dotnet run --project verification/S109/S109.csproj -- . --prepare-demo`. Chế độ này tạo DB kiểm thử riêng có chủ nhà, tòa, phòng 2.000.000đ/tháng và hợp đồng 24 tháng; in lệnh chạy app và tài khoản, lưu vào `demo-access.txt` trong thư mục đã gitignore. Người dùng đã tạo fixture và chạy app; bộ dữ liệu đang dùng được bổ sung theo [hướng dẫn demo](s1-09-demo.md). Không sửa DB gốc.

Đã kiểm tra schema SQLite gốc bằng kết nối chỉ đọc ở bước AC1: chỉ có `tai_khoan` và `sqlite_sequence`, chưa có các bảng nghiệp vụ cần cho demo. DBML là tham chiếu, không phải schema đã được áp dụng.

Không chạy app trực tiếp vào DB gốc: startup **có sẵn từ trước** gọi initializer auth/phòng và `EnsureCreated`. Không thêm initializer dịch vụ/hóa đơn vào startup. DB gốc không bị thay đổi trong task này.

```powershell
# Từ thư mục gốc repo:
dotnet build verification/S109/S109.csproj
dotnet run --project verification/S109/S109.csproj --no-build --no-restore -- .
```

Bộ kiểm thử tạo bản sao SQLite trong thư mục mới `data/S1-09-verification/`, chuẩn bị schema/tài khoản/tòa/hợp đồng chỉ trong bản sao, chạy service/HTTP và đối chiếu hash DB gốc. DLL bị chặn trong môi trường agent; người dùng đã tạo được fixture demo từ terminal của mình.

Hai lệnh riêng dưới đây chỉ dùng cho **DB kiểm thử đã kiểm tra schema tiên quyết**:

```powershell
dotnet run --project QL_PhongTro/QL_PhongTro.csproj --no-build -- --DatabasePath="D:\duong-dan\test.sqlite" --initialize-services
dotnet run --project QL_PhongTro/QL_PhongTro.csproj --no-build -- --DatabasePath="D:\duong-dan\test.sqlite" --initialize-service-invoices
dotnet run --project QL_PhongTro/QL_PhongTro.csproj --no-build -- --DatabasePath="D:\duong-dan\test.sqlite"
```

- Lệnh đầu cần `tai_khoan`, `toa_nha`, `phong_tro`; lệnh sau cần thêm dịch vụ, `hop_dong`, `ky_hop_dong`. Để đăng nhập/menu hoạt động còn cần schema auth/phân quyền/password của task trước.
- Initializer kiểm tra bảng tiên quyết, integrity, sao lưu `.before-s109-<timestamp>.bak`, áp dụng trong transaction và kiểm tra khóa ngoại; không tự tạo các bảng tiên quyết, không ghi đè bảng đã có.
- Script dịch vụ hiện là bản tổng hợp AC1–AC4, thêm `da_chot_gia`, marker khởi tạo và trigger chống chồng ngày. Nếu nơi khác đã áp dụng bản SQL AC1 cũ, cần đối chiếu và viết migration bổ sung trước, không chạy lại script tạo bảng. DB gốc trong workspace chưa áp dụng bản AC1.
- `hop_dong_dich_vu` là bảng bổ sung để lưu tham chiếu/snapshot; không sửa hợp đồng cũ. Hai ID tích hợp tùy chọn `chi_so_id`, `bao_hong_id` được chừa trong chi tiết hóa đơn, chưa thêm FK tới các module chưa có.

## Xác minh thực tế

- Build app và bộ kiểm thử thành công, 0 lỗi; còn cảnh báo giấy phép ImageSharp và hai CS8601 có sẵn ở AuthController.
- Console suite vẫn bị Application Control chặn `S109.dll` trong môi trường agent. Sau khi người dùng chạy app, đã kiểm tra HTTP thật: khởi tạo/lưu giá/validation, hóa đơn snapshot, ngày hiệu lực/trùng ngày, xóa có/chưa có tham chiếu, ngừng/kích hoạt và danh sách chọn hóa đơn đều đạt các kịch bản ghi ở [s1-09-demo.md](s1-09-demo.md).
- Script kiểm tra SQL riêng bị PowerShell Execution Policy chặn ở bước AC1; chưa chạy lại hoặc thay đổi chính sách của máy.
- Chưa kiểm thử trình duyệt đồ họa, responsive hoặc concurrency. Các ca C# đã viết vẫn cần chạy toàn bộ; không suy rộng kết quả HTTP thành toàn bộ suite đã đạt.
- Diff phần task không có lỗi khoảng trắng; project test local có sẵn vẫn có dòng trắng cuối tệp, không sửa.
- SHA-256 DB gốc giữ nguyên: `AD8C98CAB3B6980C953183FD2804D5CCB2B7A5FA46FBB02D05A1EB164FF42347`.

## Bước tiếp theo

1. PO cung cấp giá ban đầu và xác nhận bảng cách tính/đơn vị, cùng các giả định còn lại.
2. Cho phép chạy assembly tin cậy qua quy trình quản trị Windows phù hợp; chạy bộ kiểm thử trên DB riêng và sửa các lỗi phát hiện. Chưa nên coi build thành công là đạt nghiệm thu.
3. Demo trình duyệt trên fixture: năm mặc định → thiết lập giá → phát hành hóa đơn 3.000 → thêm giá 3.500 kỳ sau → kiểm tra hóa đơn cũ → chặn xóa → ngừng và kích hoạt lại.
4. Chỉ chốt AC sau khi kiểm thử/demo thực tế; không áp dụng DB gốc nếu chưa có yêu cầu mới.

