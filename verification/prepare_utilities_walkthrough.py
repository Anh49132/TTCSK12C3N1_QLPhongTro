"""Create fresh synthetic utility demo plus ignored walkthrough with random test credentials."""
import datetime as dt
import html
import json
from pathlib import Path
import re
import subprocess
import sys
sys.dont_write_bytecode = True
import prepare_service_demo as demo


def main():
    subprocess.run([sys.executable, str(demo.ROOT / 'verification/utilities_http.py'), '--serve'],
                   cwd=demo.ROOT, check=True)
    folder = Path((demo.ROOT / 'data/service-demo/latest.txt').read_text(encoding='utf-8').strip())
    access = json.loads((folder / 'access.json').read_text(encoding='utf-8'))
    demo.BASE = access['url']
    browser = demo.Browser()
    assert browser.post('/Account/Login', {'TaiKhoanDangNhap':access['accounts']['CHU_NHA'][1],
                                          'MatKhau':access['password']})[0] == 302
    assert browser.post('/PhongTro/TaoToaNha', {'TenToaNha':'Demo thử từng tiêu chí điện nước',
                                              'DiaChi':'Địa chỉ giả phục vụ kiểm thử', 'SoTang':2})[0] == 302
    import sqlite3
    database = Path(access['database'])
    with sqlite3.connect(database.as_uri() + '?mode=ro', uri=True) as connection:
        building = connection.execute('SELECT MAX(id) FROM toa_nha WHERE chu_nha_id=?',
                                      (access['accounts']['CHU_NHA'][0],)).fetchone()[0]
    for code, method, price in [('DIEN','THEO_CHI_SO',3500), ('NUOC','THEO_NGUOI',25000)]:
        route = f'/DichVu/DienNuoc?toaNhaId={building}&maDichVu={code}'
        status, body, _ = browser.request(route)
        assert status == 200
        fields = {key:html.unescape(re.search(r'name="' + key + r'"[^>]*value="([^"]*)"', body)[1])
                  for key in ['ToaNhaId','MaDichVu','PhienBan','KyApDung']}
        assert browser.post('/DichVu/DienNuoc', dict(fields, CachTinh=method, DonGia=price), route)[0] == 302
    today = dt.datetime.now(dt.timezone(dt.timedelta(hours=7))).date()
    next_month = (today.replace(day=28) + dt.timedelta(days=4)).replace(day=1)
    access['walkthrough_building'] = building
    (folder / 'access.json').write_text(json.dumps(access, ensure_ascii=False, indent=2), encoding='utf-8')
    accounts = '\n'.join(f'| {role} | `{account[1]}` |' for role, account in access['accounts'].items())
    guide = f'''# Demo điện/nước — dữ liệu giả, chỉ dùng local

Mở {demo.BASE}/Account/Login. Demo đang chạy PID {access['pid']}, database riêng `{database}`.
Không sử dụng database thật. Thông tin đăng nhập này chỉ lưu trong thư mục đã ignore.

| Vai trò | Tài khoản |
|---|---|
{accounts}

Mật khẩu chung của bốn tài khoản: `{access['password']}`.
Dùng **CHU_NHA** để thử cấu hình. ADMIN dùng xem nhật ký; QUAN_LY/KHACH_THUE không có quyền cấu hình điện/nước.

## Dữ liệu ban đầu

- Tòa nhà **Demo thử từng tiêu chí điện nước** (ID {building}): điện theo chỉ số **3.500 VND/kWh**, nước khoán đầu người **25.000 VND/người/tháng**; chưa có phiên bản kỳ sau.
- Tòa nhà đầu tiên có hóa đơn chỉ số: 10 kWh × 3.000 = 30.000 VND dịch vụ, tổng gồm tiền phòng **2.030.000 VND**. Hóa đơn kỳ sau theo đầu người: 2 × 50.000 = 100.000 VND dịch vụ, tổng **2.100.000 VND**.

## 1. Chọn cách tính riêng từng dịch vụ và từng tòa nhà

1. Vào {demo.BASE}/DichVu?toaNhaId={building}, chọn đúng tòa nhà demo.
2. Bấm **Cấu hình điện**: đang chọn **Theo chỉ số đồng hồ**.
3. Bấm **Cấu hình nước**: đang chọn **Khoán theo đầu người**.
4. Thử chuyển qua lại hai lựa chọn, chưa lưu. Chỉ có hai cách tính này.
5. Chọn tòa nhà khác: cấu hình không thay đổi theo lựa chọn chưa lưu ở tòa demo.

## 2. Đơn giá đúng đơn vị

1. Điện: chọn theo chỉ số → nhãn **Đơn giá một kWh (VND)**; nhập `3500`.
2. Nước: chọn theo chỉ số → nhãn **Đơn giá một m³ (VND)**; nhập `15000`.
3. Chọn khoán đầu người → nhãn **Số tiền một người mỗi tháng (VND)**; nhập `25000`.
4. Chưa bấm lưu để tiếp tục thử giá không hợp lệ.

## 3. Không lưu giá trống hoặc bằng 0 — thực hiện trước bước 4

1. Trên cấu hình nước tòa demo, xóa đơn giá, bấm **Lưu cấu hình** → bị chặn, không tạo dòng lịch sử.
2. Nhập `0`, bấm lưu → bị chặn, không tạo dòng lịch sử.
3. Có thể thử `-1` hoặc giá lẻ: cũng không hợp lệ.
4. Tải lại trang: đơn giá cũ **25.000**, lịch sử vẫn chỉ một phiên bản.

## 4. Đổi cách tính từ kỳ hóa đơn kế tiếp

1. Mở {demo.BASE}/DichVu/DienNuoc?toaNhaId={building}&maDichVu=NUOC.
2. Đổi từ đầu người sang **Theo chỉ số đồng hồ**, nhập `15000`.
3. Trước khi lưu, thông báo phải ghi **{next_month.strftime('%d/%m/%Y')}**, kỳ **{next_month.strftime('%m/%Y')}**. Không cho chọn áp dụng ngay.
4. Bấm lưu → lịch sử có hai dòng: đầu người 25.000 giữ đến **{(next_month-dt.timedelta(days=1)).strftime('%d/%m/%Y')}**; chỉ số 15.000/m³ bắt đầu **{next_month.strftime('%d/%m/%Y')}**.
5. Trở về danh sách: hiện cấu hình hiện hành cùng ghi chú có thay đổi đã lên lịch. Mở lại cấu hình nước: cảnh báo kỳ đã lên lịch, nút lưu bị vô hiệu để tránh ghi đè.
6. Với điện, có thể thử đổi từ chỉ số sang đầu người `50000`: kỳ áp dụng cũng là tháng kế tiếp.
7. Xem {demo.BASE}/HoaDonDichVu/Details/{access['invoice']} → hóa đơn cũ vẫn tổng **2.030.000 VND**, không đổi theo cấu hình mới.

## Chạy lại từ đầu

Mỗi dịch vụ chỉ lưu được một phiên bản cho kỳ kế tiếp. Muốn lặp lại từ dữ liệu sạch, chạy từ thư mục repository:

```powershell
python verification/prepare_utilities_walkthrough.py
```

Script tạo database và tài khoản giả mới, không ghi đè demo cũ. Dùng URL/tài khoản trong file hướng dẫn mới; mật khẩu mỗi lần khác nhau. Không cần build lại nếu mã chưa đổi.
Khi xong demo này: `Stop-Process -Id {access['pid']}`. Nếu build lại, dừng các demo đang dùng cùng DLL trước.
'''
    (folder / 'HUONG-DAN-TEST.md').write_text(guide, encoding='utf-8')
    print('Walkthrough: ' + str(folder / 'HUONG-DAN-TEST.md'))
    print('Demo building: ' + str(building))


if __name__ == '__main__':
    main()
