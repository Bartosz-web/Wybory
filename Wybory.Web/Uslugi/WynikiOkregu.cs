namespace Wybory.Web.Uslugi;

public record WynikKandydata(int KandydatId, string ImieNazwisko, string NazwaKomitetu, int NumerNaLiscie, int Glosy, bool CzyZdobylMandat);

public record WynikKomitetu(
    int KomitetId,
    string NazwaKomitetu,
    int Glosy,
    int Mandaty,
    IReadOnlyList<WynikKandydata> Kandydaci,
    double ProcentGlosow = 0,
    bool PonizejProgu = false);

public record WynikiOkregu(
    int OkregId,
    string NazwaOkregu,
    int LiczbaMandatow,
    string NazwaFormuly,
    IReadOnlyList<WynikKomitetu> Komitety,
    int LiczbaUprawnionych = 0,
    int LiczbaGlosow = 0,
    double ProgProcentowy = 0)
{
    // Frekwencja liczona wobec osób z czynnym prawem wyborczym w tym okręgu.
    public double Frekwencja => LiczbaUprawnionych > 0 ? LiczbaGlosow * 100.0 / LiczbaUprawnionych : 0;
}

public record WynikKomitetuKrajowy(int KomitetId, string NazwaKomitetu, int Glosy, double ProcentGlosow, int Mandaty);

// Wyniki wszystkich okręgów łącznie.
public record WynikiKrajowe(
    string NazwaFormuly,
    double ProgProcentowy,
    int LiczbaMandatow,
    int LiczbaUprawnionych,
    int LiczbaGlosow,
    IReadOnlyList<WynikKomitetuKrajowy> Komitety,
    IReadOnlyList<WynikiOkregu> Okregi)
{
    public double Frekwencja => LiczbaUprawnionych > 0 ? LiczbaGlosow * 100.0 / LiczbaUprawnionych : 0;
}
