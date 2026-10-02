"""Verify utility configuration/invoice history on synthetic NEW databases only.
Build: dotnet build QL_PhongTro/QL_PhongTro.csproj -o data/service-demo/utilities-runtime
Run: python verification/utilities_http.py [--serve]
Secrets and fixtures stay under ignored data/service-demo; no production database is used.
"""
import contextlib
import datetime as dt
import html
import json
import os
from pathlib import Path
import re
import socket
import sqlite3
import subprocess
import sys
import uuid
sys.dont_write_bytecode = True
import prepare_service_demo as demo


def main():
    folder = demo.ROOT / 'data/service-demo' / ('utilities-' + uuid.uuid4().hex)
    folder.mkdir(parents=True, exist_ok=False)
    demo.DLL = demo.ROOT / 'data/service-demo/utilities-runtime/QL_PhongTro.dll'
    demo.SOURCE = folder / 'empty.sqlite'
    env = dict(os.environ, DatabasePath=str(demo.SOURCE), ASPNETCORE_ENVIRONMENT='Development')
    init = subprocess.run(['dotnet', str(demo.DLL), '--initialize-database'], cwd=demo.APP,
                          env=env, capture_output=True, text=True, timeout=90)
    assert init.returncode == 0, init.stdout + init.stderr
    with socket.socket() as probe:
        probe.bind(('127.0.0.1', 0))
        demo.BASE = f'http://localhost:{probe.getsockname()[1]}'
    # Demo helper creates fake accounts with a random password, fake contracts and a meter invoice.
    with (folder / 'setup.log').open('w', encoding='utf-8') as log, contextlib.redirect_stdout(log):
        demo.main()
    artifact = Path((demo.ROOT / 'data/service-demo/latest.txt').read_text(encoding='utf-8'))
    access = json.loads((artifact / 'access.json').read_text(encoding='utf-8'))
    database = Path(access['database'])
    process_id = access['pid']
    checks = []
    passed = False

    def query(sql, args=()):
        with sqlite3.connect(database.as_uri() + '?mode=ro', uri=True) as db:
            return db.execute(sql, args).fetchall()

    def check(name, condition):
        assert condition, name
        checks.append(name)

    try:
        browser = demo.Browser()
        check('owner login', browser.post('/Account/Login', {'TaiKhoanDangNhap':access['accounts']['CHU_NHA'][1],
                                                           'MatKhau':access['password']})[0] == 302)
        building = access['building']
        route = f'/DichVu/DienNuoc?toaNhaId={building}&maDichVu=DIEN'
        code, body, _ = browser.request(route)
        check('two calculation choices and displayed period', code == 200 and 'THEO_NGUOI' in body and 'THEO_CHI_SO' in body)
        fields = {key:html.unescape(re.search(r'name="' + key + r'"[^>]*value="([^"]*)"', body)[1])
                  for key in ['ToaNhaId','MaDichVu','PhienBan','KyApDung']}
        today = dt.datetime.now(dt.timezone(dt.timedelta(hours=7))).date()
        next_month = (today.replace(day=28) + dt.timedelta(days=4)).replace(day=1)
        check('next invoice month displayed before saving', fields['KyApDung'] == next_month.isoformat()
              and next_month.strftime('%d/%m/%Y') in html.unescape(body))
        before = query('SELECT * FROM cau_hinh_dich_vu ORDER BY id')
        snapshot = query('SELECT * FROM chi_tiet_hoa_don ORDER BY id')
        for price in ['', '0', '-1', '1.5']:
            response = browser.post('/DichVu/DienNuoc', dict(fields, CachTinh='THEO_NGUOI', DonGia=price), route)
            check('reject price ' + repr(price), response[0] == 200 and query('SELECT * FROM cau_hinh_dich_vu ORDER BY id') == before)
        for invalid in [dict(CachTinh='CO_DINH', DonGia=50000),
                        dict(CachTinh='THEO_NGUOI', DonGia=50000, KyApDung=today.isoformat())]:
            response = browser.post('/DichVu/DienNuoc', dict(fields, **invalid), route)
            check('reject invalid method/period ' + str(invalid), response[0] == 200 and query('SELECT * FROM cau_hinh_dich_vu ORDER BY id') == before)
        check('CSRF required', browser.request('/DichVu/DienNuoc', dict(fields, CachTinh='THEO_NGUOI', DonGia=50000))[0] == 400)
        check('save next-month per-person version', browser.post('/DichVu/DienNuoc', dict(fields, CachTinh='THEO_NGUOI', DonGia=50000), route)[0] == 302)
        rows = query("SELECT c.id,c.cach_tinh,c.don_vi_tinh,c.don_gia,c.tu_ngay,c.den_ngay FROM cau_hinh_dich_vu c JOIN dich_vu d ON d.id=c.dich_vu_id WHERE c.toa_nha_id=? AND d.ma_dich_vu='DIEN' ORDER BY c.tu_ngay", (building,))
        check('old meter version retained through current month', len(rows) == 2 and rows[0][1:4] == ('THEO_CHI_SO','kWh',3000)
              and rows[0][5] == (next_month - dt.timedelta(days=1)).isoformat())
        check('new version correct method/unit/price/period', rows[1][1:5] == ('THEO_NGUOI','người/tháng',50000,next_month.isoformat()))
        check('issued invoice snapshot unchanged', snapshot == query('SELECT * FROM chi_tiet_hoa_don ORDER BY id'))
        check('stale version rejected', browser.post('/DichVu/DienNuoc', dict(fields, CachTinh='THEO_CHI_SO', DonGia=4000), route)[0] == 200)
        # Issue next month's invoice using the actual per-person configuration.
        issue = {'ToaNhaId':building,'HopDongId':access['contracts'][0], 'NgayApDung':next_month.isoformat(), 'SoNguoi':2,
                 'Dong[0].Chon':'true','Dong[0].DichVuId':query("SELECT id FROM dich_vu WHERE ma_dich_vu='DIEN'")[0][0],
                 'Dong[0].CauHinhId':rows[1][0], 'Dong[0].DonGiaDaXem':50000}
        check('next-month invoice uses per-person calculation', browser.post('/HoaDonDichVu/Issue', issue,
              f'/HoaDonDichVu?toaNhaId={building}&ngayApDung={next_month.isoformat()}')[0] == 302)
        check('two people produce 100000 service charge', query('SELECT tong_tien FROM hoa_don ORDER BY id DESC LIMIT 1')[0][0] == 2100000)
        check('second building created', browser.post('/PhongTro/TaoToaNha', {'TenToaNha':'Utility second building',
              'DiaChi':'Synthetic address', 'SoTang':1})[0] == 302)
        second = query('SELECT MAX(id) FROM toa_nha')[0][0]
        check('defaults initialized', browser.post('/DichVu/Initialize', {'toaNhaId':second}, f'/DichVu?toaNhaId={second}')[0] == 302)
        check('defaults do not save unpriced utility configurations', not query("SELECT c.id FROM cau_hinh_dich_vu c JOIN dich_vu d ON c.dich_vu_id=d.id WHERE c.toa_nha_id=? AND d.ma_dich_vu IN ('DIEN','NUOC')", (second,)))
        second_route = f'/DichVu/DienNuoc?toaNhaId={second}&maDichVu=NUOC'
        initial = {'ToaNhaId':second,'MaDichVu':'NUOC','PhienBan':0,'KyApDung':today.isoformat(), 'CachTinh':'THEO_NGUOI','DonGia':30000}
        check('first water price per person saved', browser.post('/DichVu/DienNuoc', initial, second_route)[0] == 302)
        water = query("SELECT c.id,c.cach_tinh,c.don_vi_tinh FROM cau_hinh_dich_vu c JOIN dich_vu d ON c.dich_vu_id=d.id WHERE c.toa_nha_id=? AND d.ma_dich_vu='NUOC'", (second,))[0]
        check('water per-person unit', water[1:] == ('THEO_NGUOI','người/tháng'))
        change = dict(initial, PhienBan=water[0], KyApDung=next_month.isoformat(), CachTinh='THEO_CHI_SO', DonGia=15000)
        check('water changes to meter next month', browser.post('/DichVu/DienNuoc', change, second_route)[0] == 302)
        check('water meter unit cubic metres', query("SELECT c.don_vi_tinh FROM cau_hinh_dich_vu c JOIN dich_vu d ON c.dich_vu_id=d.id WHERE c.toa_nha_id=? AND d.ma_dich_vu='NUOC' ORDER BY c.tu_ngay DESC LIMIT 1", (second,)) == [('m³',)])
        check('first building electricity unaffected', rows == query("SELECT c.id,c.cach_tinh,c.don_vi_tinh,c.don_gia,c.tu_ngay,c.den_ngay FROM cau_hinh_dich_vu c JOIN dich_vu d ON d.id=c.dich_vu_id WHERE c.toa_nha_id=? AND d.ma_dich_vu='DIEN' ORDER BY c.tu_ngay", (building,)))
        latest_water = query("SELECT c.id FROM cau_hinh_dich_vu c JOIN dich_vu d ON c.dich_vu_id=d.id WHERE c.toa_nha_id=? AND d.ma_dich_vu='NUOC' ORDER BY c.tu_ngay DESC LIMIT 1", (second,))[0][0]
        check('scheduled period cannot be overwritten', browser.post('/DichVu/DienNuoc', dict(change, PhienBan=latest_water, DonGia=20000), second_route)[0] == 200)
        check('unknown building denied', browser.request('/DichVu/DienNuoc?toaNhaId=2147483647&maDichVu=NUOC')[0] == 403)
        check('invalid utility code denied', browser.request(f'/DichVu/DienNuoc?toaNhaId={building}&maDichVu=RAC')[0] == 400)
        check('integrity and foreign keys', query('PRAGMA integrity_check') == [('ok',)] and not query('PRAGMA foreign_key_check'))
        (artifact / 'utilities-result.json').write_text(json.dumps({'checks':checks,'passed':len(checks)}, ensure_ascii=False, indent=2), encoding='utf-8')
        print(f'PASS: {len(checks)} utility checks, plus initial meter invoice/demo checks. URL: {demo.BASE}/DichVu')
        print('Local credentials: ' + str(artifact / 'access.json'))
        passed = True
    finally:
        if not passed or '--serve' not in sys.argv:
            subprocess.run(['taskkill','/PID',str(process_id),'/T'], capture_output=True)


if __name__ == '__main__':
    main()
