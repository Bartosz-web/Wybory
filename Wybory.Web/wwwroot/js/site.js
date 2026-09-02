// Ulepszenia wspólne dla wszystkich stron. Wyłącznie progresywne: bez
// JavaScriptu wszystko działa, tylko wymaga kliknięcia przycisku.
(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {
        // Formularz z data-auto-submit wysyła się po zmianie listy rozwijanej.
        // Przycisk zostaje w markupie dla osób bez JavaScriptu.
        document.querySelectorAll("form[data-auto-submit]").forEach(function (formularz) {
            formularz.querySelectorAll("select").forEach(function (select) {
                select.addEventListener("change", function () {
                    formularz.submit();
                });
            });
        });
    });
})();
