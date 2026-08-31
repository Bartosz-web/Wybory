namespace Wybory.Web.Dane;

public class Komitet
{
    public int Id { get; set; }
    public string Nazwa { get; set; } = "";

    // Komitet może być zarejestrowany w dowolnej liczbie okręgów (pkt 2).
    public List<KomitetOkreg> Okregi { get; set; } = [];
    public List<Kandydat> Kandydaci { get; set; } = [];
}
