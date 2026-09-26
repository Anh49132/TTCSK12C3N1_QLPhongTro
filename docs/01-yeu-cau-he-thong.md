# Yêu cầu hệ thống — nội dung trích từ Excel

Nguồn: 1. Hệ thống cho thuê phòng trọ.xlsx. Gồm nội dung các ô có dữ liệu của cả 5 trang tính.
Giữ địa chỉ ô để đối chiếu. Định dạng Excel không được tái tạo; công thức nếu có được ghi kèm giá trị trích.
Đọc 00-huong-dan.md trước. Tài liệu này lưu nội dung nghiệp vụ trích từ Excel; lựa chọn công nghệ hiện hành đã được cập nhật theo hướng ASP.NET Core MVC + HTML/CSS + SQLite hiện có.

## Trang tính: 1. Product Overview

### Dòng 1

**A1**

HỆ THỐNG QUẢN LÝ CHO THUÊ PHÒNG TRỌ VÀ CĂN HỘ

### Dòng 2

**A2**

Product Backlog · Dự án thực tập · 4 Sprint × 1 tuần

### Dòng 4

**A4**

THÔNG SỐ DỰ ÁN

### Dòng 5

**A5**

Thông số

**B5**

Giá trị

**C5**

Ghi chú

### Dòng 6

**A6**

Thời gian

**B6**

4 tuần

**C6**

4 sprint × 1 tuần, làm full-time

### Dòng 7

**A7**

Số sprint

**B7**

4

**C7**

Sprint 1 nền tảng, Sprint 2–3 thân luồng, Sprint 4 đóng luồng và nghiệm thu

### Dòng 8

**A8**

Velocity mục tiêu

**B8**

42–43 point/sprint

**C8**

Đội mới, tuần đầu mất thời gian dựng môi trường nên không đặt cao hơn

### Dòng 9

**A9**

Tổng story point

**B9**

170 point

**C9**

Tương đương khoảng 680 giờ công cho cả đội

### Dòng 10

**A10**

Số user story

**B10**

40 story

**C10**

Mỗi sprint 10 story

### Dòng 11

**A11**

Số epic

**B11**

6 epic

**C11**

EP-01 đến EP-06

### Dòng 12

**A12**

Đội ngũ

**B12**

5 lập trình viên fullstack

**C12**

Không có tester riêng, thành viên tự kiểm thử chéo

### Dòng 13

**A13**

Quy đổi point

**B13**

1 point ≈ 4 giờ công

**C13**

Thang Fibonacci 1, 2, 3, 5, 8; story lớn hơn 8 phải tách

### Dòng 15

**A15**

VẤN ĐỀ NGHIỆP VỤ

### Dòng 16

**A16**

Một chủ nhà có vài chục phòng trọ hiện đang quản lý bằng sổ tay và một file Excel chép lại mỗi tháng. Chỉ số điện nước ghi trên giấy rồi nhắn qua Zalo cho chủ nhà, tiền phòng thu bằng tiền mặt hoặc chuyển khoản không có nội dung thống nhất. Đến kỳ thu tiền, chủ nhà phải mở lại tin nhắn Zalo và soi sao kê ngân hàng để đoán xem phòng nào đã trả, phòng nào trả thiếu. Tin đăng phòng trống thì rải rác trên nhiều nhóm Facebook, khách gọi hỏi phòng đã có người thuê từ tuần trước. Khi khách trả phòng, số tiền cọc còn lại bao nhiêu sau khi trừ hư hỏng gần như là chuyện thương lượng miệng, không ai giữ được bằng chứng.

### Dòng 17

**A17**

#

**B17**

Hệ quả trực tiếp

### Dòng 18

**A18**

1.0

**B18**

Hoá đơn tháng tính bằng máy tính bỏ túi nên hay sai đơn giá hoặc sai chỉ số, khách phát hiện rồi tranh cãi, chủ nhà phải tính lại và mất uy tín.

### Dòng 19

**A19**

2.0

**B19**

Không biết chính xác tại một thời điểm có bao nhiêu phòng đang nợ tiền và nợ bao nhiêu, vì công nợ chỉ nằm trong trí nhớ và vài dòng ghi chú.

### Dòng 20

**A20**

3.0

**B20**

Tin đăng không đồng bộ với thực tế: phòng đã cho thuê vẫn còn tin, phòng vừa trống thì hai tuần sau mới có người đăng lại, mỗi phòng trống một tháng là mất trọn một tháng tiền thuê.

### Dòng 21

**A21**

4.0

**B21**

Khách báo hỏng bình nóng lạnh qua Zalo, tin nhắn trôi đi, hai tuần sau vẫn chưa ai sửa và không ai biết ai đang chịu trách nhiệm.

### Dòng 22

**A22**

5.0

**B22**

Khi trả phòng, không có bản ghi chỉ số cuối kỳ và tình trạng thiết bị lúc nhận phòng nên việc trừ cọc luôn kết thúc bằng cãi vã, có trường hợp khách bỏ đi không nhận lại cọc.

### Dòng 24

**A24**

TẦM NHÌN SẢN PHẨM

### Dòng 25

**A25**

CHO chủ nhà và người quản lý các khu trọ, căn hộ cho thuê quy mô vài chục đến vài trăm phòng,
HỆ THỐNG QUẢN LÝ CHO THUÊ PHÒNG TRỌ LÀ một ứng dụng web quản lý phòng, hợp đồng, hoá đơn và công nợ
GIÚP đưa toàn bộ vòng đời từ đăng tin, nhận khách, chốt điện nước, phát hành hoá đơn đến thu tiền và trả phòng vào một nơi duy nhất có số liệu tra cứu được,
KHÁC VỚI cách ghi sổ tay, nhắn Zalo và gõ lại Excel hằng tháng ở chỗ mọi con số đều gắn với một hợp đồng cụ thể, có lịch sử chỉnh sửa và tra lại được sau nhiều tháng.

### Dòng 27

**A27**

MỤC TIÊU & THƯỚC ĐO SAU 4 SPRINT

### Dòng 28

**A28**

Mục tiêu

**B28**

Thước đo thành công

**D28**

Đo bằng

### Dòng 29

**A29**

Chạy trọn một luồng nghiệp vụ end-to-end trên hệ thống

**B29**

Từ đăng tin phòng trống, khách gửi yêu cầu thuê, chủ nhà lập hợp đồng, chốt điện nước, phát hành hoá đơn, khách thanh toán và chủ nhà xác nhận đã thu, hoàn thành trọn vẹn trên web trong buổi nghiệm thu, không thao tác tay ngoài hệ thống

**D29**

Kịch bản demo cuối Sprint 4, chạy thật trên môi trường staging

### Dòng 30

**A30**

Rút ngắn thời gian lập hoá đơn tháng

**B30**

Phát hành hoá đơn cho một toà 30 phòng trong dưới 15 phút kể từ khi nhập xong chỉ số, thay vì nửa buổi gõ Excel

**D30**

Bấm giờ thao tác thực tế trên dữ liệu mẫu 30 phòng

### Dòng 31

**A31**

Nhìn thấy công nợ mà không phải cộng tay

**B31**

Màn hình công nợ hiển thị đúng tổng số tiền còn thiếu và danh sách hoá đơn quá hạn, đối chiếu khớp 100% với dữ liệu kiểm thử gồm ít nhất 5 trường hợp trả thiếu

**D31**

Bộ dữ liệu kiểm thử công nợ và biên bản đối chiếu

### Dòng 32

**A32**

Giảm thời gian phòng để trống

**B32**

Hợp đồng kết thúc thì phòng về trạng thái trống và tin đăng được bật lại trong cùng thao tác, không quá 1 phút, không cần đăng tin thủ công lại

**D32**

Kiểm thử luồng trả phòng trên hệ thống

### Dòng 33

**A33**

Mọi tranh chấp tiền đều có bản ghi để đối chiếu

**B33**

Mỗi hoá đơn tra được chỉ số đầu kỳ, cuối kỳ, đơn giá áp dụng và người nhập; mỗi lần xác nhận thu tiền có người thao tác và thời điểm

**D33**

Nhật ký hoạt động và trang chi tiết hoá đơn

### Dòng 35

**A35**

PHẠM VI

### Dòng 36

**A36**

TRONG PHẠM VI (In-scope)

**C36**

NGOÀI PHẠM VI (Out-of-scope)

### Dòng 37

**A37**

• Quản lý tài khoản và phân quyền cho bốn vai trò: khách thuê, chủ nhà, quản lý toà nhà, quản trị hệ thống

**C37**

• Cổng thanh toán online thật và quét mã VietQR tự động đối soát với sao kê ngân hàng

### Dòng 38

**A38**

• Khai báo toà nhà, khu trọ, phòng, diện tích, giá thuê và trạng thái phòng

**C38**

• Ký số hoặc ký điện tử hợp đồng thuê có giá trị pháp lý

### Dòng 39

**A39**

• Khai báo dịch vụ kèm theo và đơn giá: điện, nước, rác, gửi xe, internet, phí quản lý

**C39**

• Ứng dụng mobile native cho iOS và Android

### Dòng 40

**A40**

• Đăng tin cho thuê từ phòng trống, trang tìm kiếm và trang chi tiết tin cho khách xem

**C40**

• Tích hợp đẩy tin sang Chợ Tốt, Batdongsan hay các sàn rao vặt bên thứ ba

### Dòng 41

**A41**

• Khách gửi yêu cầu thuê hoặc đặt lịch xem phòng, chủ nhà duyệt, hẹn lịch hoặc từ chối

**C41**

• Khai báo tạm trú tạm vắng và kết nối dữ liệu với cơ quan công an

### Dòng 42

**A42**

