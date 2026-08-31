namespace Wybory.Web.Dane;

public class Glos
{
    public int Id { get; set; }

    // Unikalny FK — jeden głos na wyborcę (pkt 4), wymuszone też indeksem unikalnym w bazie.
    public int WyborcaId { get; set; }
    public Wyborca? Wyborca { get; set; }

    public int KandydatId { get; set; }
    public Kandydat? Kandydat { get; set; }

    public DateTime DataOddania { get; set; }
}
