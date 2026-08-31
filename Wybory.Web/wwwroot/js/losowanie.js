// Losowanie danych testowych do formularza rejestracji wyborcy (przycisk "Losuj").
(function () {
    const IMIONA_MESKIE = [
        "Adam", "Piotr", "Krzysztof", "Andrzej", "Tomasz", "Paweł", "Michał", "Marcin", "Grzegorz", "Jan",
        "Stanisław", "Tadeusz", "Jerzy", "Zbigniew", "Ryszard", "Wojciech", "Marek", "Dariusz", "Robert", "Mariusz",
        "Bartłomiej", "Łukasz", "Kamil", "Rafał", "Sebastian", "Artur", "Henryk", "Józef", "Kazimierz", "Waldemar"
    ];

    const IMIONA_ZENSKIE = [
        "Anna", "Maria", "Katarzyna", "Małgorzata", "Agnieszka", "Barbara", "Ewa", "Elżbieta", "Krystyna", "Zofia",
        "Joanna", "Magdalena", "Danuta", "Teresa", "Beata", "Monika", "Jolanta", "Halina", "Dorota", "Aleksandra",
        "Irena", "Grażyna", "Urszula", "Renata", "Iwona", "Justyna", "Karolina", "Marta", "Natalia", "Wiesława"
    ];

    const NAZWISKA = [
        "Nowak", "Kowalski", "Wiśniewski", "Wójcik", "Kowalczyk", "Kamiński", "Lewandowski", "Zieliński", "Szymański", "Woźniak",
        "Dąbrowski", "Kozłowski", "Jankowski", "Mazur", "Kwiatkowski", "Krawczyk", "Piotrowski", "Grabowski", "Nowakowski", "Pawłowski",
        "Michalski", "Nowicki", "Adamczyk", "Dudek", "Zając", "Wieczorek", "Jabłoński", "Król", "Majewski", "Olszewski",
        "Jaworski", "Wróbel", "Malinowski", "Pawlak", "Witkowski", "Walczak", "Stępień", "Górski", "Rutkowski", "Michalak",
        "Sikora", "Ostrowski", "Baran", "Duda", "Szewczyk", "Tomaszewski", "Pietrzak", "Marciniak", "Wróblewski", "Zalewski"
    ];

    const IMIONA = IMIONA_MESKIE.concat(IMIONA_ZENSKIE);

    // Wylosowane numery PESEL w bieżącej sesji formularza — nie mogą się powtórzyć.
    const wylosowanePesele = new Set();

    function losowyElement(tablica) {
        return tablica[Math.floor(Math.random() * tablica.length)];
    }

    // Generuje 10 pierwszych cyfr PESEL-u z wiarygodną datą urodzenia (rok 1950-2009)
    // wg standardowego kodowania miesiąca (1900: 01-12, 2000: 21-32).
    function losujPierwsze10Cyfr() {
        const rok = 1950 + Math.floor(Math.random() * 60);
        const kodMiesiaca = (rok >= 2000 ? 20 : 0) + (1 + Math.floor(Math.random() * 12));
        const dzien = 1 + Math.floor(Math.random() * 28);
        const rr = String(rok % 100).padStart(2, "0");
        const mm = String(kodMiesiaca).padStart(2, "0");
        const dd = String(dzien).padStart(2, "0");
        const seria = String(Math.floor(Math.random() * 10000)).padStart(4, "0");
        return rr + mm + dd + seria;
    }

    function obliczCyfreKontrolna(pierwsze10Cyfr) {
        const wagi = [1, 3, 7, 9, 1, 3, 7, 9, 1, 3];
        let suma = 0;
        for (let i = 0; i < 10; i++)
            suma += wagi[i] * Number(pierwsze10Cyfr[i]);
        return (10 - (suma % 10)) % 10;
    }

    function losujNowyPesel() {
        let pesel;
        do {
            const pierwsze10 = losujPierwsze10Cyfr();
            pesel = pierwsze10 + obliczCyfreKontrolna(pierwsze10);
        } while (wylosowanePesele.has(pesel));

        wylosowanePesele.add(pesel);
        return pesel;
    }

    function podlaczPrzycisk(przyciskId, poleId, funkcjaLosujaca, poLosowaniu) {
        const przycisk = document.getElementById(przyciskId);
        const pole = document.getElementById(poleId);
        if (!przycisk || !pole) return;

        przycisk.addEventListener("click", function () {
            pole.value = funkcjaLosujaca();
            if (poLosowaniu) poLosowaniu();
        });
    }

    // Żeńskie imiona kończą się na "a" — nazwisko odmienia się wtedy jak przymiotnik
    // żeński (np. Kowalski -> Kowalska), żeby para imię+nazwisko była gramatycznie spójna.
    // Działa w obie strony, bo pola można losować ponownie w dowolnej kolejności.
    function dopasujOdmianeNazwiskaDoImienia() {
        const poleImie = document.getElementById("Formularz_Imie");
        const poleNazwisko = document.getElementById("Formularz_Nazwisko");
        if (!poleImie || !poleNazwisko) return;

        const imieZenskie = poleImie.value.trim().endsWith("a");
        const nazwisko = poleNazwisko.value.trim();

        if (imieZenskie && nazwisko.endsWith("ski"))
            poleNazwisko.value = nazwisko.slice(0, -3) + "ska";
        else if (!imieZenskie && nazwisko.endsWith("ska"))
            poleNazwisko.value = nazwisko.slice(0, -3) + "ski";
    }

    document.addEventListener("DOMContentLoaded", function () {
        podlaczPrzycisk("losuj-pesel", "Formularz_Pesel", losujNowyPesel);
        podlaczPrzycisk("losuj-imie", "Formularz_Imie", () => losowyElement(IMIONA), dopasujOdmianeNazwiskaDoImienia);
        podlaczPrzycisk("losuj-nazwisko", "Formularz_Nazwisko", () => losowyElement(NAZWISKA), dopasujOdmianeNazwiskaDoImienia);
    });
})();
