namespace Wybory.Web.Dane;

public class Kandydat
{
    public int Id { get; set; }

    // Unikalny FK — wyborca może kandydować co najwyżej raz (nie w wielu komitetach/okręgach naraz).
    public int WyborcaId { get; set; }
    public Wyborca? Wyborca { get; set; }

    // Wymagane, nie-nullowalne — nie może istnieć kandydat bez komitetu (pkt 3).
    public int KomitetId { get; set; }
    public Komitet? Komitet { get; set; }

    // Okręg, w którym kandydat startuje; musi być jednym z okręgów, w których
    // zarejestrowany jest KomitetId — walidowane w UslugaRejestracji.
    public int OkregId { get; set; }
    public Okreg? Okreg { get; set; }

    public int NumerNaLiscie { get; set; }

    public List<Glos> Glosy { get; set; } = [];
}
