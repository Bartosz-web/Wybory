namespace Wybory.Web.Dane;

public class Wyborca
{
    public int Id { get; set; }

    // Unikalny w bazie (patrz BazaDanych.OnModelCreating) — jedna osoba, jeden rekord.
    public string Pesel { get; set; } = "";
    public string Imie { get; set; } = "";
    public string Nazwisko { get; set; } = "";

    // Wyborca należy do dokładnie jednego okręgu (pkt 1) — przypisywany raz, przy rejestracji.
    public int OkregId { get; set; }
    public Okreg? Okreg { get; set; }

    // Aktywne (wybieranie) i bierne (kandydowanie) prawo wyborcze — niezależne od siebie.
    public bool CzynnePrawoWyborcze { get; set; }
    public bool BierneProwoWyborcze { get; set; }

    public Kandydat? Kandydatura { get; set; }
    public Glos? Glos { get; set; }
}
