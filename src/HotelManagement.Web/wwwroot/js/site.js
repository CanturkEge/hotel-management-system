"use strict";
document.querySelectorAll("form[data-confirm]").forEach(form => {
    form.addEventListener("submit", event => {
        if (!window.confirm(form.dataset.confirm)) event.preventDefault();
    });
});
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
