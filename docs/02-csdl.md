# Đặc tả CSDL hiện hành: 22 bảng — SQLite

Bên dưới là toàn bộ nội dung `02-csdl.dbml`. Đây là đặc tả dữ liệu tham chiếu, không phải migration và không khẳng định tệp SQLite hiện có đã khớp hoàn toàn. Ứng dụng mới dùng ASP.NET Core MVC và Entity Framework Core để ánh xạ vào CSDL cũ. Kiểm tra schema thực tế trước khi cấu hình; không tự tạo lại hoặc thay đổi CSDL.

```dbml
// QUẢN LÝ PHÒNG TRỌ — MÔ HÌNH 22 BẢNG CHO DỰ ÁN THỰC TẬP
// Ứng dụng định hướng: ASP.NET Core MVC / Entity Framework Core; CSDL cũ: SQLite.
// Đây là đặc tả DBML, không phải model EF Core hoặc migration đã chạy.
// PK/FK/UNIQUE được khai báo trực tiếp. Quy tắc trong Note cần hiện thực riêng.

Project QuanLyPhongTro {
  database_type: 'SQLite'
  Note: '''
  Một tổ chức quản lý nhiều tòa, bốn vai trò. 22 bảng nghiệp vụ;
  không tính các bảng hạ tầng xác thực, quyền và migration của framework.
  Tiền VND lưu số nguyên đồng; xử lý bằng kiểu số thập phân chính xác của C#, không dùng số thực dấu phẩy động.
  Ngày nghiệp vụ dùng date; thời điểm thao tác lưu nhất quán theo UTC,
  hiển thị Asia/Ho_Chi_Minh. Số điện thoại/giấy tờ lưu chuỗi.
  Khoảng ngày tu_ngay đến den_ngay trong mô hình này BAO GỒM cả hai đầu;
  ngày kết thúc NULL nghĩa là chưa xác định. Hai khoảng có chung ngày là chồng lấn.
  Kỳ thuê tiếp theo bắt đầu ngày sau ngày kết thúc kỳ trước.
  Giữ lịch sử tài chính, không xóa dây chuyền. Mọi FK cần on_delete thích hợp,
  ưu tiên ngăn xóa dây chuyền đối với chứng từ; DBML không thay thế cấu hình hành vi xóa ở tầng ứng dụng/CSDL.
  CHECK, unique có điều kiện, kiểm tra chồng thời gian và các quy tắc liên bảng
  được ghi trong Note: phải chuyển thành constraints/service/migrations.
  SQLite có giới hạn về khóa đồng thời;
  phải có chiến lược ghi/transaction, chống gửi trùng và kiểm tra đồng thời phù hợp.
  Quyết định thiết kế: một hóa đơn chưa hủy / HỢP ĐỒNG / tháng, cho phép
  hai khách nối tiếp cùng phòng/tháng. Đây là làm rõ khác với câu phòng/kỳ của Excel.
  Tháng cuối thay thế bản định kỳ cùng kỳ khi đủ điều kiện; không tự hủy bản đã thu.
  Bản rút gọn chưa hỗ trợ hoàn/điều chỉnh hóa đơn đã thu hoặc thu hồi cọc đã hoàn
  bằng quy trình phức tạp. Cần chốt nghiệp vụ trước khi mở rộng.
  Các task đăng ký đầu tiên chỉ cần tai_khoan và hạ tầng xác thực của ứng dụng;
  chưa cần triển khai toàn bộ 22 bảng hoặc tạo hồ sơ khach_thue ngay.
  '''
}

// 01. TÀI KHOẢN — custom User ngay từ migration đầu tiên.
Table tai_khoan {
  id integer [pk, increment]
  ho_ten varchar(100) [not null]
  email varchar(254) [not null, unique]
  so_dien_thoai varchar(10) [not null, unique]
  mat_khau varchar(128) [not null]
  vai_tro varchar(20) [not null, default: 'KHACH_THUE']
  dang_hoat_dong boolean [not null, default: true]
  is_staff boolean [not null, default: false]
  is_superuser boolean [not null, default: false]
  last_login datetime
  ngay_tao datetime [not null]
  ngay_cap_nhat datetime [not null]

  indexes {
    (vai_tro, dang_hoat_dong)
  }
  Note: '''
  Vai trò: KHACH_THUE, CHU_NHA, QUAN_LY, ADMIN. Người tự đăng ký luôn KHACH_THUE,
  không lấy role/staff/superuser từ form. Một tài khoản một vai trò nghiệp vụ.
  Chuẩn hóa email strip + lowercase trước kiểm tra/lưu; unique không phân biệt hoa/thường
  cần constraint/collation hoặc cơ chế chuẩn hóa nhất quán ở mọi đường ghi.
  Điện thoại đúng ^0[0-9]{9}$. Mật khẩu >=8 ký tự, có chữ cái và chữ số;
  dùng cơ chế băm và xác minh mật khẩu an toàn của ASP.NET Core, bảo đảm tương thích với hash cũ nếu cần,
  không lưu mật khẩu rõ. cột mật khẩu ánh xạ vào mat_khau; trạng thái hoạt động ánh xạ vào dang_hoat_dong.
  is_staff/is_superuser/last_login là các trường kỹ thuật tương thích dữ liệu cũ nếu có.
  Đăng ký kiểm tra trùng độc lập hai trường, bắt xung đột UNIQUE đúng cách.
  Tạo phiên đăng nhập bằng cookie bảo mật sau khi đăng ký thành công. Khóa tài khoản phải
  được backend kiểm tra trên phiên hiện có. Không dùng role thay is_superuser tùy tiện.
  '''
}

// 02. HỒ SƠ NGƯỜI THUÊ / NGƯỜI Ở GHÉP
Table khach_thue {
  id integer [pk, increment]
  tai_khoan_id integer [unique, ref: - tai_khoan.id]
  ho_ten varchar(100) [not null]
  ngay_sinh date
  so_giay_to varchar(255) [note: 'Giá trị mã hóa nếu triển khai lưu giấy tờ; không log giá trị rõ.']
  giay_to_bon_so_cuoi varchar(4)
  anh_giay_to_truoc varchar(500)
  anh_giay_to_sau varchar(500)
  dia_chi_thuong_tru text
  nghe_nghiep varchar(150)
  so_dien_thoai varchar(15)
  email varchar(254)
  lien_he_khan_cap varchar(255)
  ngay_tao datetime [not null]

  Note: 'Tài khoản tùy chọn: người ở ghép có thể chưa đăng ký. Email/điện thoại ở đây là liên hệ, không là định danh đăng nhập. Người đứng tên và người ở ghép cùng tham chiếu bảng này. Kiểm tra giấy tờ đầu vào 9 hoặc 12 chữ số theo nguồn; che dữ liệu theo quyền. Ảnh giấy tờ truy cập qua view có kiểm tra quyền, không dùng media public.'
}

// 03. TÒA NHÀ
Table toa_nha {
  id integer [pk, increment]
  chu_nha_id integer [not null, ref: > tai_khoan.id]
  quan_ly_id integer [ref: > tai_khoan.id]
  ten_toa_nha varchar(150) [not null]
  dia_chi text [not null]
  phuong_xa varchar(100)
  quan_huyen varchar(100)
  tinh_thanh varchar(100)
  so_tang integer
  ngay_chot_hang_thang integer [not null]
  dang_hoat_dong boolean [not null, default: true]
  ghi_chu text

  indexes {
    chu_nha_id
    quan_ly_id
  }
  Note: 'Chủ nhà/Quản lý phải có role phù hợp. Một quản lý phụ trách nhiều tòa. Ngày chốt 1..31; đề xuất tháng ngắn dùng ngày cuối tháng. Kỳ hóa đơn theo tháng dương lịch. Không xóa tòa còn phòng.'
}

// 04. PHÒNG TRỌ
Table phong_tro {
  id integer [pk, increment]
  toa_nha_id integer [not null, ref: > toa_nha.id]
  ma_phong varchar(20) [not null]
  tang integer
  loai_phong varchar(50)
  dien_tich decimal(8,2) [not null]
  gia_thue bigint [not null, note: 'Giá niêm yết hiện tại; không thay giá hợp đồng cũ.']
  tien_coc_du_kien bigint [not null, default: 0]
  so_nguoi_toi_da integer [not null]
  trang_thai varchar(25) [not null, default: 'TRONG']
  mo_ta text
  ngay_tao datetime [not null]
  phien_ban integer [not null, default: 0]

  indexes {
    (toa_nha_id, ma_phong) [unique]
    (toa_nha_id, trang_thai)
  }
  Note: 'TRONG, DA_DAT_COC, DANG_THUE, NGUNG_CHO_THUE. DA_DAT_COC chỉ biểu thị giữ chỗ; tiền cọc thực thu nằm ở giao_dich_coc. CHECK diện tích >0, sức chứa >0, giá thuê >=500000, cọc dự kiến >=0. Ảnh đại diện là ảnh có thu_tu nhỏ nhất trong anh_phong. Cập nhật trạng thái và lịch sử cùng transaction.'
}

// 05. ẢNH PHÒNG
Table anh_phong {
  id integer [pk, increment]
  phong_id integer [not null, ref: > phong_tro.id]
  duong_dan varchar(500) [not null]
  duong_dan_anh_nho varchar(500)
  thu_tu integer [not null]
  mo_ta varchar(255)
  ngay_tao datetime [not null]

  indexes {
    (phong_id, thu_tu) [unique]
  }
  Note: 'Tối đa 8 ảnh/phòng; CHECK thu_tu 1..8. JPG/PNG, tối đa 5MB mỗi ảnh; thumbnail 400px theo nguồn. Kiểm tra nội dung tệp và xử lý xóa tệp sau thay đổi DB có cơ chế retry. Đổi thứ tự phải tránh va unique tạm thời.'
}

// 06. DANH MỤC DỊCH VỤ
Table dich_vu {
  id integer [pk, increment]
  ma_dich_vu varchar(20) [not null, unique]
  ten_dich_vu varchar(100) [not null]
  mo_ta text
  dang_hoat_dong boolean [not null, default: true]

  Note: 'Seed DIEN, NUOC, RAC, GUI_XE, INTERNET. Có thể bổ sung dịch vụ. Không xóa khi đã được cấu hình/hóa đơn tham chiếu.'
}

// 07. CẤU HÌNH VÀ LỊCH SỬ GIÁ DỊCH VỤ
Table cau_hinh_dich_vu {
  id integer [pk, increment]
  toa_nha_id integer [not null, ref: > toa_nha.id]
  phong_id integer [ref: > phong_tro.id]
  dich_vu_id integer [not null, ref: > dich_vu.id]
  cach_tinh varchar(25) [not null]
  don_vi_tinh varchar(30) [not null]
  don_gia bigint [not null]
  tu_ngay date [not null]
  den_ngay date
  dang_ap_dung boolean [not null, default: true]
  nguoi_tao_id integer [not null, ref: > tai_khoan.id]
  ngay_tao datetime [not null]

  indexes {
    (toa_nha_id, dich_vu_id, tu_ngay)
    (phong_id, dich_vu_id, tu_ngay)
  }
  Note: '''
  THEO_CHI_SO, THEO_NGUOI, CO_DINH. phong_id NULL: mặc định tòa; có giá trị: cấu hình
  riêng của phòng, phải thuộc toa_nha_id. Giá riêng hiệu lực được ưu tiên trước giá tòa.
  Cấu hình riêng dang_ap_dung=false nghĩa là ngừng dịch vụ, không rơi về giá mặc định.
  Thay giá bằng phiên bản mới, đóng den_ngay cũ vào ngày trước tu_ngay mới.
  Cấm chồng khoảng trong cùng phạm vi/dịch vụ; cần kiểm tra logic nghiệp vụ vì SQLite không có ràng buộc EXCLUDE.
  Cần UNIQUE có điều kiện riêng cho (toa_nha,dich_vu,tu_ngay) khi phong NULL và
  (phong,dich_vu,tu_ngay) khi phong NOT NULL. Không dùng unique với NULL rồi cho rằng đủ.
  CHECK den_ngay>=tu_ngay nếu có; đơn giá không âm, điện/nước đang áp dụng phải >0.
  Đổi cách tính hoặc ngừng áp dụng từ kỳ sau. Invoice chọn cấu hình tại ngày chốt,
  rồi snapshot giá/cách tính. Không tự sửa giá lịch sử đã sử dụng.
  '''
}

// 08. TIN ĐĂNG
Table tin_dang {
  id integer [pk, increment]
  phong_id integer [not null, ref: > phong_tro.id]
  nguoi_dang_id integer [not null, ref: > tai_khoan.id]
  tin_goc_id integer [ref: > tin_dang.id]
  tieu_de varchar(200) [not null]
  noi_dung text
  ngay_dang datetime
  ngay_het_han datetime
  trang_thai varchar(25) [not null, default: 'NHAP']
  ngay_tao datetime [not null]

  indexes {
    (phong_id, trang_thai)
    (trang_thai, ngay_het_han)
  }
  Note: 'NHAP, DANG_HIEN_THI, TAM_AN, DA_CHO_THUE. Cần unique có điều kiện theo phong_id khi DANG_HIEN_THI. Chỉ đăng phòng TRONG, hạn mặc định 30 ngày. Public query kiểm tra cả trạng thái phòng, tin và hạn. Giá/ảnh lấy từ phòng hiện tại; không có lịch sử ảnh tin riêng. Trả phòng tạo tin NHAP mới liên kết tin_goc, chủ nhà tự duyệt đăng.'
}

// 09. YÊU CẦU THUÊ / XEM PHÒNG
Table yeu_cau_thue {
  id integer [pk, increment]
  ma_yeu_cau varchar(30) [not null, unique]
  tin_dang_id integer [not null, ref: > tin_dang.id]
  khach_thue_id integer [not null, ref: > khach_thue.id]
  loai_yeu_cau varchar(20) [not null]
  ngay_mong_muon date [not null]
  so_nguoi_du_kien integer [not null]
  loi_nhan text
  lich_hen datetime
  trang_thai varchar(25) [not null, default: 'MOI']
  ly_do_tu_choi text
  nguoi_xu_ly_id integer [ref: > tai_khoan.id]
  ngay_xu_ly datetime
  ngay_tao datetime [not null]
  phien_ban integer [not null, default: 0]

  indexes {
    (khach_thue_id, ngay_tao)
    (tin_dang_id, trang_thai)
  }
  Note: 'XEM_PHONG hoặc THUE_NGAY. MOI, DA_HEN_LICH, DA_DUYET, TU_CHOI, DA_HUY. Unique có điều kiện khách/tin với trạng thái MOI/DA_HEN_LICH/DA_DUYET. Ngày mong muốn từ hôm nay đến +60 ngày; số người 1..sức chứa. Khách chỉ tự hủy MOI/DA_HEN_LICH. Duyệt thuê kiểm tra lại phòng còn trống, giữ chỗ cùng transaction. Lịch sử đổi trạng thái/lịch hẹn nằm trong nhat_ky_hoat_dong, khách chỉ được xem các trường nghiệp vụ đã lọc.'
}

// 10. HỢP ĐỒNG — thông tin chung, không ghi đè lịch sử giá gia hạn.
Table hop_dong {
  id integer [pk, increment]
  ma_hop_dong varchar(30) [not null, unique]
  phong_id integer [not null, ref: > phong_tro.id]
  khach_dung_ten_id integer [not null, ref: > khach_thue.id]
  yeu_cau_thue_id integer [unique, ref: - yeu_cau_thue.id]
  tien_coc_thoa_thuan bigint [not null, default: 0]
  dieu_khoan text
  trang_thai varchar(25) [not null, default: 'NHAP']
  ngay_kich_hoat datetime
  ngay_tra_phong date
  ly_do_ket_thuc text
  nguoi_lap_id integer [not null, ref: > tai_khoan.id]
  ngay_tao datetime [not null]
  phien_ban integer [not null, default: 0]

  indexes {
    (phong_id, trang_thai)
    (khach_dung_ten_id, trang_thai)
  }
  Note: '''
  NHAP, CHO_HIEU_LUC, DANG_HIEU_LUC, DA_KET_THUC, DA_HUY.
  Ngày bắt đầu/kết thúc và giá nằm ở ky_hop_dong; không có nguồn giá thứ hai ở đây.
  Cọc thỏa thuận 0..3 tháng giá kỳ đầu, không phải tiền thực nhận.
  Trước ký phải có kỳ đầu và đúng người/phòng của yêu cầu được duyệt nếu có yêu cầu nguồn.
  Cấm khoảng sử dụng phòng chồng nhau giữa các hợp đồng đã chốt, kể cả giữ lịch sử kết thúc;
  ngày trả sớm giới hạn khoảng ở thực tế, bản hủy không chiếm phòng.
  CHO_HIEU_LUC giữ chỗ; kích hoạt đổi phòng/tin trong cùng transaction.
  Một người đứng tên, không lặp ở bảng người ở ghép. Sau DA_KET_THUC không tạo hóa đơn mới,
  vẫn cho nhận tiền trả công nợ cũ. Phải lập đủ hóa đơn cuối trước khi đóng.
  '''
}

// 11. KỲ HỢP ĐỒNG — kỳ gốc và từng lần gia hạn.
Table ky_hop_dong {
  id integer [pk, increment]
  hop_dong_id integer [not null, ref: > hop_dong.id]
  so_thu_tu integer [not null]
  ngay_bat_dau date [not null]
  ngay_ket_thuc date [not null]
  so_thang integer [not null]
  gia_thue bigint [not null]
  thong_tin_chot text [note: 'JSON snapshot điều khoản, người ký và dịch vụ/giá khi ký; không dùng tính hóa đơn live.']
  tep_hop_dong varchar(500)
  nguoi_lap_id integer [not null, ref: > tai_khoan.id]
  ngay_tao datetime [not null]
  ghi_chu text

  indexes {
    (hop_dong_id, so_thu_tu) [unique]
    (hop_dong_id, ngay_bat_dau) [unique]
  }
  Note: 'CHECK số thứ tự, số tháng, giá >0; kết thúc >=bắt đầu. Kỳ 1 là hợp đồng ban đầu. Gia hạn nối tiếp ngày sau kỳ trước; không chồng/đứt quãng. Đề xuất ngày kết thúc = cộng số tháng rồi trừ 1 ngày; cần chốt ngày cuối tháng. Giá kỳ giao cắt ngày gia hạn cần chia đoạn hoặc chính sách PO xác nhận. PDF đã ký không tái tạo từ giá/hồ sơ mới; snapshot nhạy cảm phải bảo vệ.'
}

// 12. NGƯỜI Ở GHÉP
Table nguoi_o_ghep {
  id integer [pk, increment]
  hop_dong_id integer [not null, ref: > hop_dong.id]
  khach_thue_id integer [not null, ref: > khach_thue.id]
  ngay_vao date [not null]
  ngay_chuyen_di date
  ky_bat_dau_tinh_phi date [not null, note: 'Ngày đầu tháng bắt đầu tính phí đầu người.']
  ky_ngung_tinh_phi date [note: 'Ngày đầu tháng đầu tiên không tính người này.']
  ghi_chu text

  indexes {
    (hop_dong_id, khach_thue_id, ngay_vao) [unique]
    (khach_thue_id, ngay_vao)
  }
  Note: 'Không lặp người đứng tên, không chồng khoảng ở của cùng người/hợp đồng. Kiểm tra sức chứa tại mọi mốc ngày thay đổi, gồm người đứng tên. Người chuyển đi giảm phí từ kỳ sau; đề xuất người mới cũng tăng từ kỳ sau nhưng PO cần chốt. Ngày ở thực và kỳ tính phí là hai thông tin khác nhau. Người có tài khoản xem được hợp đồng liên quan; quyền hóa đơn mặc định chỉ người đứng tên.'
}

// 13. CHỈ SỐ ĐIỆN NƯỚC
Table chi_so_dien_nuoc {
  id integer [pk, increment]
  hop_dong_id integer [not null, ref: > hop_dong.id]
  dich_vu_id integer [not null, ref: > dich_vu.id]
  tu_ngay date [not null]
  den_ngay date [not null]
  chi_so_dau decimal(14,3) [not null]
  chi_so_cuoi decimal(14,3) [not null]
  da_khoa boolean [not null, default: false]
  da_xac_nhan_bat_thuong boolean [not null, default: false]
  nguoi_nhap_id integer [not null, ref: > tai_khoan.id]
  ngay_nhap datetime [not null]
  phien_ban integer [not null, default: 0]
  ghi_chu text

  indexes {
    (hop_dong_id, dich_vu_id, tu_ngay, den_ngay) [unique]
  }
  Note: 'Khoảng ngày là khoảng tiêu thụ bao gồm hai đầu; các khoảng liên tiếp nối ngày. CHECK đầu>=0, cuối>=đầu, đến>=từ. Chỉ dùng dịch vụ THEO_CHI_SO đang áp dụng cho phòng. Không chồng khoảng, chỉ số đầu khớp cuối kỳ trước hoặc bàn giao. Khóa bản đã dùng khi phát hành; phòng chưa ghi vẫn nhập được. Điều chỉnh bản khóa/reset đồng hồ cần quy trình riêng, chưa hỗ trợ mặc định. Cảnh báo 200% so trung bình 3 kỳ cần PO chốt công thức và xử lý thiếu lịch sử.'
}

// 14. HÓA ĐƠN
Table hoa_don {
  id integer [pk, increment]
  ma_hoa_don varchar(30) [not null, unique]
  hop_dong_id integer [not null, ref: > hop_dong.id]
  thang integer [not null]
  nam integer [not null]
  tu_ngay date [not null]
  den_ngay date [not null]
  ngay_chot date [not null, note: 'Ngày chọn phiên bản đơn giá dịch vụ.']
  so_nguoi_tinh_phi integer [not null]
  loai_hoa_don varchar(20) [not null, default: 'DINH_KY']
  ngay_lap datetime [not null]
  ngay_phat_hanh datetime
  han_thanh_toan date [not null]
  tong_tien bigint [not null, default: 0]
  trang_thai varchar(25) [not null, default: 'NHAP']
  thay_the_hoa_don_id integer [unique, ref: > hoa_don.id]
  nguoi_lap_id integer [not null, ref: > tai_khoan.id]
  nguoi_phat_hanh_id integer [ref: > tai_khoan.id]
  nguoi_huy_id integer [ref: > tai_khoan.id]
  ngay_huy datetime
  ly_do_huy text
  phien_ban integer [not null, default: 0]
  ghi_chu text

  indexes {
    (hop_dong_id, nam, thang)
    (trang_thai, han_thanh_toan)
  }
  Note: '''
  DINH_KY, KY_CUOI. NHAP, DA_PHAT_HANH, DA_HUY. Unique có điều kiện
  (hop_dong_id,nam,thang) khi trạng thái khác DA_HUY, không thêm loại hóa đơn vào khóa.
  CHECK tháng 1..12, tổng>=0, ngày hợp lệ; khoảng tính phí thuộc kỳ và thời gian thuê.
  Chỉ nháp sửa được; phát hành khóa nội dung/dòng, ghi người/thời điểm và tạo thông báo.
  Thay thế phải cùng hợp đồng/kỳ, bản nguồn đã hủy. Không tự hủy bản có khoản thu hoặc
  cấn cọc đã xác nhận. Kỳ cuối cùng tháng phải xử lý bản định kỳ, tránh thu hai lần.
  Hạn mặc định ngày phát hành +7 ngày. Tổng từ chi tiết, không nhập độc lập.
  Đã trả/trả một phần/chờ xác nhận/quá hạn tính riêng, không gộp vào trạng thái chứng từ.
  Dư nợ = tổng - thanh toán xác nhận - giao dịch cọc CAN_TRU xác nhận.
  '''
}

// 15. CHI TIẾT HÓA ĐƠN
Table chi_tiet_hoa_don {
  id integer [pk, increment]
  hoa_don_id integer [not null, ref: > hoa_don.id]
  so_thu_tu integer [not null]
  dich_vu_id integer [ref: > dich_vu.id]
  cau_hinh_dich_vu_id integer [ref: > cau_hinh_dich_vu.id]
  chi_so_id integer [ref: > chi_so_dien_nuoc.id]
  ky_hop_dong_id integer [ref: > ky_hop_dong.id]
  bao_hong_id integer [ref: > bao_hong.id]
  loai_khoan varchar(20) [not null]
  ten_khoan varchar(150) [not null]
  cach_tinh_ap_dung varchar(25)
  don_vi_tinh varchar(30)
  so_luong decimal(14,3) [not null]
  don_gia bigint [not null]
  chi_so_dau decimal(14,3)
  chi_so_cuoi decimal(14,3)
  so_ngay_tinh_tien integer
  so_ngay_trong_thang integer
  thanh_tien bigint [not null]
  ghi_chu text

  indexes {
    (hoa_don_id, so_thu_tu) [unique]
    bao_hong_id
  }
  Note: '''
  TIEN_PHONG, DICH_VU, PHAT_SINH, GIAM_TRU. CHECK số lượng/đơn giá/thành tiền>=0.
  GIAM_TRU lưu số dương và TRỪ khi cộng tổng, không vừa lưu âm vừa trừ.
  Lưu snapshot tên/cách tính/giá/số lượng/chỉ số. Nguồn chỉ số/cấu hình/kỳ thuê
  phải cùng hợp đồng/phòng/dịch vụ tương ứng; FK riêng không tự bảo đảm điều này.
  Làm tròn từng dòng tới đồng rồi cộng. Tiền phòng chia ngày tính trực tiếp
  giá tháng*ngày ở/ngày tháng bằng Decimal rồi làm tròn, không nhân tỷ lệ đã làm tròn 3 số.
  Phí bảo trì một lần trên các hóa đơn chưa hủy; bản thay thế có thể giữ nguồn phí.
  Hư hỏng/phạt khi trả phòng cũng thành dòng PHAT_SINH, không khấu trừ thêm ở sổ khác.
  Dòng giảm trừ/phát sinh yêu cầu ghi chú. Nội dung bất biến sau phát hành.
  '''
}

// 16. THANH TOÁN — báo trả và xác nhận thu trong cùng bảng.
Table thanh_toan {
  id integer [pk, increment]
  ma_thanh_toan varchar(30) [not null, unique]
  khoa_chong_trung varchar(100) [not null, unique]
  hoa_don_id integer [not null, ref: > hoa_don.id]
  so_tien bigint [not null]
  ngay_thanh_toan datetime [not null]
  hinh_thuc varchar(25) [not null]
  ma_giao_dich varchar(100)
  anh_chung_tu varchar(500)
  trang_thai varchar(25) [not null, default: 'CHO_XAC_NHAN']
  nguoi_bao_id integer [not null, ref: > tai_khoan.id]
  ngay_bao datetime [not null]
  nguoi_xac_nhan_id integer [ref: > tai_khoan.id]
  ngay_xac_nhan datetime
  ly_do_tu_choi_huy text
  nguoi_huy_id integer [ref: > tai_khoan.id]
  ngay_huy datetime
  ghi_chu text

  indexes {
    (hoa_don_id, trang_thai)
    (trang_thai, ngay_thanh_toan)
  }
  Note: 'Hình thức chỉ TIEN_MAT, CHUYEN_KHOAN. Cấn trừ ở giao_dich_coc, không ghi trùng vào đây. CHO_XAC_NHAN, DA_XAC_NHAN, TU_CHOI, DA_HUY. CHECK tiền>0. Chỉ xác nhận làm giảm nợ; nội dung báo đã gửi không sửa. Khách hủy khoản của mình còn chờ; chủ nhà hủy khoản ghi nhầm có lý do. Kiểm tra lại dư nợ khi xác nhận, không thu vượt, không xử lý hai lần. Hủy ghi nhầm không đồng nghĩa đã hoàn tiền thật. Chứng từ private, chủ nhà liên quan/Admin mới xem.'
}

// 17. SỔ CỌC — không là tiền thu hóa đơn lần hai.
Table giao_dich_coc {
  id integer [pk, increment]
  ma_giao_dich_coc varchar(30) [not null, unique]
  khoa_chong_trung varchar(100) [not null, unique]
  hop_dong_id integer [not null, ref: > hop_dong.id]
  hoa_don_id integer [ref: > hoa_don.id]
  loai_giao_dich varchar(20) [not null]
  so_tien bigint [not null]
  hinh_thuc varchar(25)
  ngay_giao_dich datetime [not null]
  trang_thai varchar(25) [not null, default: 'DA_XAC_NHAN']
  nguoi_thuc_hien_id integer [not null, ref: > tai_khoan.id]
  nguoi_huy_id integer [ref: > tai_khoan.id]
  ngay_huy datetime
  ly_do_huy text
  ghi_chu text

  indexes {
    (hop_dong_id, trang_thai, ngay_giao_dich)
    (hoa_don_id, trang_thai)
  }
  Note: '''
  THU_COC, HOAN_COC, CAN_TRU. DA_XAC_NHAN, DA_HUY. Tiền>0.
  CAN_TRU bắt buộc hoa_don cùng hợp đồng, hình thức NULL; THU/HOAN không có hoa_don,
  hình thức TIEN_MAT/CHUYEN_KHOAN. Chỉ chủ nhà được ghi xác nhận trong phạm vi.
  Cọc giữ = tổng THU_COC - HOAN_COC - CAN_TRU đã xác nhận, không được âm.
  Cấn trừ đồng thời không vượt cọc giữ và dư nợ hóa đơn; hủy thu/hoàn/cấn phải
  kiểm tra lại các bất biến, không tự đảo một khoản làm sai quyết toán đã chốt.
  Thu nhiều lần được hỗ trợ. Cọc thỏa thuận và thực nhận là hai số khác nhau.
  Hư hỏng/phạt nằm trong hóa đơn cuối rồi dùng CAN_TRU, không thêm loại khấu trừ trực tiếp.
  '''
}

// 18. BÁO HỎNG — dùng ba ảnh cố định để giữ mô hình gọn.
Table bao_hong {
  id integer [pk, increment]
  ma_bao_hong varchar(30) [not null, unique]
  hop_dong_id integer [not null, ref: > hop_dong.id]
  nguoi_bao_id integer [not null, ref: > tai_khoan.id]
  nguoi_phu_trach_id integer [ref: > tai_khoan.id]
  loai_thiet_bi varchar(100) [not null]
  mo_ta text [not null]
  muc_do varchar(20) [not null, default: 'THUONG']
  trang_thai varchar(25) [not null, default: 'MOI']
  anh_1 varchar(500)
  anh_2 varchar(500)
  anh_3 varchar(500)
  chi_phi bigint [not null, default: 0]
  ben_chiu_phi varchar(20)
  dua_vao_hoa_don boolean [not null, default: false]
  ky_tinh_phi date
  phan_hoi text
  ngay_tao datetime [not null]
  ngay_hoan_thanh datetime
  phien_ban integer [not null, default: 0]

  indexes {
    (hop_dong_id, trang_thai)
    (trang_thai, muc_do, ngay_tao)
  }
  Note: 'Mức THUONG/GAP. Trạng thái MOI, DANG_XU_LY, HOAN_THANH, TU_CHOI. MOI->DANG_XU_LY->HOAN_THANH hoặc từ chối trước hoàn thành. Bên chịu CHU_NHA/KHACH_THUE; chi phí>=0. Chỉ người đang ở theo hợp đồng hiệu lực được báo. Mỗi đổi trạng thái có ghi chú vào nhật ký; phan_hoi là nội dung mới nhất. Một tổng phí/báo hỏng; chỉ đưa hóa đơn nếu khách chịu và chưa tính trên chứng từ hiệu lực. Mỗi ảnh <=5MB, private.'
}

// 19. THÔNG BÁO — in-app + trạng thái email đơn giản.
Table thong_bao {
  id integer [pk, increment]
  nguoi_nhan_id integer [not null, ref: > tai_khoan.id]
  hoa_don_id integer [not null, ref: > hoa_don.id]
  loai_thong_bao varchar(30) [not null]
  tieu_de varchar(200) [not null]
  noi_dung text [not null]
  duong_dan varchar(500) [not null]
  email_nhan varchar(254) [not null]
  ngay_tao datetime [not null]
  ngay_doc datetime
  trang_thai_email varchar(25) [not null, default: 'CHO_GUI']
  so_lan_gui integer [not null, default: 0]
  lan_gui_tiep_theo datetime
  khoa_xu_ly_den datetime
  ngay_gui_thanh_cong datetime
  loi_gan_nhat text

  indexes {
    (hoa_don_id, nguoi_nhan_id, loai_thong_bao) [unique]
    (nguoi_nhan_id, ngay_doc)
    (trang_thai_email, lan_gui_tiep_theo)
  }
  Note: 'Loại HOA_DON_MOI, TRUOC_HAN_2_NGAY, QUA_HAN. Email CHO_GUI, DANG_GUI, DA_GUI, THAT_BAI, DA_HUY. Tạo thông báo cùng transaction phát hành; management command xử lý sau commit, nhắc hạn 08:00 giờ Việt Nam. Chống trùng sự kiện bằng unique, không nhắc invoice hủy/đã trả đủ. Lưu lịch sử lần gửi vào nhật ký để bảng này chỉ giữ trạng thái cuối. Đề xuất 1 lần đầu +3 thử lại, cần PO chốt. SMTP không bảo đảm tuyệt đối không email lặp khi sự cố sau gửi. Chỉ người đứng tên nhận nhắc mặc định.'
}

// 20. NHẬT KÝ CHUNG — thay nhiều bảng lịch sử nhỏ.
Table nhat_ky_hoat_dong {
  id integer [pk, increment]
  nguoi_thuc_hien_id integer [ref: > tai_khoan.id]
  vai_tro_luc_thuc_hien varchar(20)
  loai_doi_tuong varchar(50) [not null]
  doi_tuong_id integer [not null]
  hanh_dong varchar(50) [not null]
  du_lieu_truoc text [note: 'JSON đã loại bí mật/giấy tờ rõ.']
  du_lieu_sau text [note: 'JSON đã loại bí mật/giấy tờ rõ.']
  ghi_chu text
  thoi_diem datetime [not null]

  indexes {
    (loai_doi_tuong, doi_tuong_id, thoi_diem)
    (nguoi_thuc_hien_id, thoi_diem)
  }
  Note: 'Chỉ thêm, không cho sửa/xóa qua ứng dụng, kể cả Admin. Ghi cùng transaction thay đổi nghiệp vụ. Tham chiếu đa hình loai_doi_tuong/doi_tuong_id không có FK chung; service chịu trách nhiệm. Lịch sử yêu cầu, bảo trì, gửi email có thể dùng chung bảng này. Chỉ trả các trường nghiệp vụ được phép cho khách, không mở toàn bộ audit. Không lưu password/token, giấy tờ rõ hoặc URL ký. Người có quyền trực tiếp sửa file SQLite nằm ngoài bảo vệ của role ứng dụng.'
}

// 21. THANH LÝ HỢP ĐỒNG — snapshot lúc chốt, không tạo sổ nợ thứ hai.
Table thanh_ly_hop_dong {
  id integer [pk, increment]
  hop_dong_id integer [not null, unique, ref: - hop_dong.id]
  hoa_don_cuoi_id integer [ref: > hoa_don.id]
  ngay_tra_phong date [not null]
  ket_thuc_truoc_han boolean [not null, default: false]
  ly_do_ket_thuc text
  ap_dung_phat boolean [not null, default: false]
  coc_con_giu_truoc_chot bigint [not null, default: 0]
  tong_no_truoc_can_tru bigint [not null, default: 0]
  tien_hu_hong bigint [not null, default: 0]
  tien_phat bigint [not null, default: 0]
  tien_coc_can_tru bigint [not null, default: 0]
  tien_phai_hoan bigint [not null, default: 0]
  tien_khach_tra_them bigint [not null, default: 0]
  chi_tiet_chot text [note: 'JSON snapshot công nợ từng invoice và khoản hư hỏng/phạt đã nằm trong invoice.']
  trang_thai varchar(20) [not null, default: 'NHAP']
  tep_bien_ban varchar(500)
  nguoi_lap_id integer [not null, ref: > tai_khoan.id]
  ngay_tao datetime [not null]
  nguoi_chot_id integer [ref: > tai_khoan.id]
  ngay_chot datetime

  Note: '''
  NHAP, DA_CHOT. Mọi số tiền>=0. Khi chốt cần hóa đơn KY_CUOI đã phát hành cùng hợp đồng.
  Tổng nợ bao gồm hư hỏng/phạt đã vào hóa đơn; hai trường hư hỏng/phạt chỉ phân tích,
  KHÔNG cộng thêm một lần vào tổng nợ. Phải hoàn=max(cọc trước chốt-tổng nợ,0);
  khách trả thêm=max(tổng nợ-cọc trước chốt,0); cấn trừ=min(cọc,tổng nợ), phân vào
  từng invoice bằng giao_dich_coc. Snapshot không thay số liệu nguồn và không cập nhật
  âm thầm khi khách trả thêm sau này. Tiền hoàn thực tế nằm ở sổ cọc.
  DA_CHOT là chốt nghĩa vụ, chưa đồng nghĩa đã thu/hoàn tiền xong; cho đóng hợp đồng
  và truy thu nợ cũ là đề xuất cần PO xác nhận. Chốt, đóng hợp đồng, cập nhật phòng,
  lịch sử phòng và tin nháp mới cùng transaction; kiểm tra phòng không vướng hợp đồng khác.
  Người đứng tên được tải biên bản sau kết thúc hợp đồng. Biên bản đã chốt bất biến.
  '''
}

// 22. LỊCH SỬ TRẠNG THÁI PHÒNG — báo cáo lấp đầy tại thời điểm quá khứ.
Table lich_su_trang_thai_phong {
  id integer [pk, increment]
  phong_id integer [not null, ref: > phong_tro.id]
  hop_dong_id integer [ref: > hop_dong.id]
  trang_thai varchar(25) [not null]
  tu_thoi_diem datetime [not null]
  den_thoi_diem datetime
  nguoi_thuc_hien_id integer [ref: > tai_khoan.id]
  ly_do text

  indexes {
    (phong_id, tu_thoi_diem) [unique]
  }
  Note: 'Enum như phong_tro. Riêng khoảng THỜI ĐIỂM dùng [từ,đến), đến NULL là hiện tại. Cần unique có điều kiện phong khi đến NULL, cấm chồng/đứt lịch sử từ lúc tạo phòng. Đổi trạng thái đóng bản cũ và mở bản mới cùng transaction. Lấp đầy cuối tháng: DANG_THUE / (TRONG+DA_DAT_COC+DANG_THUE) tại thời điểm chốt; loại NGUNG_CHO_THUE. Mẫu số 0 hiển thị không áp dụng. Không sửa báo cáo cũ dựa trên trạng thái hiện tại.'
}

```
