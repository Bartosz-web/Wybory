namespace Wybory.Web.Dane;

public class Okreg
{
    public int Id { get; set; }
    public string Nazwa { get; set; } = "";

    // Liczba mandatów do rozdzielenia między komitety w tym okręgu (pkt 5).
    public int LiczbaMandatow { get; set; }

    public List<Wyborca> Wyborcy { get; set; } = [];
    public List<KomitetOkreg> KomitetyOkregu { get; set; } = [];
    public List<Kandydat> Kandydaci { get; set; } = [];
}
