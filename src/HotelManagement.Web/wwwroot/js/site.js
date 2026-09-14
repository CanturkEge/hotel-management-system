"use strict";

const navToggle = document.querySelector(".nav-toggle");
const siteNav = document.querySelector(".site-nav");
if (navToggle && siteNav) {
    navToggle.addEventListener("click", () => {
        const open = siteNav.classList.toggle("is-open");
        navToggle.setAttribute("aria-expanded", String(open));
    });
}

document.querySelectorAll("form[data-confirm]").forEach(form => {
    form.addEventListener("submit", event => {
        if (!window.confirm(form.dataset.confirm)) event.preventDefault();
    });
});

document.querySelectorAll(".notice-close").forEach(button => {
    button.addEventListener("click", () => button.closest(".notice")?.remove());
});

const bulkForm = document.querySelector("[data-bulk-room-form]");
const roomPreview = document.querySelector("[data-room-preview]");
if (bulkForm && roomPreview) {
    const startInput = bulkForm.querySelector('[name="Bulk.StartNumber"]');
    const countInput = bulkForm.querySelector('[name="Bulk.Count"]');
    const floorInput = bulkForm.querySelector('[name="Bulk.Floor"]');
    const typeInput = bulkForm.querySelector('[name="Bulk.RoomTypeId"]');

    const updatePreview = () => {
        const start = Number(startInput?.value);
        const count = Number(countInput?.value);
        const floor = Number(floorInput?.value);
        const type = typeInput?.selectedOptions[0]?.textContent?.split(" · ")[0] ?? "";
        if (!Number.isInteger(start) || !Number.isInteger(count) || count < 1 || count > 100) {
            roomPreview.textContent = "Geçerli bir başlangıç numarası ve 1–100 arasında adet girin.";
            return;
        }
        const end = start + count - 1;
        const title = document.createElement("strong");
        title.textContent = `${start}–${end} arası ${count} oda`;
        const detail = document.createElement("span");
        detail.textContent = `${type && type !== "Oda tipi seçin" ? type + " · " : ""}${floor}. kat`;
        roomPreview.replaceChildren(title, detail);
    };

    [startInput, countInput, floorInput, typeInput].forEach(input => input?.addEventListener("input", updatePreview));
    updatePreview();
}

const roomSearch = document.querySelector("[data-room-search]");
const roomStatus = document.querySelector("[data-room-status]");
const roomRows = [...document.querySelectorAll("[data-room-row]")];
const roomCount = document.querySelector("[data-room-result-count]");
const roomEmpty = document.querySelector("[data-room-empty]");
if (roomRows.length && roomSearch && roomStatus) {
    const filterRooms = () => {
        const needle = roomSearch.value.trim().toLocaleLowerCase("tr-TR");
        const status = roomStatus.value;
        let visible = 0;
        roomRows.forEach(row => {
            const matchesText = row.textContent.toLocaleLowerCase("tr-TR").includes(needle);
            const matchesStatus = status === "all"
                || (status === "active" && row.dataset.active === "true")
                || (status === "inactive" && row.dataset.active === "false")
                || row.dataset.status === status;
            row.hidden = !(matchesText && matchesStatus);
            if (!row.hidden) visible++;
        });
        if (roomCount) roomCount.textContent = `${visible} oda`;
        if (roomEmpty) roomEmpty.hidden = visible !== 0;
    };
    roomSearch.addEventListener("input", filterRooms);
    roomStatus.addEventListener("change", filterRooms);
}
document.querySelectorAll("[data-filter]").forEach(input => {
    input.addEventListener("input", () => {
        const table = document.getElementById(input.dataset.filter);
        if (!table) return;
        const needle = input.value.toLocaleLowerCase("tr-TR");
        table.querySelectorAll("tbody tr").forEach(row => {
            row.hidden = !row.textContent.toLocaleLowerCase("tr-TR").includes(needle);
        });
    });
});
