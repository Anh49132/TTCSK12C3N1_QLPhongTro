"""Tao du lieu demo S2-08 (yeu cau thue) tren mot ban sao moi; khong bao gio ghi de file co san.

S2-06 chua lam UI tao yeu cau, nen yeu cau thue phai duoc seed de co dung chuc nang xac nhan /
doi lich / tu choi / duyet thue ngay de kiem thu tay.
"""
import os
from pathlib import Path
import sqlite3
import subprocess
import sys
from datetime import datetime, timedelta, timezone

sys.stdout.reconfigure(encoding='utf-8')

root = Path(__file__).resolve().parents[1]
app = root / 'QL_PhongTro'
dll = app / 'bin/Debug/net10.0/QL_PhongTro.dll'
target = (root / (sys.argv[1] if len(sys.argv) > 1 else 'data/s2-08-demo.sqlite')).resolve()
if target.exists():
    raise SystemExit('Destination exists. Reuse it, or pass a NEW path. Nothing changed.')

# 7 gio = muc gio Viet Nam ma moi dau vao.
LUC_GIO_VN = timezone(timedelta(hours=7))
bay_ngay = datetime.now(LUC_GIO_VN).replace(minute=0, second=0, microsecond=0)
bay_ngay += timedelta(days=7 - bay_ngay.weekday())
day_bay_ngay = bay_ngay.strftime('%Y-%m-%d')


def gio_utc(vn):
    return vn.astimezone(timezone.utc).strftime('%Y-%m-%d %H:%M:%S')


def tao_ban_sao():
    nguon = app / 'Data' / 'local-dev.sqlite'
    if not nguon.exists():
        raise SystemExit(f'Missing source database: {nguon}')
    target.parent.mkdir(parents=True, exist_ok=True)
    src = sqlite3.connect(nguon)
    dst = sqlite3.connect(target)
    try:
        src.backup(dst)
    finally:
        src.close()
        dst.close()
    env = dict(os.environ)
    env['DatabasePath'] = str(target)
    subprocess.run(['dotnet', str(dll), '--update-database'], cwd=app, env=env, check=True)


def tai_khoan(c, ho_ten, email, vai_tro, mat_khau):
    c.execute("""INSERT INTO tai_khoan(ho_ten,email,so_dien_thoai,mat_khau,vai_tro,dang_hoat_dong,
                    ngay_tao,ngay_cap_nhat,must_change_password,email_confirmed,is_deleted)
                 VALUES(?,?,?,?,?,1,CURRENT_TIMESTAMP,CURRENT_TIMESTAMP,0,1,0)""",
              (ho_ten, email, '09' + str(abs(hash(email)) % 10 ** 8).zfill(8), mat_khau, vai_tro))
    return c.execute('SELECT last_insert_rowid()').fetchone()[0]


