"""Chrome check of utility labels, date notice and native price validation; uses latest fake demo."""
import asyncio
import json
from pathlib import Path
import sys
ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / 'data/task1-build/browser-packages'))
from playwright.async_api import async_playwright

async def main():
    folder = Path((ROOT / 'data/service-demo/latest.txt').read_text(encoding='utf-8').strip())
    access = json.loads((folder / 'access.json').read_text(encoding='utf-8'))
    async with async_playwright() as p:
        browser = await p.chromium.launch(channel='chrome', headless=True)
        page = await browser.new_page()
        errors = []
        page.on('pageerror', lambda error: errors.append(str(error)))
        await page.goto(access['url'] + '/Account/Login')
        # Disable AJAX only for authentication; ordinary MVC login is also supported.
        await page.locator('#TaiKhoanDangNhap').fill(access['accounts']['CHU_NHA'][1])
        await page.locator('#MatKhau').fill(access['password'])
        await page.locator('#loginForm').evaluate('(form) => HTMLFormElement.prototype.submit.call(form)')
        await page.wait_for_url(access['url'] + '/')
        await page.goto(access['url'] + f'/DichVu/DienNuoc?toaNhaId={access["building"]}&maDichVu=NUOC')
        assert await page.locator('.alert-info').is_visible()
        assert await page.locator('#utility-method option').count() == 2
        await page.locator('#utility-method').select_option('THEO_CHI_SO')
        assert 'm³' in await page.locator('#utility-price-label').inner_text()
        await page.locator('#utility-method').select_option('THEO_NGUOI')
        assert 'mỗi tháng' in await page.locator('#utility-price-label').inner_text()
        for price in ['', '0']:
            await page.locator('#DonGia').fill(price)
            assert not await page.locator('#DonGia').evaluate('(input) => input.checkValidity()')
        await page.locator('#DonGia').fill('30000')
        assert await page.locator('#DonGia').evaluate('(input) => input.checkValidity()')
        assert not errors, errors
        await page.screenshot(path=str(folder / 'utilities-browser.png'), full_page=True)
        (folder / 'utilities-browser.json').write_text(json.dumps({'passed':True,'checks':['period visible','two methods','meter label','monthly person label','blank/zero blocked','positive price accepted','no JavaScript errors']}), encoding='utf-8')
        await browser.close()
        print('PASS: Chrome utility labels, period, blank/zero validation; no JavaScript errors.')

if __name__ == '__main__':
    asyncio.run(main())
