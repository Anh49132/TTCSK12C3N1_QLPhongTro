"""Task 1/2: fresh synthetic demo + HTTP checks; upgrade only a disposable copy.

Build first: dotnet build QL_PhongTro -o data/task1-build/runtime
Run: python verification/timtin_http.py [--serve]
No third-party Python dependencies. Never writes to the source database.
"""
import argparse
import hashlib
import html
import json
import os
from pathlib import Path
import re
import socket
import sqlite3
import subprocess
import time
from urllib.parse import urlencode
from urllib.request import urlopen
from datetime import datetime, timezone
from uuid import uuid4

ROOT = Path(__file__).resolve().parents[1]
APP = ROOT / 'QL_PhongTro'
DLL = ROOT / 'data/task1-build/runtime/QL_PhongTro.dll'


def snapshot(connection):
    tables = [r[0] for r in connection.execute("SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' AND name!='app_schema_version'")]
    return {t: connection.execute('SELECT * FROM "' + t + '" ORDER BY rowid').fetchall() for t in tables}


def main():
    global DLL
    parser = argparse.ArgumentParser()
    parser.add_argument('--runtime', type=Path, default=DLL, help='Path to the built QL_PhongTro.dll')
    parser.add_argument('--serve', action='store_true', help='Keep the tested synthetic demo running')
    args = parser.parse_args()
    DLL = args.runtime.resolve()
    folder = ROOT / 'data/timtin-demo' / uuid4().hex
    folder.mkdir(parents=True)
    database = folder / 'demo.sqlite'
    env = os.environ.copy()
    env.update(DatabasePath=str(database), ASPNETCORE_ENVIRONMENT='Development')
    # No SMTP or fixed login credential is needed for this public search demo.
    env['PasswordReset__PickupDirectory'] = str(folder / 'mail')
    checks = []

    def run(flag, success=True):
        result = subprocess.run(['dotnet', str(DLL), flag], cwd=APP, env=env, capture_output=True, text=True)
        with (folder / 'commands.log').open('a', encoding='utf-8') as log:
            log.write(flag + '\n' + result.stdout + result.stderr + '\n')
        assert (result.returncode == 0) == success, result.stdout + result.stderr

    run('--initialize-database')
    with sqlite3.connect(database) as c:
        assert c.execute('SELECT count(*) FROM tai_khoan').fetchone()[0] == 0
        assert c.execute('SELECT max(version) FROM app_schema_version').fetchone()[0] == 6
    before = database.read_bytes()
    run('--initialize-database', success=False)
    assert before == database.read_bytes(), 'Initializer changed an existing file'
    checks.append('Fresh initialization v6, empty accounts, refuses overwrite')

    # SQLite consistent backup from a read-only source; all upgrade writes go to copy.
    source = Path(env.get('TIMTIN_SOURCE_DATABASE', str(APP / 'Data/local-dev.sqlite'))).resolve()
    source_hash = hashlib.sha256(source.read_bytes()).hexdigest()
    upgrade = folder / 'upgrade-copy.sqlite'
    with sqlite3.connect(source.as_uri() + '?mode=ro', uri=True) as c:
        original = snapshot(c)
        with sqlite3.connect(upgrade) as target:
            c.backup(target)
    env['DatabasePath'] = str(upgrade)
    run('--update-database')
    run('--check-database')
    run('--update-database')
    with sqlite3.connect(upgrade) as c:
        after = snapshot(c)
        assert all(after[t] == rows for t, rows in original.items()), 'Existing rows changed'
        assert c.execute('PRAGMA integrity_check').fetchone()[0] == 'ok'
        assert not c.execute('PRAGMA foreign_key_check').fetchall()
        assert c.execute('SELECT max(version) FROM app_schema_version').fetchone()[0] == 6
    assert list(folder.glob('upgrade-copy.sqlite.before-update-*.bak')), 'Missing upgrade backup'
    assert hashlib.sha256(source.read_bytes()).hexdigest() == source_hash
    checks.append('Copy upgrade v6, backup, rerun, integrity/FK, all existing rows preserved, source hash unchanged')

    env['DatabasePath'] = str(database)
    with sqlite3.connect(database) as c:
        c.execute('PRAGMA foreign_keys=ON')
        now = datetime.now(timezone.utc).strftime('%Y-%m-%d %H:%M:%S')
        c.execute("INSERT INTO tai_khoan(ho_ten,email,so_dien_thoai,mat_khau,vai_tro,dang_hoat_dong,is_staff,is_superuser,ngay_tao,ngay_cap_nhat,email_confirmed,is_deleted) VALUES(?,?,?,?,?,0,0,0,?,?,1,0)",
                  ('Chủ nhà mẫu', 'owner@example.invalid', '0900000000', uuid4().hex, 'CHU_NHA', now, now))
        owner = c.execute('SELECT last_insert_rowid()').fetchone()[0]
        districts = ['Quận 1', 'Quận 2']
        buildings = []
        for district in districts:
            c.execute('INSERT INTO toa_nha(chu_nha_id,ten_toa_nha,dia_chi,quan_huyen) VALUES(?,?,?,?)', (owner, 'Tòa mẫu ' + district, 'Địa chỉ mẫu', district))
            buildings.append(c.execute('SELECT last_insert_rowid()').fetchone()[0])
        fixtures = [
            ('A', 0, 1000000, 10.25, 2, 'DANG_HIEN_THI', '2099-01-01 00:00:00'),
            ('B', 0, 2000000, 20.5, 3, 'DANG_HIEN_THI', '2099-01-01 00:00:00'),
            ('C', 1, 2000000, 20.5, 3, 'DANG_HIEN_THI', '2099-01-01 00:00:00'),
            ('D', 1, 3000000, 30.75, 4, 'DANG_HIEN_THI', '2099-01-01 00:00:00'),
            ('HIDDEN', 0, 2000000, 20.5, 3, 'TAM_AN', '2099-01-01 00:00:00'),
            ('EXPIRED', 0, 2000000, 20.5, 3, 'DANG_HIEN_THI', '2000-01-01 00:00:00'),
            ('DRAFT', 0, 2000000, 20.5, 3, 'NHAP', '2099-01-01 00:00:00'),
            ('RENTED', 0, 2000000, 20.5, 3, 'DA_CHO_THUE', '2099-01-01 00:00:00'),
            ('NULL_EXPIRY', 0, 2000000, 20.5, 3, 'DANG_HIEN_THI', None),
        ]
        for name, area, price, size, people, state, expiry in fixtures:
            c.execute("INSERT INTO phong_tro(toa_nha_id,ma_phong,tang,dien_tich,gia_thue,so_nguoi_toi_da,trang_thai,ngay_tao) VALUES(?,?,1,?,?,?,'TRONG',?)", (buildings[area], name, size, price, people, now))
            room = c.execute('SELECT last_insert_rowid()').fetchone()[0]
            c.execute('INSERT INTO tin_dang(phong_id,nguoi_dang_id,tieu_de,trang_thai,ngay_het_han,ngay_tao) VALUES(?,?,?,?,?,?)', (room, owner, 'TIN-' + name, state, expiry, now))

    # Bind a free port for the isolated demo.
    with socket.socket() as sock:
        sock.bind(('127.0.0.1', 0))
        port = sock.getsockname()[1]
    base = f'http://localhost:{port}'
    log = (folder / 'server.log').open('w', encoding='utf-8')
    server = subprocess.Popen(['dotnet', str(DLL), '--urls', base], cwd=APP, env=env, stdout=log, stderr=log,
                              creationflags=subprocess.CREATE_NO_WINDOW if os.name == 'nt' else 0)
    try:
        for _ in range(120):
            if server.poll() is not None:
                raise RuntimeError((folder / 'server.log').read_text(encoding='utf-8'))
            try:
                with urlopen(base + '/TimTin', timeout=2):
                    break
            except OSError:
                time.sleep(0.5)
        else:
            raise RuntimeError('Server did not start')

        def request(filters):
            with urlopen(base + '/TimTin?' + urlencode(filters), timeout=15) as response:
                assert response.status == 200
                return html.unescape(response.read().decode())

        cases = [
            ({}, {'A', 'B', 'C', 'D'}),
            ({'QuanHuyen': 'Quận 1'}, {'A', 'B'}),
            ({'QuanHuyen': 'Quận 2'}, {'C', 'D'}),
            ({'GiaToiThieu': 1000000, 'GiaToiDa': 2000000}, {'A', 'B', 'C'}),
            ({'GiaToiThieu': 2000000}, {'B', 'C', 'D'}),
            ({'GiaToiDa': 1000000}, {'A'}),
            ({'DienTichToiThieu': '10.25', 'DienTichToiDa': '20.5'}, {'A', 'B', 'C'}),
            ({'DienTichToiThieu': '20.5'}, {'B', 'C', 'D'}),
            ({'DienTichToiDa': '10.25'}, {'A'}),
            ({'SoNguoiToiDa': 3}, {'B', 'C'}),
            ({'SoNguoiToiDa': 2}, {'A'}),
            ({'SoNguoiToiDa': 4}, {'D'}),
            ({'QuanHuyen': 'Quận 1', 'GiaToiThieu': 2000000}, {'B'}),
            ({'GiaToiDa': 2000000, 'DienTichToiThieu': '20.5'}, {'B', 'C'}),
            ({'QuanHuyen': 'Quận 2', 'GiaToiThieu': 2000000, 'GiaToiDa': 2000000,
              'DienTichToiThieu': '20.5', 'DienTichToiDa': '20.5', 'SoNguoiToiDa': 3}, {'C'}),
            ({'QuanHuyen': '', 'GiaToiThieu': '', 'GiaToiDa': '', 'DienTichToiThieu': '', 'DienTichToiDa': '', 'SoNguoiToiDa': ''}, {'A', 'B', 'C', 'D'}),
            ({'GiaToiDa': 0}, set()),
        ]
        for filters, expected in cases:
            page = request(filters)
            actual = set(re.findall(r'<h2[^>]*>TIN-([^<]+)</h2>', page))
            assert actual == expected, (filters, actual, expected)
            for field, value in filters.items():
                if str(value) == '':
                    continue
                if field in ('QuanHuyen', 'SoNguoiToiDa'):
                    assert re.search(r'<option(?=[^>]*value="' + re.escape(str(value)) + r'")(?=[^>]*selected)[^>]*>', page), (field, 'selection lost')
                else:
                    assert re.search(r'<input(?=[^>]*name="' + field + r'")(?=[^>]*value="' + re.escape(str(value)) + r'")[^>]*>', page), (field, 'value lost')
        errors = [
            ({'GiaToiThieu': 3000000, 'GiaToiDa': 1000000}, 'Giá thuê tối thiểu không được lớn hơn'),
            ({'DienTichToiThieu': 30, 'DienTichToiDa': 10}, 'Diện tích tối thiểu không được lớn hơn'),
            ({'GiaToiThieu': -1}, 'Giá thuê phải là số không âm'),
            ({'DienTichToiDa': -1}, 'Diện tích phải là số không âm'),
            ({'SoNguoiToiDa': 0}, 'Số người ở tối đa phải là số nguyên dương'),
            ({'QuanHuyen': 'Không tồn tại'}, 'Quận/huyện không thuộc danh sách'),
            ({'SoNguoiToiDa': 99}, 'Số người ở tối đa không thuộc danh sách'),
            ({'GiaToiDa': 'abc'}, 'field-validation-error'),
        ]
        for filters, message in errors:
            page = request(filters)
            assert message in page and not re.search(r'<h2[^>]*>TIN-', page), (filters, 'validation failed')
        checks.append(f'HTTP: {len(cases)} valid/combined/boundary/blank cases and {len(errors)} invalid cases; selections retained; hidden/expired/draft/rented/NULL expiry excluded')
        result = {'url': base + '/TimTin', 'database': str(database), 'pid': server.pid, 'checks': checks}
        (folder / 'result.json').write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding='utf-8')
        (folder.parent / 'latest.txt').write_text(str(folder), encoding='utf-8')
        print(json.dumps(result, ensure_ascii=False, indent=2))
        if args.serve:
            print('Demo server remains running. Stop only the PID shown above when finished.')
            server = None
    finally:
        if server is not None:
            server.terminate()
            server.wait(timeout=15)
        log.close()


if __name__ == '__main__':
    main()
