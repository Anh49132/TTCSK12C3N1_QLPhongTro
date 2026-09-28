(() => {
    document.querySelectorAll("[data-vnd-input]").forEach(display => {
        const formatThousands = digits => digits.replace(/\B(?=(\d{3})+(?!\d))/g, ".");

        const updateValue = formatDisplay => {
            const raw = display.value.trim();
            const isPlainInteger = /^\d+$/.test(raw);
            const isGroupedInteger = /^\d{1,3}(?:\.\d{3})+$/.test(raw);
            const digits = isGroupedInteger ? raw.replaceAll(".", "") : isPlainInteger ? raw : "";
            if (formatDisplay && digits) display.value = formatThousands(digits);
        };

        display.addEventListener("input", () => updateValue(false));
        display.addEventListener("blur", () => updateValue(true));
        const initialValue = display.value.trim();
        if (/^\d+$/.test(initialValue)) display.value = formatThousands(initialValue);
    });
})();