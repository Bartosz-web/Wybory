namespace Wybory.Web.Uslugi;

// Pule danych do symulacji głosowania i przycisków "Losuj" — te same listy co w wwwroot/js/losowanie.js.
internal static class DaneLosowania
{
    public static readonly string[] ImionaMeskie =
    [
        "Adam", "Piotr", "Krzysztof", "Andrzej", "Tomasz", "Paweł", "Michał", "Marcin", "Grzegorz", "Jan",
        "Stanisław", "Tadeusz", "Jerzy", "Zbigniew", "Ryszard", "Wojciech", "Marek", "Dariusz", "Robert", "Mariusz",
        "Bartłomiej", "Łukasz", "Kamil", "Rafał", "Sebastian", "Artur", "Henryk", "Józef", "Kazimierz", "Waldemar"
    ];

    public static readonly string[] ImionaZenskie =
    [
        "Anna", "Maria", "Katarzyna", "Małgorzata", "Agnieszka", "Barbara", "Ewa", "Elżbieta", "Krystyna", "Zofia",
        "Joanna", "Magdalena", "Danuta", "Teresa", "Beata", "Monika", "Jolanta", "Halina", "Dorota", "Aleksandra",
        "Irena", "Grażyna", "Urszula", "Renata", "Iwona", "Justyna", "Karolina", "Marta", "Natalia", "Wiesława"
    ];

    public static readonly string[] Nazwiska =
    [
        "Nowak", "Kowalski", "Wiśniewski", "Wójcik", "Kowalczyk", "Kamiński", "Lewandowski", "Zieliński", "Szymański", "Woźniak",
        "Dąbrowski", "Kozłowski", "Jankowski", "Mazur", "Kwiatkowski", "Krawczyk", "Piotrowski", "Grabowski", "Nowakowski", "Pawłowski",
        "Michalski", "Nowicki", "Adamczyk", "Dudek", "Zając", "Wieczorek", "Jabłoński", "Król", "Majewski", "Olszewski",
        "Jaworski", "Wróbel", "Malinowski", "Pawlak", "Witkowski", "Walczak", "Stępień", "Górski", "Rutkowski", "Michalak",
        "Sikora", "Ostrowski", "Baran", "Duda", "Szewczyk", "Tomaszewski", "Pietrzak", "Marciniak", "Wróblewski", "Zalewski"
    ];

    public static readonly string[] NazwyKomitetow =
    [
        "Komitet Jedność Narodowa", "Komitet Nowa Droga", "Komitet Wspólna Polska",
        "Komitet Obywatelski Postęp", "Komitet Razem dla Regionu", "Komitet Przyszłość"
    ];

    // Ta sama reguła co w wwwroot/js/losowanie.js: żeńskie imię (kończy się na "a") -> nazwisko na "-ska".
    public static (string Imie, string Nazwisko) LosujImieNazwisko(Random losowy)
    {
        var zenskie = losowy.Next(2) == 0;
        var imie = zenskie
            ? ImionaZenskie[losowy.Next(ImionaZenskie.Length)]
            : ImionaMeskie[losowy.Next(ImionaMeskie.Length)];
        var nazwisko = Nazwiska[losowy.Next(Nazwiska.Length)];

        if (zenskie && nazwisko.EndsWith("ski"))
            nazwisko = nazwisko[..^3] + "ska";

        return (imie, nazwisko);
    }
}