• Lập hợp đồng thuê ghi nhận tiền cọc, ngày bắt đầu, kỳ hạn, giá thuê và danh sách người ở ghép

**C42**

• Kế toán thuế, xuất hoá đơn điện tử theo chuẩn cơ quan thuế và sổ sách kế toán

### Dòng 43

**A43**

• Ghi chỉ số điện nước đầu kỳ và cuối kỳ, kiểm tra chỉ số mới không nhỏ hơn chỉ số cũ

**C43**

• Đồng hồ điện nước IoT đọc chỉ số tự động hoặc nhận diện chỉ số từ ảnh chụp bằng AI

### Dòng 44

**A44**

• Phát hành hoá đơn hằng tháng gồm tiền phòng, điện, nước, dịch vụ cố định, có hạn thanh toán

**C44**

• Chấm điểm tín nhiệm khách thuê và gợi ý giá thuê theo thị trường

### Dòng 45

**A45**

• Ghi nhận thanh toán kể cả thanh toán một phần, theo dõi công nợ và cảnh báo hoá đơn quá hạn

### Dòng 46

**A46**

• Gia hạn, chấm dứt hợp đồng trước hạn, trả phòng và tất toán tiền cọc có trừ hư hỏng

### Dòng 47

**A47**

• Khách báo hỏng thiết bị, quản lý toà nhà tiếp nhận và cập nhật tiến độ xử lý; báo cáo doanh thu và tỉ lệ lấp đầy

### Dòng 49

**A49**

NỀN TẢNG KỸ THUẬT

### Dòng 50

**A50**

Tầng

**B50**

Lựa chọn

**C50**

Lý do chọn cho đội thực tập

### Dòng 51

**A51**

Frontend

**B51**

HTML5 + CSS3 trong Razor Views; JavaScript khi cần

**C51**

Giao diện server-rendered, phù hợp yêu cầu dùng HTML/CSS và không cần framework frontend riêng

### Dòng 52

**A52**

Backend

**B52**

