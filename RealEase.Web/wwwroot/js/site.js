// site.js - Funcionalidades base para RealEase (Bienes Raíces)


document.addEventListener("DOMContentLoaded", function () {
    const toggleBtn = document.querySelector(".menu-toggle");
    const nav = document.querySelector(".nav-links");

    if (toggleBtn && nav) {
        toggleBtn.addEventListener("click", () => {
            nav.classList.toggle("active");
        });
    }
});


document.querySelectorAll("[data-confirm]").forEach(btn => {
    btn.addEventListener("click", function (e) {
        const message = btn.getAttribute("data-confirm");
        if (!confirm(message)) {
            e.preventDefault();
        }
    });
});


document.querySelectorAll("input, textarea, select").forEach(input => {
    input.addEventListener("focus", function () {
        input.style.borderColor = "#66bb6a";
        input.style.boxShadow = "0 0 5px #66bb6a";
    });
    input.addEventListener("blur", function () {
        input.style.borderColor = "#ccc";
        input.style.boxShadow = "none";
    });
});


document.querySelectorAll(".btn").forEach(btn => {
    btn.addEventListener("mousedown", () => {
        btn.style.transform = "scale(0.97)";
    });
    btn.addEventListener("mouseup", () => {
        btn.style.transform = "scale(1)";
    });
});
