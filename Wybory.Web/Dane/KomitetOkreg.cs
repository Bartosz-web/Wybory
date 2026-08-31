namespace Wybory.Web.Dane;

// Encja łącząca N:N — w jakich okręgach zarejestrowany jest dany komitet (pkt 2).
// Klucz złożony (KomitetId, OkregId) ustawiony w BazaDanych.OnModelCreating.
public class KomitetOkreg
{
    public int KomitetId { get; set; }
    public Komitet? Komitet { get; set; }

    public int OkregId { get; set; }
    public Okreg? Okreg { get; set; }
}
