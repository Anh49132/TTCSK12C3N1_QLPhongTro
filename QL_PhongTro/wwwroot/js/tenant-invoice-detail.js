(() => {
    const page = document.querySelector(".tenant-invoice-detail");
    if (!page) return;

    const loading = document.getElementById("tenant-invoice-loading");
    const error = document.getElementById("tenant-invoice-error");
    const content = document.getElementById("tenant-invoice-content");
    const body = document.querySelector("#tenant-invoice-lines tbody");
    const period = document.getElementById("tenant-invoice-period");
    const room = document.getElementById("tenant-invoice-room");

    const cell = (value, className) => {
        const element = document.createElement("td");
        element.textContent = value;
        if (className) element.className = className;
        return element;
    };

    const showError = (message) => {
        loading.hidden = true;
        content.hidden = true;
        error.textContent = message;
        error.hidden = false;
    };

    const load = async () => {
        loading.hidden = false;
        content.hidden = true;
        error.hidden = true;
        try {
            const response = await fetch(page.dataset.invoiceUrl, {
                headers: { Accept: "application/json" },
                cache: "no-store",
                credentials: "same-origin"
            });
            if (response.status === 404) {
                showError("Không tìm thấy hóa đơn.");
                return;
            }
            if (!response.ok) throw new Error("Invoice request failed.");

            const invoice = await response.json();
            period.textContent = `${invoice.thang}/${invoice.nam}`;
            room.textContent = `Phòng ${invoice.maPhong}`;
            body.replaceChildren();
            for (const line of invoice.chiTiet) {
                const row = document.createElement("tr");
                const unit = line.donViTinh ? ` ${line.donViTinh}` : "";
                row.append(
                    cell(line.tenKhoan),
                    cell(line.chiSoDau === null ? "—" : `${line.chiSoDau}${unit}`),
                    cell(line.chiSoCuoi === null ? "—" : `${line.chiSoCuoi}${unit}`),
                    cell(`${line.soLuongTieuThu}${unit}`),
                    cell(`${line.donGia} đ${line.donViTinh ? `/${line.donViTinh}` : ""}`, "text-end"),
                    cell(`${line.thanhTien} đ`, "text-end fw-semibold")
                );
                body.append(row);
            }
            loading.hidden = true;
            content.hidden = false;
        } catch {
            showError("Không thể tải hóa đơn. Vui lòng thử lại.");
        }
    };

    load();
})();