ASP.NET Core MVC (C#)

**C52**

Một ứng dụng MVC phục vụ giao diện Razor và xử lý nghiệp vụ ở backend; phù hợp làm lại hệ thống mà không thay đổi yêu cầu chức năng

### Dòng 53

**A53**

Cơ sở dữ liệu

**B53**

SQLite hiện có

**C53**

Giữ nguyên CSDL và dữ liệu của hệ thống cũ; ánh xạ schema hiện tại, không tạo lại hoặc tự ý thay đổi cấu trúc

### Dòng 54

**A54**

Xác thực

**B54**

ASP.NET Core Cookie Authentication, phân quyền theo vai trò

**C54**

Phù hợp ứng dụng MVC render giao diện trên server; cần đối chiếu cách lưu và băm mật khẩu hiện tại trước khi chuyển tài khoản

### Dòng 55

**A55**

Lưu trữ tệp và ảnh

**B55**

MinIO tương thích S3, ảnh phòng và ảnh chứng từ thanh toán

**C55**

Chạy được bằng Docker trên máy cá nhân, không tốn chi phí dịch vụ đám mây trong kỳ thực tập

### Dòng 56

**A56**

Gửi email hoặc thông báo

**B56**

SMTP qua Gmail App Password, kèm thông báo trong ứng dụng

**C56**

Miễn phí, đủ để gửi thông báo hoá đơn mới và nhắc hạn thanh toán; không dùng SMS vì mất phí

### Dòng 58

**A58**

YÊU CẦU PHI CHỨC NĂNG

### Dòng 59

**A59**

Nhóm

**B59**

Yêu cầu

### Dòng 60

**A60**

Hiệu năng

**B60**

Trang danh sách phòng và danh sách hoá đơn trả kết quả dưới 2 giây với 500 bản ghi; phát hành hoá đơn hàng loạt cho 50 phòng hoàn tất dưới 30 giây

### Dòng 61

**A61**

Quy mô

**B61**

Chịu được 10 toà nhà, 500 phòng, 500 hợp đồng đang hiệu lực và 12 tháng hoá đơn, tương đương khoảng 6.000 hoá đơn; 30 người dùng đồng thời

### Dòng 62

**A62**

Bảo mật

**B62**

Mật khẩu băm bằng BCrypt, khoá tài khoản sau 5 lần đăng nhập sai trong 15 phút, mọi API kiểm tra quyền ở tầng backend chứ không chỉ ẩn nút trên giao diện

### Dòng 63

**A63**

Bảo vệ dữ liệu cá nhân

**B63**

Số căn cước và số điện thoại khách thuê chỉ hiển thị đầy đủ cho chủ nhà của phòng đó và quản trị hệ thống, các vai trò khác thấy dạng che bớt; ảnh chứng từ thanh toán chỉ truy cập được qua liên kết có hạn 15 phút

### Dòng 64

**A64**

Giao diện

**B64**

Responsive từ 360px, khách thuê thao tác được toàn bộ luồng xem tin, gửi yêu cầu, xem hoá đơn và báo hỏng trên điện thoại

### Dòng 65

**A65**

Ngôn ngữ

**B65**

Toàn bộ giao diện tiếng Việt, tiền tệ VND định dạng có dấu chấm ngăn cách nghìn, ngày giờ dạng dd/MM/yyyy HH:mm theo múi giờ Asia/Ho_Chi_Minh

### Dòng 66

**A66**

Sao lưu

**B66**

Sao lưu cơ sở dữ liệu tự động hằng ngày lúc 02:00, giữ 7 bản gần nhất, có kịch bản phục hồi đã thử ít nhất một lần trước khi nghiệm thu

### Dòng 68

**A68**

GIẢ ĐỊNH CẦN PO XÁC NHẬN

### Dòng 69

**A69**

#

**B69**

Giả định

**D69**

Ảnh hưởng nếu sai

### Dòng 70

**A70**

1.0

**B70**

Mỗi phòng chỉ có một hợp đồng đang hiệu lực tại một thời điểm

**D70**

Nếu phải hỗ trợ nhiều hợp đồng song song theo từng giường trong phòng ký túc, cần thêm khoảng 13 point cho mô hình dữ liệu và màn hình quản lý giường

### Dòng 71

**A71**

2.0

**B71**

Đơn giá điện nước là đơn giá phẳng theo đơn vị, không tính theo bậc thang của EVN

**D71**

Nếu phải tính theo bậc thang lũy tiến và đổi bậc giữa kỳ, cần thêm khoảng 8 point cho phần cấu hình bậc và công thức tính

### Dòng 72

**A72**

3.0

**B72**

Kỳ hoá đơn trùng với tháng dương lịch, chốt vào cùng một ngày cho mọi phòng trong toà

**D72**

Nếu mỗi hợp đồng có kỳ chốt riêng theo ngày vào ở, cần thêm khoảng 8 point cho lịch chốt theo hợp đồng và tính tiền phòng theo tỉ lệ ngày

### Dòng 73

**A73**

4.0

**B73**

Thanh toán được ghi nhận thủ công bởi chủ nhà sau khi tự đối chiếu sao kê ngân hàng

**D73**

Nếu phải đối soát tự động với sao kê hoặc cổng thanh toán, cần thêm ít nhất 21 point và thời gian chờ tích hợp bên thứ ba

### Dòng 74

**A74**

5.0

**B74**

Hợp đồng dùng một mẫu thống nhất do chủ nhà cấu hình sẵn, xuất ra PDF để ký giấy

**D74**

Nếu cần trình soạn thảo mẫu hợp đồng có biến động và nhiều mẫu theo từng toà, cần thêm khoảng 8 point

### Dòng 75

**A75**

6.0

**B75**

Hệ thống chỉ phục vụ một tổ chức chủ nhà trong phạm vi thực tập, không phải nền tảng nhiều nhà cung cấp

**D75**

Nếu phải tách dữ liệu đa tổ chức với gói dịch vụ riêng, cần thêm khoảng 13 point cho tầng phân tách dữ liệu và quản lý gói

## Trang tính: 2. User Roles

### Dòng 1

**A1**

USER ROLES — 4 VAI TRÒ

### Dòng 3

**A3**

#

**B3**

Vai trò

**C3**

Mã

**D3**

Là ai trong hệ thống

**E3**

Mục tiêu chính khi dùng hệ thống

### Dòng 4

**A4**

1.0

**B4**

Khách thuê

**C4**

Tenant

**D4**

Người đang thuê hoặc đang tìm phòng, phần lớn là sinh viên và người đi làm trẻ, thao tác chủ yếu trên điện thoại

**E4**

Tìm được phòng phù hợp, gửi yêu cầu thuê, xem rõ hoá đơn hằng tháng gồm những khoản gì và báo hỏng khi thiết bị trong phòng gặp sự cố

### Dòng 5

**A5**

2.0

**B5**

Chủ nhà

**C5**

Landlord

**D5**

Người sở hữu hoặc thuê lại toà nhà để cho thuê, chịu trách nhiệm về giá, hợp đồng và dòng tiền

**E5**

Biết phòng nào trống, phòng nào nợ tiền, lập hợp đồng và phát hành hoá đơn nhanh, thu tiền đúng và đủ

### Dòng 6

**A6**

3.0

**B6**

Quản lý toà nhà

**C6**

Manager

**D6**

Người trông coi tại chỗ, thường là một người cho vài toà, đi ghi chỉ số điện nước và xử lý hỏng hóc

**E6**

Ghi chỉ số nhanh gọn ngay tại hành lang bằng điện thoại và không để sót phòng nào, theo dõi các việc sửa chữa đang tồn

### Dòng 7

**A7**

4.0

**B7**

Quản trị hệ thống

**C7**

Admin

**D7**

Người phụ trách kỹ thuật, cấp tài khoản và xử lý sự cố dữ liệu

**E7**

Cấp và khoá tài khoản đúng vai trò, tra được nhật ký ai đã sửa gì khi có tranh chấp số liệu

### Dòng 9

**A9**

MA TRẬN PHÂN QUYỀN THEO MODULE

### Dòng 10

**A10**

F = toàn quyền | W = ghi trong phạm vi được giao | R = chỉ xem | – = không truy cập | * Chỉ trên dữ liệu của chính mình, tức phòng hoặc hợp đồng mà người dùng đang gắn với

### Dòng 11

**A11**

Module

**C11**

Tenant

**D11**

Landlord

**E11**

Manager

**F11**

Admin

### Dòng 12

**A12**

Tài khoản và phân quyền

**C12**

R*

**D12**

R

**E12**

–

**F12**

F

### Dòng 13

**A13**

Toà nhà, phòng và bảng giá

**C13**

R

**D13**

F

**E13**

R

**F13**

R

### Dòng 14

**A14**

Tin đăng cho thuê

**C14**

R

**D14**

F

**E14**

R

**F14**

R

### Dòng 15

**A15**

Yêu cầu thuê và lịch xem phòng

**C15**

W*

**D15**

F

**E15**

R

**F15**

R

### Dòng 16

**A16**

Hợp đồng thuê và người ở ghép

**C16**

R*

**D16**

F

**E16**

R

**F16**

R

### Dòng 17

**A17**

Chỉ số điện nước

**C17**

R*

**D17**

F

**E17**

W

**F17**

R

### Dòng 18

**A18**

Hoá đơn, thanh toán và công nợ

**C18**

R*

**D18**

F

**E18**

R

**F18**

R

### Dòng 19

**A19**

Báo hỏng và bảo trì

**C19**

W*

**D19**

F

**E19**

W

**F19**

R

### Dòng 20

**A20**

Báo cáo doanh thu và tỉ lệ lấp đầy

**C20**

–

**D20**

F

**E20**

R

**F20**

R

## Trang tính: 3. Epics

### Dòng 1

**A1**

EPICS — 6 NHÓM CHỨC NĂNG

### Dòng 3

**A3**

Mã Epic

**B3**

Tên Epic

**C3**

Mục tiêu / phạm vi

**D3**

Point

**E3**

% tổng

**F3**

Sprint

**G3**

Số story

### Dòng 4

**A4**

EP-01

**B4**

Tài khoản và phân quyền

**C4**

Đăng ký, đăng nhập, hồ sơ cá nhân, cấp và khoá tài khoản theo bốn vai trò, phân quyền menu và nhật ký hoạt động làm căn cứ đối chiếu khi có tranh chấp số liệu.

**D4**

30.0

**E4**

0.1764705882352941

**F4**

1, 4

**G4**

8.0

### Dòng 5

**A5**

EP-02

**B5**

Danh mục toà nhà, phòng và dịch vụ

**C5**

Khai báo toà nhà, phòng, diện tích, giá thuê, ảnh phòng, các dịch vụ kèm theo và đơn giá điện, nước, rác, gửi xe, internet làm dữ liệu gốc cho hợp đồng và hoá đơn.

**D5**

26.0

**E5**

0.1529411764705882

**F5**

1–2

**G5**

6.0

### Dòng 6

**A6**

EP-03

**B6**

Đăng tin cho thuê và yêu cầu thuê

**C6**

Đăng tin từ phòng trống, trang tìm kiếm và chi tiết tin cho khách, khách gửi yêu cầu thuê hoặc đặt lịch xem phòng, chủ nhà xử lý và phản hồi.

**D6**

31.0

**E6**

0.1823529411764706

**F6**

2

**G6**

7.0

### Dòng 7

**A7**

EP-04

**B7**

Hợp đồng thuê và khách đang ở

**C7**

Lập hợp đồng từ yêu cầu đã duyệt với tiền cọc, ngày bắt đầu và giá thuê, quản lý người ở ghép, gia hạn, chấm dứt trước hạn, trả phòng và tất toán cọc.

**D7**

32.0

**E7**

0.1882352941176471

**F7**

3–4

**G7**

7.0

### Dòng 8

**A8**

EP-05

**B8**

Chỉ số điện nước, hoá đơn và thanh toán

**C8**

Chốt chỉ số đầu kỳ và cuối kỳ, phát hành hoá đơn tháng gồm tiền phòng và các khoản dịch vụ, ghi nhận thanh toán kể cả trả thiếu và theo dõi công nợ.

**D8**

35.0

**E8**

0.2058823529411765

**F8**

3–4

**G8**

8.0

### Dòng 9

**A9**

EP-06

**B9**

Báo hỏng, thông báo và báo cáo

**C9**

Khách báo hỏng thiết bị và theo dõi tiến độ xử lý, thông báo hoá đơn mới và nhắc hạn thanh toán, báo cáo doanh thu và tỉ lệ lấp đầy cho chủ nhà.

**D9**

16.0

**E9**

0.09411764705882353

**F9**

4

**G9**

4.0

### Dòng 10

**A10**

TỔNG

**D10**

170.0

**E10**

1.0

**F10**

1–4

**G10**

40.0

## Trang tính: 4. Product Backlog

### Dòng 1

**A1**

v

### Dòng 2

**A2**

Mẫu story: Là <vai trò>, tôi muốn <hành động>, để <giá trị>.   |   Ưu tiên MoSCoW: Must / Should / Could.   |   1 point ≈ 4 giờ công của đội 5 người.

### Dòng 4

**A4**

ID

**B4**

Sprint

**C4**

Epic

**D4**

Vai trò

**E4**

User Story

**F4**

Tiêu chí chấp nhận (AC)

**G4**

Point

**H4**

Ưu tiên

**I4**

Trạng thái

**J4**

Ghi chú / Người thực hiện

### Dòng 5

**A5**

SPRINT 1 — Nền tảng tài khoản và danh mục gốc   ·   10 story · 42 point

### Dòng 6

**A6**

Sprint Goal: Cuối sprint, bốn vai trò đăng nhập được vào hệ thống và chủ nhà đã khai báo xong toà nhà, phòng cùng bảng giá dịch vụ để các sprint sau tham chiếu.

### Dòng 7

**A7**

S1-01

**B7**

1.0

**C7**

EP-01

**D7**

Khách thuê

**E7**

Là Khách thuê, tôi muốn tự đăng ký tài khoản bằng số điện thoại và email, để gửi yêu cầu thuê ngay khi thấy tin phù hợp mà không phải chờ ai cấp tài khoản.

**F7**

• Biểu mẫu gồm họ tên, số điện thoại, email và mật khẩu; số điện thoại đúng định dạng 10 chữ số bắt đầu bằng số 0
• Mật khẩu tối thiểu 8 ký tự, có ít nhất một chữ cái và một chữ số, lưu dưới dạng băm BCrypt
• Trùng số điện thoại hoặc trùng email đã tồn tại thì bị từ chối kèm thông báo chỉ rõ trường nào trùng
• Tài khoản đăng ký thành công được gán vai trò Khách thuê và đăng nhập được ngay

**G7**

5.0

**H7**

Must

**I7**

Chưa bắt đầu

### Dòng 8

**A8**

S1-02

**B8**

1.0

**C8**

EP-01

**D8**

Khách thuê

**E8**

Là Khách thuê, tôi muốn đăng nhập và duy trì phiên làm việc, để không phải nhập lại mật khẩu mỗi lần mở lại trang trên điện thoại.

**F8**

• Đăng nhập bằng số điện thoại hoặc email cùng mật khẩu, trả về access token hạn 30 phút và refresh token hạn 7 ngày
• Sai mật khẩu 5 lần trong 15 phút thì khoá đăng nhập tài khoản đó 15 phút và báo rõ thời gian còn lại
• Thông báo lỗi khi sai thông tin không tiết lộ tài khoản có tồn tại hay không
• Đăng xuất làm mất hiệu lực refresh token, gọi lại API bằng token cũ trả về mã 401

**G8**

3.0

**H8**

Must

**I8**

Chưa bắt đầu

### Dòng 9

**A9**

S1-03

**B9**

1.0

**C9**

EP-01

**D9**

Quản trị hệ thống

**E9**

Là Quản trị hệ thống, tôi muốn tạo và khoá tài khoản cho chủ nhà cùng quản lý toà nhà, để người trông coi nghỉ việc là mất quyền truy cập dữ liệu khách thuê ngay trong ngày.

**F9**

• Tạo tài khoản với họ tên, email, số điện thoại và chọn vai trò trong danh sách Chủ nhà, Quản lý toà nhà, Quản trị hệ thống
• Hệ thống sinh mật khẩu tạm và gửi qua email, người dùng buộc đổi mật khẩu ở lần đăng nhập đầu tiên
• Khoá tài khoản làm mọi phiên đang hoạt động của tài khoản đó hết hiệu lực trong vòng 1 phút
• Danh sách tài khoản lọc được theo vai trò và trạng thái, phân trang 20 dòng một trang

**G9**

5.0

**H9**

Must

**I9**

Chưa bắt đầu

### Dòng 10

**A10**

S1-04

**B10**

1.0

**C10**

EP-01

**D10**

Quản trị hệ thống

**E10**

Là Quản trị hệ thống, tôi muốn phân quyền theo vai trò và ẩn hiện menu tương ứng, để quản lý toà nhà không vào được màn hình công nợ và không thấy giá thuê thoả thuận riêng của từng phòng.

**F10**

• Mỗi vai trò có danh sách quyền theo module đúng như bảng phân quyền, lưu trong cơ sở dữ liệu chứ không gán cứng trong mã nguồn
• Menu bên trái chỉ hiển thị mục mà vai trò hiện tại có quyền xem
• Gọi thẳng API của module không có quyền trả về mã 403 kèm thông điệp thống nhất, không trả dữ liệu
• Truy cập đường dẫn trực tiếp trên trình duyệt tới trang không có quyền thì chuyển về trang báo không đủ quyền

**G10**

5.0

**H10**

Must

**I10**

Chưa bắt đầu

### Dòng 11

**A11**

S1-05

**B11**

1.0

**C11**

EP-01

**D11**

Khách thuê

**E11**

Là Khách thuê, tôi muốn đổi mật khẩu và lấy lại mật khẩu khi quên, để tự xử lý mà không phải nhờ chủ nhà gọi cho quản trị viên.

**F11**

• Đổi mật khẩu yêu cầu nhập đúng mật khẩu hiện tại, mật khẩu mới phải khác mật khẩu cũ
• Quên mật khẩu gửi liên kết đặt lại qua email, liên kết hết hạn sau 30 phút và chỉ dùng được một lần
• Yêu cầu đặt lại mật khẩu giới hạn 3 lần trong một giờ cho mỗi địa chỉ email
• Đặt lại mật khẩu thành công thì mọi phiên đang đăng nhập của tài khoản đó bị đăng xuất

**G11**

3.0

**H11**

Must

**I11**

Chưa bắt đầu

### Dòng 12

**A12**

S1-06

**B12**

1.0

**C12**

EP-01

**D12**

Khách thuê

**E12**

Là Khách thuê, tôi muốn khai báo hồ sơ cá nhân gồm số căn cước và ảnh giấy tờ, để đến lúc ký hợp đồng chủ nhà không phải hỏi lại và chụp lại giấy tờ từ đầu.

**F12**

• Hồ sơ gồm họ tên, ngày sinh, số căn cước, quê quán, nghề nghiệp và ảnh mặt trước mặt sau giấy tờ
• Ảnh chấp nhận JPG hoặc PNG, tối đa 5MB mỗi tệp, tự nén chiều rộng tối đa 1600px trước khi lưu
• Số căn cước hiển thị dạng che bớt chỉ còn 4 chữ số cuối với mọi vai trò trừ chủ nhà của phòng đang thuê và quản trị hệ thống
• Trường số căn cước chỉ nhận 9 hoặc 12 chữ số, nhập sai thì báo lỗi ngay tại trường đó

**G12**

3.0

**H12**

Should

**I12**

Chưa bắt đầu

### Dòng 13

**A13**

S1-07

**B13**

1.0

**C13**

EP-02

**D13**

Chủ nhà

**E13**

Là Chủ nhà, tôi muốn khai báo các toà nhà và khu trọ của mình, để mọi phòng, hợp đồng và hoá đơn về sau đều quy về đúng địa điểm khi xem báo cáo.

**F13**

• Toà nhà gồm tên, địa chỉ đầy đủ tới phường xã, số tầng, người quản lý phụ trách và ghi chú
• Một toà nhà gán được cho một tài khoản Quản lý toà nhà, một quản lý phụ trách được nhiều toà
• Không cho xoá toà nhà đang có phòng, hệ thống đề nghị chuyển sang trạng thái ngừng hoạt động thay vì xoá
• Danh sách toà nhà hiển thị số phòng, số phòng đang trống và tìm kiếm theo tên hoặc địa chỉ

**G13**

5.0

**H13**

Must

**I13**

Chưa bắt đầu

### Dòng 14

**A14**

S1-08

**B14**

1.0

**C14**

EP-02

**D14**

Chủ nhà

**E14**

Là Chủ nhà, tôi muốn khai báo từng phòng với diện tích, giá thuê và trạng thái, để nhìn một màn hình là biết còn phòng nào trống thay vì lần lại sổ tay.

**F14**

• Phòng gồm mã phòng, toà nhà, tầng, diện tích theo mét vuông, giá thuê theo tháng, số người ở tối đa và trạng thái
• Trạng thái phòng thuộc một trong bốn giá trị: Trống, Đã đặt cọc, Đang thuê, Ngừng cho thuê
• Mã phòng không được trùng trong cùng một toà nhà, trùng thì báo lỗi ngay khi lưu
• Giá thuê là số nguyên dương tính theo VND, tối thiểu 500.000 và hiển thị có dấu chấm ngăn cách nghìn
• Tạo nhanh nhiều phòng theo mẫu số tầng và số phòng mỗi tầng, hệ thống sinh mã phòng tự động

**G14**

5.0

**H14**

Must

**I14**

Chưa bắt đầu

### Dòng 15

**A15**

S1-09

**B15**

1.0

**C15**

EP-02

**D15**

Chủ nhà

**E15**

Là Chủ nhà, tôi muốn khai báo các dịch vụ kèm theo và đơn giá của chúng, để hoá đơn hằng tháng lấy đúng đơn giá thay vì mỗi tháng gõ lại bằng tay.

**F15**

• Dịch vụ gồm tên, cách tính tiền theo chỉ số hoặc theo đầu người hoặc cố định theo phòng, đơn vị tính và đơn giá
• Hệ thống tạo sẵn năm dịch vụ mặc định: điện, nước, rác, gửi xe, internet, chủ nhà sửa được đơn giá
• Sửa đơn giá phải ghi ngày bắt đầu hiệu lực, hoá đơn đã phát hành trước ngày đó giữ nguyên đơn giá cũ
• Không cho xoá dịch vụ đang được tham chiếu bởi hợp đồng hoặc hoá đơn, chỉ cho ngừng áp dụng

**G15**

5.0

**H15**

Must

**I15**

Chưa bắt đầu

### Dòng 16

**A16**

S1-10

**B16**

1.0

**C16**

EP-01

**D16**

Quản trị hệ thống

**E16**

Là Quản trị hệ thống, tôi muốn xem nhật ký thao tác trên dữ liệu quan trọng, để khi khách nói hoá đơn bị sửa thì có căn cứ trả lời chứ không phải đoán.

**F16**

• Ghi lại thao tác tạo, sửa, xoá trên phòng, đơn giá dịch vụ, hợp đồng, chỉ số điện nước, hoá đơn và thanh toán
• Mỗi dòng nhật ký có thời điểm, người thực hiện, vai trò, đối tượng tác động và giá trị trước sau của các trường bị đổi
• Lọc nhật ký theo khoảng ngày, theo người thực hiện và theo loại đối tượng
• Nhật ký chỉ đọc, không tài khoản nào sửa hay xoá được kể cả quản trị hệ thống

**G16**

3.0

**H16**

Should

**I16**

Chưa bắt đầu

### Dòng 17

**A17**

SPRINT 2 — Tin đăng phòng trống và yêu cầu thuê   ·   10 story · 42 point

### Dòng 18

**A18**

Sprint Goal: Cuối sprint, chủ nhà đăng được tin cho thuê từ phòng trống và khách tìm thấy tin rồi gửi yêu cầu thuê hoặc đặt lịch xem phòng, chủ nhà xử lý được yêu cầu đó.

### Dòng 19

**A19**

S2-01

**B19**

2.0

**C19**

EP-02

**D19**

Chủ nhà

**E19**

Là Chủ nhà, tôi muốn gán dịch vụ và đơn giá riêng cho từng phòng, để phòng tầng trệt có thêm phí gửi xe còn phòng trên tầng thì không, mà vẫn tính đúng khi lên hoá đơn.

**F19**

• Mỗi phòng chọn được các dịch vụ áp dụng, mặc định lấy theo cấu hình của toà nhà
• Cho phép đặt đơn giá riêng cho một dịch vụ ở một phòng, khi đó đơn giá riêng thắng đơn giá chung
• Màn hình phòng hiển thị bảng dịch vụ đang áp dụng kèm tổng chi phí cố định dự kiến mỗi tháng
• Bỏ áp dụng một dịch vụ chỉ ảnh hưởng tới hoá đơn phát hành từ kỳ sau, không đụng vào hoá đơn cũ

**G19**

3.0

**H19**

Must

**I19**

Chưa bắt đầu

### Dòng 20

**A20**

S2-02

**B20**

2.0

**C20**

EP-02

**D20**

Chủ nhà

**E20**

Là Chủ nhà, tôi muốn tải lên ảnh thực tế của phòng, để khách nhìn đúng phòng rồi mới hẹn đến xem, đỡ mất công dẫn khách lên xuống cả buổi.

**F20**

• Mỗi phòng tải được tối đa 8 ảnh, mỗi ảnh tối đa 5MB, định dạng JPG hoặc PNG
• Kéo thả để sắp xếp thứ tự ảnh, ảnh đầu tiên là ảnh đại diện hiển thị trên danh sách tin
• Ảnh được tạo thêm bản thu nhỏ chiều rộng 400px dùng cho danh sách để tải nhanh trên 3G
• Xoá ảnh có hỏi xác nhận và xoá cả tệp trên kho lưu trữ, không để lại tệp mồ côi

**G20**

3.0

**H20**

Should

**I20**

Chưa bắt đầu

### Dòng 21

**A21**

S2-03

**B21**

2.0

**C21**

EP-03

**D21**

Chủ nhà

**E21**

Là Chủ nhà, tôi muốn đăng tin cho thuê từ một phòng đang trống, để không phải gõ lại diện tích và giá đã có sẵn trong hệ thống mỗi lần đăng.

**F21**

• Chỉ phòng ở trạng thái Trống mới tạo được tin đăng mới, phòng đang thuê thì nút đăng tin bị vô hiệu kèm lý do
• Tin đăng lấy sẵn diện tích, giá thuê, ảnh và dịch vụ của phòng, chủ nhà chỉnh tiêu đề và mô tả thêm
• Tin có trạng thái Nháp, Đang hiển thị, Tạm ẩn, Đã cho thuê và ngày hết hạn hiển thị mặc định 30 ngày
• Một phòng chỉ có tối đa một tin đang hiển thị tại một thời điểm
• Tin quá ngày hết hạn tự chuyển sang Tạm ẩn và chủ nhà thấy cảnh báo trên danh sách tin

**G21**

5.0

**H21**

Must

**I21**

Chưa bắt đầu

### Dòng 22

**A22**

S2-04

**B22**

2.0

**C22**

EP-03

**D22**

Khách thuê

**E22**

Là Khách thuê, tôi muốn tìm và lọc tin theo khu vực, khoảng giá và diện tích, để nhanh chóng loại bỏ những phòng ngoài khả năng chi trả.

**F22**

• Lọc theo quận huyện, khoảng giá thuê, khoảng diện tích và số người ở tối đa, các bộ lọc kết hợp được với nhau
• Chỉ hiển thị tin ở trạng thái Đang hiển thị và chưa quá ngày hết hạn
• Sắp xếp theo mới đăng nhất, giá tăng dần hoặc giá giảm dần; phân trang 12 tin mỗi trang
• Kết quả trả về dưới 2 giây với 500 tin trong cơ sở dữ liệu
• Không có kết quả thì hiển thị gợi ý nới rộng khoảng giá thay vì để trang trắng

**G22**

5.0

**H22**

Must

**I22**

Chưa bắt đầu

### Dòng 23

**A23**

S2-05

**B23**

2.0

**C23**

EP-03

**D23**

Khách thuê

**E23**

Là Khách thuê, tôi muốn xem trang chi tiết tin với đầy đủ chi phí kèm theo, để biết tổng tiền phải trả mỗi tháng chứ không chỉ mỗi tiền phòng.

**F23**

• Trang hiển thị bộ ảnh, diện tích, giá thuê, số người ở tối đa, tiền cọc dự kiến và mô tả
• Bảng dịch vụ liệt kê rõ điện và nước tính theo đơn giá bao nhiêu một đơn vị, các khoản cố định bao nhiêu một tháng
• Có dòng ước tính tổng chi phí tháng đầu gồm tiền phòng cộng các khoản cố định, kèm chú thích chưa bao gồm điện nước theo thực tế sử dụng
• Trang xem được khi chưa đăng nhập, bố cục đọc tốt trên màn hình rộng 360px

**G23**

3.0

**H23**

Must

**I23**

Chưa bắt đầu

### Dòng 24

**A24**

S2-06

**B24**

2.0

**C24**

EP-03

**D24**

Khách thuê

**E24**

Là Khách thuê, tôi muốn gửi yêu cầu thuê hoặc đặt lịch xem phòng ngay trên tin đăng, để không phải gọi điện nhiều lần mà vẫn giữ được chỗ.

**F24**

• Biểu mẫu gồm loại yêu cầu là Xem phòng hoặc Thuê ngay, ngày mong muốn, số người dự kiến ở và lời nhắn
• Ngày mong muốn không được là ngày quá khứ và không xa quá 60 ngày kể từ hôm nay
• Số người dự kiến ở vượt số người tối đa của phòng thì bị từ chối kèm thông báo nêu rõ giới hạn
• Một tài khoản chỉ gửi được một yêu cầu đang mở cho cùng một tin, gửi lại bị từ chối và chỉ dẫn tới yêu cầu cũ
• Gửi thành công sinh mã yêu cầu dạng YC-yyyyMM-xxxx và hiển thị ngay cho khách

**G24**

5.0

**H24**

Must

**I24**

Chưa bắt đầu

### Dòng 25

**A25**

S2-07

**B25**

2.0

**C25**

EP-03

**D25**

Chủ nhà

**E25**

Là Chủ nhà, tôi muốn xem danh sách yêu cầu theo trạng thái trên một màn hình, để sáng ra biết ngay hôm nay phải trả lời những ai thay vì lục lại tin nhắn.

**F25**

• Danh sách hiển thị mã yêu cầu, tên khách, số điện thoại, phòng, loại yêu cầu, ngày mong muốn và trạng thái
• Lọc theo trạng thái Mới, Đã hẹn lịch, Đã duyệt, Từ chối, Đã huỷ và lọc theo toà nhà
• Mặc định sắp xếp yêu cầu mới nhất lên đầu và đánh dấu nổi bật yêu cầu chưa xử lý quá 24 giờ
• Hiển thị số lượng yêu cầu chưa xử lý ngay trên menu để không bỏ sót

**G25**

5.0

**H25**

Must

**I25**

Chưa bắt đầu

### Dòng 26

**A26**

S2-08

**B26**

2.0

**C26**

EP-03

**D26**

Chủ nhà

**E26**

Là Chủ nhà, tôi muốn xác nhận lịch hẹn xem phòng, đổi lịch hoặc từ chối yêu cầu, để khách biết rõ có đến được hay không mà không phải gọi lại hỏi.

**F26**

• Xác nhận lịch phải chọn ngày giờ cụ thể, hệ thống cảnh báo nếu đã có lịch khác cùng phòng trong khoảng 30 phút
• Từ chối bắt buộc chọn lý do trong danh sách gồm đã có khách thuê, không phù hợp số người, khách không liên lạc được, lý do khác kèm ghi chú
• Mỗi lần đổi trạng thái đều ghi lịch sử với người thực hiện và thời điểm, khách xem được lịch sử này
• Duyệt yêu cầu loại Thuê ngay chuyển phòng sang trạng thái Đã đặt cọc và mở nút lập hợp đồng

**G26**

5.0

**H26**

Must

**I26**

Chưa bắt đầu

### Dòng 27

**A27**

S2-09

**B27**

2.0

**C27**

EP-03

**D27**

Khách thuê

**E27**

Là Khách thuê, tôi muốn theo dõi trạng thái các yêu cầu đã gửi, để biết mình còn phải chờ hay nên đi tìm phòng khác.

**F27**

• Danh sách yêu cầu của tôi hiển thị mã yêu cầu, phòng, ngày gửi, trạng thái hiện tại và lịch hẹn nếu có
• Yêu cầu bị từ chối hiển thị lý do chủ nhà đã chọn
• Khách tự huỷ được yêu cầu khi trạng thái còn là Mới hoặc Đã hẹn lịch, huỷ rồi thì không khôi phục
• Từ mỗi dòng bấm được sang trang chi tiết tin đăng tương ứng

**G27**

3.0

**H27**

Must

**I27**

Chưa bắt đầu

### Dòng 28

**A28**

S2-10

**B28**

2.0

**C28**

EP-02

**D28**

Chủ nhà

**E28**

Là Chủ nhà, tôi muốn cấu hình cách tính tiền điện nước cho từng toà nhà, để toà dùng nước khoán theo đầu người và toà dùng đồng hồ riêng đều tính đúng.

**F28**

• Mỗi dịch vụ điện và nước ở một toà chọn được cách tính theo chỉ số đồng hồ hoặc khoán theo đầu người
• Cách tính theo chỉ số yêu cầu nhập đơn giá trên một đơn vị, cách tính khoán yêu cầu nhập số tiền một người một tháng
• Đổi cách tính chỉ áp dụng từ kỳ hoá đơn kế tiếp, hệ thống hiển thị rõ kỳ bắt đầu áp dụng trước khi lưu
• Không cho lưu cấu hình thiếu đơn giá hoặc đơn giá bằng 0

**G28**

5.0

**H28**

Must

**I28**

Chưa bắt đầu

### Dòng 29

**A29**

SPRINT 3 — Hợp đồng thuê, chốt điện nước và hoá đơn tháng   ·   10 story · 43 point

### Dòng 30

**A30**

Sprint Goal: Cuối sprint, chủ nhà lập được hợp đồng từ yêu cầu đã duyệt, quản lý toà nhà ghi chỉ số điện nước và hệ thống phát hành được hoá đơn tháng đúng số tiền.

### Dòng 31

**A31**

S3-01

**B31**

3.0

**C31**

EP-04

**D31**

Chủ nhà

**E31**

Là Chủ nhà, tôi muốn lập hợp đồng thuê từ yêu cầu đã duyệt, để tiền cọc, ngày bắt đầu và giá thuê được ghi lại một chỗ thay vì ghi trên tờ giấy dễ thất lạc.

**F31**

• Hợp đồng lấy sẵn thông tin khách và phòng từ yêu cầu đã duyệt, chủ nhà nhập tiền cọc, giá thuê chốt, ngày bắt đầu, kỳ hạn tính theo tháng và ngày chốt hoá đơn hằng tháng
• Ngày kết thúc tự tính từ ngày bắt đầu cộng kỳ hạn, không cho sửa tay để tránh lệch với kỳ hạn
• Không cho lập hợp đồng mới cho phòng đã có hợp đồng hiệu lực chồng lấn khoảng thời gian, hệ thống chỉ ra hợp đồng đang vướng
• Tiền cọc mặc định bằng một tháng giá thuê, sửa được nhưng không nhỏ hơn 0 và không lớn hơn ba tháng giá thuê
• Lưu thành công sinh mã hợp đồng dạng HD-yyyy-xxxx, chuyển phòng sang Đang thuê và ghi chỉ số điện nước đầu kỳ

**G31**

8.0

**H31**

Must

**I31**

Chưa bắt đầu

### Dòng 32

**A32**

S3-02

**B32**

3.0

**C32**

EP-04

**D32**

Chủ nhà

**E32**

Là Chủ nhà, tôi muốn ghi nhận danh sách người ở ghép trong phòng bên cạnh người đứng tên, để tính đúng các khoản khoán theo đầu người và biết trong phòng hiện có những ai.

**F32**

• Mỗi hợp đồng có đúng một người đứng tên, là người chịu trách nhiệm thanh toán và nhận lại tiền cọc
• Thêm được người ở ghép với họ tên, số điện thoại, số căn cước và ngày bắt đầu ở cùng
• Tổng số người ở gồm người đứng tên và người ở ghép không vượt số người tối đa của phòng, vượt thì chặn kèm thông báo nêu rõ giới hạn
• Ghi nhận ngày chuyển đi của một người ở ghép, từ kỳ hoá đơn sau các khoản khoán theo đầu người giảm tương ứng
• Lịch sử người ở của phòng tra lại được theo từng khoảng thời gian

**G32**

5.0

**H32**

Must

**I32**

Chưa bắt đầu

### Dòng 33

**A33**

S3-03

**B33**

3.0

**C33**

EP-04

**D33**

Khách thuê

**E33**

Là Khách thuê, tôi muốn xem hợp đồng của mình và tải bản PDF, để lúc cần đối chiếu điều khoản thì mở điện thoại ra xem chứ không phải tìm tờ giấy.

**F33**

• Trang hợp đồng hiển thị mã hợp đồng, phòng, giá thuê, tiền cọc, ngày bắt đầu, ngày kết thúc và danh sách người ở
• Tải được bản PDF có đầy đủ các thông tin trên cùng bảng dịch vụ và đơn giá áp dụng
• Khách chỉ xem được hợp đồng mà mình là người đứng tên hoặc người ở ghép, gọi tới hợp đồng khác trả về mã 403
• Hợp đồng còn dưới 30 ngày là hết hạn thì hiển thị cảnh báo kèm số ngày còn lại

**G33**

3.0

**H33**

Must

**I33**

Chưa bắt đầu

### Dòng 34

**A34**

S3-04

**B34**

3.0

**C34**

EP-04

**D34**

Chủ nhà

**E34**

Là Chủ nhà, tôi muốn phòng và tin đăng tự đổi trạng thái khi hợp đồng có hiệu lực, để không còn cảnh khách gọi hỏi một phòng đã cho thuê từ tuần trước.

**F34**

• Hợp đồng chuyển sang hiệu lực thì phòng chuyển sang Đang thuê và tin đăng của phòng chuyển sang Đã cho thuê trong cùng một giao dịch
• Tin đã ẩn không còn xuất hiện trên trang tìm kiếm công khai ngay lần tải trang kế tiếp
• Nếu một bước thất bại thì toàn bộ giao dịch bị hoàn tác, không để phòng đang thuê mà tin vẫn hiển thị
• Mọi lần đổi trạng thái phòng đều được ghi vào nhật ký hoạt động

**G34**

2.0

**H34**

Must

**I34**

Chưa bắt đầu

### Dòng 35

**A35**

S3-05

**B35**

3.0

**C35**

EP-05

**D35**

Quản lý toà nhà

**E35**

Là Quản lý toà nhà, tôi muốn nhập chỉ số điện nước cuối kỳ ngay trên điện thoại khi đi ghi từng phòng, để không phải ghi giấy rồi tối về gõ lại và gõ nhầm.

**F35**

• Màn hình nhập liệt kê toàn bộ phòng đang thuê của toà theo thứ tự tầng và mã phòng, hiển thị sẵn chỉ số kỳ trước
• Chỉ số mới không được nhỏ hơn chỉ số kỳ trước, nhập nhỏ hơn thì chặn lưu và báo lỗi ngay tại dòng đó
• Mức tiêu thụ chênh quá 200 phần trăm so với trung bình ba kỳ gần nhất thì cảnh báo và yêu cầu người nhập xác nhận trước khi lưu
• Lưu được từng phòng một, phòng chưa nhập vẫn giữ nguyên trạng thái chưa chốt và hiển thị rõ còn bao nhiêu phòng chưa nhập
• Bố cục dùng được bằng một tay trên màn hình rộng 360px, ô nhập bật sẵn bàn phím số

**G35**

5.0

**H35**

Must

**I35**

Chưa bắt đầu

### Dòng 36

**A36**

S3-06

**B36**

3.0

**C36**

EP-05

**D36**

Chủ nhà

**E36**

Là Chủ nhà, tôi muốn phát hành hoá đơn tháng cho cả toà chỉ bằng một lần thao tác, để không mất nửa buổi gõ Excel và cộng nhầm tiền điện.

**F36**

• Hoá đơn gồm tiền phòng, tiền điện và nước tính theo chỉ số nhân đơn giá, các khoản dịch vụ cố định và khoản khoán theo đầu người
• Tiền điện nước lấy đúng đơn giá có hiệu lực tại ngày chốt kỳ, không lấy đơn giá sửa sau đó
• Hoá đơn có kỳ áp dụng, ngày phát hành và hạn thanh toán mặc định 7 ngày kể từ ngày phát hành, sửa được trước khi phát hành
• Phòng chưa chốt chỉ số thì bị bỏ qua và liệt kê rõ trong kết quả chạy, không phát hành hoá đơn thiếu dữ liệu
• Một phòng chỉ có một hoá đơn cho một kỳ, chạy lại lần hai không tạo hoá đơn trùng, phát hành cho 50 phòng hoàn tất dưới 30 giây

**G36**

8.0

**H36**

Must

**I36**

Chưa bắt đầu

### Dòng 37

**A37**

S3-07

**B37**

3.0

**C37**

EP-05

**D37**

Khách thuê

**E37**

Là Khách thuê, tôi muốn xem hoá đơn tháng với từng khoản được bóc tách, để biết tiền điện tháng này cao là do dùng nhiều chứ không phải bị tính nhầm.

**F37**

• Hoá đơn hiển thị từng dòng khoản mục kèm chỉ số đầu kỳ, chỉ số cuối kỳ, số đơn vị tiêu thụ, đơn giá và thành tiền
• Có dòng tổng cộng, số đã thanh toán, số còn phải trả và hạn thanh toán
• Hoá đơn quá hạn hiển thị nhãn quá hạn kèm số ngày trễ tính theo múi giờ Asia/Ho_Chi_Minh
• Danh sách hoá đơn của tôi lọc được theo kỳ và theo trạng thái thanh toán

**G37**

3.0

**H37**

Must

**I37**

Chưa bắt đầu

### Dòng 38

**A38**

S3-08

**B38**

3.0

**C38**

EP-05

**D38**

Chủ nhà

**E38**

Là Chủ nhà, tôi muốn kiểm tra và sửa hoá đơn nháp trước khi gửi cho khách, để không phải xin lỗi rồi gửi lại hoá đơn đính chính.

**F38**

• Hoá đơn sinh ra ở trạng thái Nháp, chỉ khi bấm phát hành mới gửi thông báo cho khách
• Ở trạng thái Nháp được sửa chỉ số, thêm khoản phát sinh hoặc khoản giảm trừ kèm ghi chú bắt buộc
• Hoá đơn đã phát hành không sửa được, chỉ huỷ kèm lý do rồi phát hành lại, bản huỷ vẫn lưu để tra cứu
• Mọi lần sửa hoá đơn nháp đều ghi nhật ký với giá trị trước và sau

**G38**

3.0

**H38**

Must

**I38**

Chưa bắt đầu

### Dòng 39

**A39**

S3-09

**B39**

3.0

**C39**

EP-04

**D39**

Chủ nhà

**E39**

Là Chủ nhà, tôi muốn gia hạn hợp đồng sắp hết hạn, để khách ở tiếp mà không phải lập lại hợp đồng mới và mất lịch sử thanh toán cũ.

**F39**

• Gia hạn nhập kỳ hạn mới tính theo tháng và giá thuê mới, mặc định giữ nguyên giá cũ
• Ngày bắt đầu kỳ gia hạn là ngày liền sau ngày kết thúc hiện tại, không cho tạo khoảng trống hay chồng lấn
• Giá thuê mới chỉ áp dụng từ kỳ hoá đơn đầu tiên nằm trong khoảng gia hạn
• Lịch sử gia hạn hiển thị trên hợp đồng gồm từng lần, người thực hiện và giá áp dụng

**G39**

3.0

**H39**

Should

**I39**

Chưa bắt đầu

### Dòng 40

**A40**

S3-10

**B40**

3.0

**C40**

EP-05

**D40**

Chủ nhà

**E40**

Là Chủ nhà, tôi muốn theo dõi tiến độ chốt chỉ số của từng toà theo kỳ, để đến ngày phát hành hoá đơn không bị thiếu phòng nào.

**F40**

• Màn hình theo kỳ hiển thị tổng số phòng đang thuê, số phòng đã chốt và số phòng còn thiếu của từng toà
• Bấm vào số phòng còn thiếu mở ra danh sách phòng cụ thể kèm tên người quản lý phụ trách
• Khoá kỳ sau khi phát hành hoá đơn, kỳ đã khoá không cho sửa chỉ số nữa
• Đến ngày chốt mà còn phòng chưa nhập thì hiển thị cảnh báo trên trang chủ của chủ nhà

**G40**

3.0

**H40**

Should

**I40**

Chưa bắt đầu

### Dòng 41

**A41**

SPRINT 4 — Thanh toán, công nợ, trả phòng và nghiệm thu   ·   10 story · 43 point

### Dòng 42

**A42**

Sprint Goal: Cuối sprint, khách báo đã thanh toán và chủ nhà xác nhận đã thu kể cả trả thiếu, hệ thống theo dõi công nợ, xử lý trả phòng tất toán cọc và luồng nghiệp vụ chạy trọn vẹn trên môi trường staging.

### Dòng 43

**A43**

S4-01

**B43**

4.0

**C43**

EP-05

**D43**

Khách thuê

**E43**

Là Khách thuê, tôi muốn báo đã chuyển khoản kèm ảnh chứng từ, để chủ nhà đối chiếu sao kê nhanh và không nhắn lại hỏi đã trả chưa.

**F43**

• Biểu mẫu gồm số tiền đã trả, ngày trả, hình thức tiền mặt hoặc chuyển khoản và ảnh chứng từ
• Số tiền báo trả phải lớn hơn 0 và không vượt quá số còn phải trả của hoá đơn
• Ảnh chứng từ tối đa 5MB, chỉ chủ nhà của phòng và quản trị hệ thống xem được qua liên kết có hạn 15 phút
• Sau khi gửi, hoá đơn hiển thị trạng thái Chờ xác nhận và khách không sửa được báo cáo đã gửi, chỉ huỷ khi chủ nhà chưa xác nhận

**G43**

3.0

**H43**

Must

**I43**

Chưa bắt đầu

### Dòng 44

**A44**

S4-02

**B44**

4.0

**C44**

EP-05

**D44**

Chủ nhà

**E44**

Là Chủ nhà, tôi muốn xác nhận đã thu tiền kể cả khi khách trả thiếu, để phần còn thiếu tự thành công nợ thay vì nằm trong trí nhớ.

**F44**

• Một hoá đơn nhận được nhiều lần thanh toán, mỗi lần ghi số tiền, ngày thu, hình thức và người xác nhận
• Tổng đã thu nhỏ hơn tổng hoá đơn thì trạng thái là Trả một phần, bằng tổng thì là Đã thanh toán, không cho thu vượt tổng hoá đơn
• Trạng thái và số còn phải trả của hoá đơn cập nhật ngay sau mỗi lần xác nhận
• Huỷ một lần thu đã ghi nhầm phải nhập lý do, hệ thống tính lại số còn phải trả và giữ bản ghi đã huỷ để tra cứu

**G44**

5.0

**H44**

Must

**I44**

Chưa bắt đầu

### Dòng 45

**A45**

S4-03

**B45**

4.0

**C45**

EP-05

**D45**

Chủ nhà

**E45**

Là Chủ nhà, tôi muốn xem công nợ theo phòng và theo toà trên một màn hình, để biết ngay hôm nay phải đi đòi những phòng nào.

**F45**

• Danh sách công nợ hiển thị phòng, người đứng tên, số hoá đơn chưa trả hết, tổng còn thiếu và số ngày quá hạn dài nhất
• Lọc theo toà nhà, theo khoảng ngày quá hạn và theo ngưỡng số tiền còn thiếu
• Có dòng tổng cộng số tiền còn thiếu của tập kết quả đang lọc
• Số liệu khớp 100% với bộ dữ liệu kiểm thử gồm ít nhất 5 trường hợp trả thiếu và 2 trường hợp quá hạn nhiều kỳ
• Xuất được danh sách đang lọc ra tệp CSV mã hoá UTF-8 có dấu tiếng Việt

**G45**

5.0

**H45**

Must

**I45**

Chưa bắt đầu

### Dòng 46

**A46**

S4-04

**B46**

4.0

**C46**

EP-04

**D46**

Chủ nhà

**E46**

Là Chủ nhà, tôi muốn xử lý trả phòng và tất toán tiền cọc có trừ hư hỏng, để hai bên nhìn cùng một bảng số liệu thay vì cãi nhau bằng trí nhớ.

**F46**

• Trả phòng yêu cầu nhập ngày trả thực tế và chỉ số điện nước cuối cùng, chỉ số không nhỏ hơn lần chốt gần nhất
• Hệ thống tạo hoá đơn kỳ cuối tính tiền phòng theo tỉ lệ số ngày ở thực tế cộng điện nước phát sinh
• Bảng tất toán gồm tiền cọc, các khoản khấu trừ hư hỏng có tên khoản và số tiền, công nợ còn lại của các kỳ trước, ra số tiền hoàn cho khách hoặc khách còn phải trả thêm
• Chấm dứt trước hạn phải chọn lý do và chỉ rõ có áp dụng phạt cọc hay không, mức phạt ghi thành một dòng khấu trừ
• Tất toán xong thì hợp đồng chuyển sang Đã kết thúc, không ghi nhận thêm hoá đơn mới cho hợp đồng đó và khách tải được bảng tất toán dạng PDF

**G46**

8.0

**H46**

Must

**I46**

Chưa bắt đầu

### Dòng 47

**A47**

S4-05

**B47**

4.0

**C47**

EP-04

**D47**

Chủ nhà

**E47**

Là Chủ nhà, tôi muốn phòng tự về trạng thái trống và tin đăng bật lại khi hợp đồng kết thúc, để không mất thêm tuần nào chỉ vì quên đăng tin.

**F47**

• Hợp đồng chuyển sang Đã kết thúc thì phòng chuyển sang Trống trong cùng giao dịch
• Tin đăng cũ của phòng được nhân bản thành tin mới ở trạng thái Nháp, giữ nguyên ảnh và mô tả, cập nhật giá theo giá phòng hiện tại
• Chủ nhà bấm một nút để chuyển tin nháp sang Đang hiển thị, thao tác hoàn tất trong dưới 1 phút
• Trang chủ của chủ nhà hiển thị danh sách phòng vừa trống chưa có tin đang hiển thị

**G47**

3.0

**H47**

Must

**I47**

Chưa bắt đầu

### Dòng 48

**A48**

S4-06

**B48**

4.0

**C48**

EP-06

**D48**

Khách thuê

**E48**

Là Khách thuê, tôi muốn báo hỏng thiết bị trong phòng kèm ảnh, để yêu cầu không bị trôi mất như tin nhắn Zalo.

**F48**

• Báo hỏng gồm loại thiết bị, mô tả, mức độ khẩn gồm Thường và Gấp, tối đa 3 ảnh mỗi ảnh 5MB
• Hệ thống sinh mã báo hỏng dạng BH-yyyyMM-xxxx và đặt trạng thái Mới
• Khách chỉ báo hỏng cho phòng mình đang thuê theo hợp đồng còn hiệu lực
• Khách theo dõi được trạng thái và các phản hồi của quản lý trên cùng một trang

**G48**

3.0

**H48**

Must

**I48**

Chưa bắt đầu

### Dòng 49

**A49**

S4-07

**B49**

4.0

**C49**

EP-06

**D49**

Quản lý toà nhà

**E49**

Là Quản lý toà nhà, tôi muốn tiếp nhận và cập nhật tiến độ xử lý báo hỏng, để chủ nhà và khách đều thấy việc đang tới đâu mà không phải gọi hỏi.

**F49**

• Trạng thái đi theo chuỗi Mới, Đang xử lý, Đã xong, Từ chối; mỗi lần đổi bắt buộc có ghi chú
• Ghi nhận chi phí sửa chữa và bên chịu chi phí là chủ nhà hay khách thuê, khoản do khách chịu được đánh dấu để đưa vào hoá đơn kỳ sau
• Báo hỏng mức Gấp chưa xử lý quá 24 giờ được đánh dấu nổi bật trên danh sách
• Danh sách lọc theo toà nhà, trạng thái và mức độ khẩn

**G49**

3.0

**H49**

Must

**I49**

Chưa bắt đầu

### Dòng 50

**A50**

S4-08

**B50**

4.0

**C50**

EP-06

**D50**

Khách thuê

**E50**

Là Khách thuê, tôi muốn nhận thông báo khi có hoá đơn mới và khi sắp đến hạn, để không bị tính là trễ hạn chỉ vì quên ngày.

**F50**

• Gửi email và tạo thông báo trong ứng dụng khi hoá đơn được phát hành, khi còn 2 ngày tới hạn và khi vừa quá hạn
• Nội dung email có kỳ hoá đơn, tổng tiền, số còn phải trả, hạn thanh toán và liên kết mở thẳng hoá đơn
• Tác vụ nhắc hạn chạy mỗi ngày lúc 08:00 giờ Việt Nam, mỗi hoá đơn chỉ nhắc một lần cho mỗi mốc
• Gửi email lỗi thì thử lại tối đa 3 lần và ghi lại lần gửi thất bại để quản trị hệ thống tra cứu
• Chuông thông báo trong ứng dụng hiển thị số thông báo chưa đọc và đánh dấu đã đọc được

**G50**

5.0

**H50**

Must

**I50**

Chưa bắt đầu

### Dòng 51

**A51**

S4-09

**B51**

4.0

**C51**

EP-06

**D51**

Chủ nhà

**E51**

Là Chủ nhà, tôi muốn xem báo cáo doanh thu và tỉ lệ lấp đầy theo tháng, để biết toà nào đang để trống nhiều mà điều chỉnh giá.

**F51**

• Báo cáo theo tháng hiển thị tổng tiền đã phát hành, tổng đã thu, số còn phải thu và tỉ lệ thu được tính theo phần trăm
• Tỉ lệ lấp đầy tính bằng số phòng đang thuê chia tổng số phòng đang cho thuê của toà tại ngày cuối tháng
• Lọc theo toà nhà và theo khoảng tháng, tối đa 12 tháng một lần xem
• Có biểu đồ cột doanh thu 6 tháng gần nhất và số liệu khớp với tổng hợp từ bảng hoá đơn
• Trang tải xong dưới 3 giây với dữ liệu 12 tháng của 500 phòng

**G51**

5.0

**H51**

Should

**I51**

Chưa bắt đầu

### Dòng 52

**A52**

S4-10

**B52**

4.0

**C52**

EP-01

**D52**

Quản trị hệ thống

**E52**

Là Quản trị hệ thống, tôi muốn có dữ liệu mẫu và môi trường staging đã rà soát phân quyền, để buổi nghiệm thu chạy được kịch bản thật mà không dừng lại vì lỗi cấu hình.

**F52**

• Bộ dữ liệu mẫu gồm 2 toà nhà, 30 phòng, 20 hợp đồng, 3 kỳ hoá đơn và các trường hợp trả đủ, trả thiếu, quá hạn
• Nạp lại dữ liệu mẫu bằng một lệnh, chạy xong dưới 2 phút và không để lại dữ liệu cũ
• Bảng rà soát phân quyền được kiểm thử với cả bốn vai trò trên toàn bộ module, mọi truy cập trái quyền trả về mã 403
• Hệ thống chạy trên staging bằng Docker Compose, có tài khoản mẫu cho bốn vai trò ghi trong tài liệu bàn giao
• Kịch bản demo end-to-end được chạy thử trọn vẹn ít nhất một lần trước buổi nghiệm thu

**G52**

3.0

**H52**

Must

**I52**

Chưa bắt đầu

### Dòng 53

**A53**

TỔNG CỘNG

**G53**

170.0

## Trang tính: 5. Sprint Plan

### Dòng 1

**A1**

KẾ HOẠCH SPRINT TỔNG THỂ

### Dòng 3

**A3**

Sprint

**B3**

Chủ đề

**C3**

Kết quả demo được ở cuối sprint

**D3**

Story

**E3**

Point

**F3**

Luỹ kế

**G3**

Còn lại

### Dòng 4

**A4**

1.0

**B4**

Nền tảng tài khoản và danh mục gốc

**C4**

Đăng nhập lần lượt bằng bốn tài khoản, mỗi vai trò thấy đúng menu của mình; chủ nhà tạo một toà nhà 10 phòng và khai báo đơn giá điện nước.

**D4**

10.0

**E4**

42.0

**F4**

42.0

**G4**

128.0

### Dòng 5

**A5**

2.0

**B5**

Tin đăng phòng trống và yêu cầu thuê

**C5**

Chủ nhà đăng tin một phòng trống kèm ảnh, khách tìm theo giá và quận rồi gửi yêu cầu xem phòng, chủ nhà xác nhận lịch hẹn và khách thấy trạng thái đã đổi.

**D5**

10.0

**E5**

42.0

**F5**

84.0

**G5**

86.0

### Dòng 6

**A6**

3.0

**B6**

Hợp đồng thuê, chốt điện nước và hoá đơn tháng

**C6**

Lập hợp đồng cho một phòng gồm tiền cọc và ngày bắt đầu, thêm một người ở ghép, ghi chỉ số cuối kỳ rồi phát hành hoá đơn tháng và khách xem được hoá đơn đó.

**D6**

10.0

**E6**

43.0

**F6**

127.0

**G6**

43.0

### Dòng 7

**A7**

4.0

**B7**

Thanh toán, công nợ, trả phòng và nghiệm thu

**C7**

Chạy liền mạch từ tin đăng, yêu cầu thuê, hợp đồng, hoá đơn, khách trả thiếu rồi trả nốt, chủ nhà xác nhận, khách trả phòng và tất toán cọc, phòng về trống và tin đăng bật lại.

**D7**

10.0

**E7**

43.0

**F7**

170.0

**G7**

0.0

### Dòng 8

**A8**

TỔNG

**D8**

40

Công thức nguồn: `SUM(D4:D7)`

**E8**

170

### Dòng 10

**A10**

DEFINITION OF READY / DEFINITION OF DONE

### Dòng 11

**A11**

#

**B11**

Definition of Ready — story được đưa vào sprint khi

**E11**

Definition of Done — story hoàn thành khi

### Dòng 12

**A12**

1.0

**B12**

Story viết đúng mẫu Là vai trò, tôi muốn, để và mọi thành viên trong đội hiểu giống nhau về phạm vi.

**E12**

Toàn bộ tiêu chí chấp nhận của story đã được kiểm thử thủ công và ghi lại kết quả.

### Dòng 13

**A13**

2.0

**B13**

Có từ 2 đến 5 tiêu chí chấp nhận cụ thể, kiểm chứng được, nêu rõ điều kiện biên và hành vi khi lỗi.

**E13**

Mã nguồn đã được ít nhất một thành viên khác review và đã hợp nhất vào nhánh chính, không còn xung đột.

### Dòng 14

**A14**

3.0

**B14**

Đã ước lượng point bằng planning poker, không story nào lớn hơn 8 point; lớn hơn thì tách trước khi đưa vào sprint.

**E14**

Có kiểm thử tự động cho các quy tắc nghiệp vụ tính tiền và ràng buộc chỉ số điện nước.

### Dòng 15

**A15**

4.0

**B15**

Phụ thuộc vào story hoặc API khác đã được xác định và story phụ thuộc đã nằm ở sprint trước hoặc đứng trước trong cùng sprint.

**E15**

Kiểm tra quyền được thực hiện ở tầng backend, không chỉ ẩn nút trên giao diện.

### Dòng 16

**A16**

5.0

**B16**

Có phác thảo giao diện hoặc mô tả bố cục cho các story có màn hình mới, đủ để lập trình viên không phải tự nghĩ ra luồng.

**E16**

Giao diện hoạt động đúng trên màn hình rộng 360px và trên Chrome, Safari phiên bản hiện hành.

### Dòng 17

**A17**

6.0

**B17**

Dữ liệu mẫu cần thiết để kiểm thử story đã xác định được lấy từ đâu, hoặc đã có trong bộ dữ liệu mẫu.

**E17**

Mọi số tiền hiển thị theo định dạng VND có dấu chấm ngăn cách nghìn, ngày giờ theo múi giờ Asia/Ho_Chi_Minh.

### Dòng 18

**A18**

7.0

**E18**

Không còn lỗi mức nghiêm trọng hoặc cao đang mở liên quan tới story.

### Dòng 19

**A19**

8.0

**E19**

Thay đổi cấu trúc cơ sở dữ liệu có script migration chạy được từ đầu trên cơ sở dữ liệu rỗng.

### Dòng 20

**A20**

9.0

**E20**

Story đã triển khai lên môi trường staging và giảng viên hướng dẫn xem được kết quả chạy thật.

### Dòng 22

**A22**

RỦI RO CHÍNH

### Dòng 23

**A23**

#

**B23**

Rủi ro

**C23**

Cách ứng phó

**E23**

Khả năng

**F23**

Tác động

### Dòng 24

**A24**

1.0

**B24**

Chỉ có 4 sprint nên phạm vi dễ phình ra rồi không kịp đóng luồng nghiệp vụ

**C24**

Cố định luồng chính từ đăng tin tới thu tiền là bắt buộc; các story Should và Could gồm báo cáo, nhật ký, cấu hình nâng cao được cắt trước tiên khi velocity tụt dưới 38 point; rà soát phạm vi vào giữa mỗi sprint

**E24**

Cao

**F24**

Cao

### Dòng 25

**A25**

2.0

**B25**

Nợ kỹ thuật và công việc kỹ thuật không nằm trong backlog như dựng CI, viết migration, cấu hình MinIO ăn mất thời gian của story

**C25**

Dành sẵn khoảng 15 phần trăm thời gian mỗi sprint cho việc kỹ thuật và ghi thành đầu việc riêng trên bảng; dựng khung dự án, Docker Compose và CI ngay trong hai ngày đầu Sprint 1

**E25**

Cao

**F25**

Trung bình

### Dòng 26

**A26**

3.0

**B26**

Quy tắc tính tiền điện nước và tất toán cọc bị hiểu sai dẫn tới hoá đơn sai số

**C26**

Chốt bộ ví dụ tính tay gồm ít nhất 10 trường hợp trước khi viết mã, biến thành kiểm thử tự động; mọi thay đổi công thức phải chạy lại bộ ví dụ này

**E26**

Trung bình

**F26**

Cao

### Dòng 27

**A27**

4.0

**B27**

Đội 5 người đều là thực tập sinh, chưa quen Spring Boot nên tốc độ hai tuần đầu thấp hơn dự kiến

**C27**

Đặt velocity Sprint 1 ở mức thận trọng, ghép cặp lập trình cho các story khó, chuẩn bị sẵn mẫu mã nguồn cho tầng repository và service để sao chép theo

**E27**

Cao

**F27**

Trung bình

### Dòng 28

**A28**

5.0

**B28**

Yêu cầu nghiệp vụ thay đổi giữa chừng khi giảng viên hoặc chủ nhà thật xem demo

**C28**

Demo cuối mỗi sprint, thay đổi được ghi thành story mới và xếp vào backlog thay vì chèn ngang; đổi phạm vi trong sprint phải đánh đổi bằng một story cùng số point

**E28**

Trung bình

**F28**

Trung bình

### Dòng 29

**A29**

6.0

**B29**

Gửi email qua SMTP Gmail bị chặn hoặc giới hạn số lượng làm hỏng phần thông báo

**C29**

Tách tầng gửi thông báo sau một giao diện chung, có chế độ ghi ra nhật ký thay vì gửi thật khi chạy môi trường phát triển; chuẩn bị sẵn phương án dùng Mailtrap cho demo

**E29**

Trung bình

**F29**

Thấp

### Dòng 30

**A30**

7.0

**B30**

Hai thành viên cùng sửa hoá đơn hoặc chỉ số của một phòng gây ghi đè dữ liệu

**C30**

Dùng khoá lạc quan theo số phiên bản trên bản ghi hoá đơn và chỉ số, người lưu sau nhận cảnh báo dữ liệu đã đổi và phải tải lại; khoá kỳ sau khi phát hành hoá đơn

**E30**

Thấp

**F30**

Trung bình
