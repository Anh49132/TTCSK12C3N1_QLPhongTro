CREATE TABLE IF NOT EXISTS yeu_cau_thue (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    ma_yeu_cau TEXT NOT NULL UNIQUE,
    khach_thue_id INTEGER NOT NULL REFERENCES khach_thue(id) ON DELETE RESTRICT,
    loai_yeu_cau TEXT NOT NULL,
    ngay_mong_muon TEXT NOT NULL,
    so_nguoi_du_kien INTEGER NOT NULL CHECK (so_nguoi_du_kien > 0),
    loi_nhan TEXT,
    lich_hen TEXT,
    trang_thai TEXT NOT NULL DEFAULT 'MOI'
        CHECK (trang_thai IN ('MOI','DA_HEN_LICH','DA_DUYET','TU_CHOI','DA_HUY')),
    ly_do_tu_choi TEXT,
    nguoi_xu_ly_id INTEGER REFERENCES tai_khoan(id) ON DELETE RESTRICT,
    ngay_xu_ly TEXT,
    ngay_tao TEXT NOT NULL,
    phien_ban INTEGER NOT NULL DEFAULT 0 CHECK (phien_ban >= 0)
);

CREATE INDEX IF NOT EXISTS ix_yeu_cau_thue_khach_ngay
    ON yeu_cau_thue(khach_thue_id, ngay_tao DESC, id DESC);