def seed():
    c = sqlite3.connect(target)
    try:
        mau = c.execute("SELECT mat_khau FROM tai_khoan WHERE email LIKE 'demo-quan_ly-%@example.test' "
                        "ORDER BY id DESC LIMIT 1").fetchone()
        if mau is None:
            mau = c.execute('SELECT mat_khau FROM tai_khoan WHERE vai_tro=? ORDER BY id LIMIT 1',
                            ('CHU_NHA',)).fetchone()
        if mau is None:
            raise SystemExit('No password hash available to reuse for the demo accounts.')
        mat_khau = mau[0]

        chu_nha = tai_khoan(c, 'Chủ nhà demo S2-08', 's2-08-demo-chunha@example.test', 'CHU_NHA', mat_khau)
        khach = tai_khoan(c, 'Khách demo S2-08', 's2-08-demo-khach@example.test', 'KHACH_THUE', mat_khau)

        c.execute("""INSERT INTO toa_nha(chu_nha_id,ten_toa_nha,dia_chi,ngay_chot_hang_thang,dang_hoat_dong)
                     VALUES(?,'Tòa nhà demo S2-08','1 Đường Demo, Quận 1',1,1)""", (chu_nha,))
        toa_nha = c.execute('SELECT last_insert_rowid()').fetchone()[0]
        c.execute("""INSERT INTO toa_nha(chu_nha_id,ten_toa_nha,dia_chi,ngay_chot_hang_thang,dang_hoat_dong)
                     VALUES(?,'Tòa nhà khác demo S2-08','2 Đường Demo, Quận 1',1,1)""", (chu_nha,))

        for ma_phong, tang, gia in (('A101', 1, 3000000), ('A201', 2, 3200000), ('B201', 3, 3500000)):
            c.execute("""INSERT INTO phong_tro(toa_nha_id,ma_phong,tang,dien_tich,gia_thue,tien_coc_du_kien,
                             so_nguoi_toi_da,trang_thai,ngay_tao,phien_ban)
                         VALUES(?,?,?,20,?,1000000,3,'TRONG',CURRENT_TIMESTAMP,0)""",
                      (toa_nha, ma_phong, tang, gia))
        ma_theo_id = dict(c.execute('SELECT ma_phong,id FROM phong_tro WHERE toa_nha_id=?', (toa_nha,)))
        phong_a, phong_b = ma_theo_id['A101'], ma_theo_id['B201']

        c.execute("""INSERT INTO khach_thue(tai_khoan_id,ho_ten,ngay_tao) VALUES(?,?,CURRENT_TIMESTAMP)""",
                  (khach, 'Khách demo S2-08'))
        khach_thue = c.execute('SELECT last_insert_rowid()').fetchone()[0]

        def yeu_cau(ma, phong, loai, trang_thai, lich_hen_vn=None, so_nguoi=2):
            c.execute("""INSERT INTO yeu_cau_thue(ma_yeu_cau,phong_id,khach_thue_id,loai_yeu_cau,ngay_mong_muon,
                             so_nguoi_du_kien,lich_hen,trang_thai,ngay_tao,phien_ban)
                         VALUES(?,?,?,?,?,?,?,?,CURRENT_TIMESTAMP,0)""",
                      (ma, phong, khach_thue, loai, day_bay_ngay, so_nguoi,
                       gio_utc(lich_hen_vn) if lich_hen_vn else None, trang_thai))
            return c.execute('SELECT last_insert_rowid()').fetchone()[0]

        # 09:00 va 09:30 Viet Nam cung mot phong: dung bang bien "trung lich" 30 phut.
        trung = yeu_cau('YC-DEMO-01', phong_a, 'XEM_PHONG', 'DA_HEN_LICH', bay_ngay.replace(hour=9, minute=0))
        yeu_cau('YC-DEMO-02', phong_a, 'XEM_PHONG', 'MOI', bay_ngay.replace(hour=9, minute=30))
        # 14:00 van nam trong vung 30 phut so voi 09:30 theo hai chieu.
        yeu_cau('YC-DEMO-03', phong_a, 'XEM_PHONG', 'MOI', bay_ngay.replace(hour=14, minute=0))
        # 17:00 lech 2 gio 30: khong trung.
        yeu_cau('YC-DEMO-04', phong_a, 'THUE_NGAY', 'MOI', bay_ngay.replace(hour=17, minute=0))
        # Phong B chi co mot lich duy nhat, khong trung gi.
        yeu_cau('YC-DEMO-05', phong_b, 'XEM_PHONG', 'MOI', bay_ngay.replace(hour=10, minute=0))

        c.execute("""INSERT INTO yeu_cau_thue_lich_su(yeu_cau_thue_id,trang_thai_cu,trang_thai_moi,hanh_dong,
                         nguoi_thuc_hien_id,ten_nguoi_thuc_hien,vai_tro_luc_thuc_hien,thoi_diem)
                     VALUES(?,?,?,?,?,?,'CHU_NHA',CURRENT_TIMESTAMP)""",
                  (trung, 'MOI', 'DA_HEN_LICH', 'XAC_NHAN', chu_nha, 'Chủ nhà demo S2-08'))

        c.execute("""INSERT INTO thong_bao(nguoi_nhan_id,loai,tieu_de,noi_dung,duong_dan,da_doc,ngay_tao)
                     VALUES(?,'YEU_CAU_XAC_NHAN','Lịch hẹn đã được xác nhận',?,?,0,CURRENT_TIMESTAMP)""",
                  (khach, f'Chủ nhà đã xác nhận lịch hẹn cho yêu cầu YC-DEMO-01.', f'/YeuCauThue/Detail/{trung}'))
        c.commit()
        print('Demo ready:', target)
        print(f'Signed-in landlord: s2-08-demo-chunha@example.test (room A101/A201/B201)')
        print(f'Signed-in tenant:    s2-08-demo-khach@example.test')
        print(f'Next {day_bay_ngay} is a Monday. 09:00 and 09:30 in room A101 overlap by design.')
    finally:
        c.close()


tao_ban_sao()
seed()
print('Original database unchanged.')
