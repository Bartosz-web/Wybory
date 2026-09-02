// Ulepszenia formularzy działające wyłącznie po stronie przeglądarki.
// Bez JavaScriptu formularze nadal działają, tylko mniej wygodnie.
// Walidacja regul biznesowych i tak jest po stronie serwera.
(function () {
    "use strict";

    // <select data-zalezny-od="ID_INNEGO_SELECTA"> z opcjami opisanymi
    // atrybutem data-okregi="1,3" pokazuje tylko opcje pasujące do wyboru.
    function podlaczListeZalezna(select) {
        const nadrzedny = document.getElementById(select.dataset.zaleznyOd);
        if (!nadrzedny) return;

        // Opcje trzymamy w pamięci i przebudowujemy <select> od zera: atrybut
        // hidden na <option> nie jest jednakowo wspierany przez przeglądarki.
        const wszystkie = Array.from(select.options).map(function (opcja) {
            return {
                value: opcja.value,
                tekst: opcja.textContent,
                okregi: (opcja.dataset.okregi || "")
                    .split(",")
                    .filter(Boolean)
                    .map(Number)
            };
        });

        function odswiez() {
            const wybranyOkreg = Number(nadrzedny.value);
            const poprzedniWybor = select.value;

            // Pozycja "-- wybierz --" zostaje zawsze, brak data-okregi oznacza
            // brak ograniczenia.
            const pasujace = wszystkie.filter(function (o) {
                return o.value === "0" || !wybranyOkreg || o.okregi.length === 0
                    || o.okregi.indexOf(wybranyOkreg) !== -1;
            });

            select.innerHTML = "";
            pasujace.forEach(function (o) {
                const opcja = document.createElement("option");
                opcja.value = o.value;
                opcja.textContent = o.tekst;
                select.appendChild(opcja);
            });

            // Zachowaj poprzedni wybór, jeśli nadal jest na liście.
            select.value = pasujace.some(function (o) { return o.value === poprzedniWybor; })
                ? poprzedniWybor
                : "0";

            const komunikat = select.parentElement.querySelector(".form-text");
            if (komunikat && wybranyOkreg && pasujace.length <= 1) {
                komunikat.textContent = "Żaden komitet nie jest zarejestrowany w tym okręgu.";
                komunikat.classList.add("text-danger");
            } else if (komunikat) {
                komunikat.classList.remove("text-danger");
            }
        }

        nadrzedny.addEventListener("change", odswiez);
        odswiez();
    }

    // Pole tekstowe zawężające długą listę rozwijaną.
    function podlaczFiltrListy(pole) {
        const select = document.getElementById(pole.dataset.filtruje);
        if (!select) return;

        const wszystkie = Array.from(select.options).map(function (opcja) {
            return { value: opcja.value, tekst: opcja.textContent };
        });

        pole.hidden = false;

        pole.addEventListener("input", function () {
            const fraza = pole.value.trim().toLowerCase();
            const poprzedniWybor = select.value;

            const pasujace = wszystkie.filter(function (o) {
                return o.value === "0" || o.tekst.toLowerCase().indexOf(fraza) !== -1;
            });

            select.innerHTML = "";
            pasujace.forEach(function (o) {
                const opcja = document.createElement("option");
                opcja.value = o.value;
                opcja.textContent = o.tekst;
                select.appendChild(opcja);
            });

            select.value = pasujace.some(function (o) { return o.value === poprzedniWybor; })
                ? poprzedniWybor
                : "0";
        });
    }

    document.addEventListener("DOMContentLoaded", function () {
        document.querySelectorAll("select[data-zalezny-od]").forEach(podlaczListeZalezna);
        document.querySelectorAll("[data-filtruje]").forEach(podlaczFiltrListy);
    });
})();
