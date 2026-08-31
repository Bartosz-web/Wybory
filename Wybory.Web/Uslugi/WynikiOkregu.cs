namespace Wybory.Web.Uslugi;

public record WynikKandydata(int KandydatId, string ImieNazwisko, string NazwaKomitetu, int NumerNaLiscie, int Glosy, bool CzyZdobylMandat);

public record WynikKomitetu(int KomitetId, string NazwaKomitetu, int Glosy, int Mandaty, IReadOnlyList<WynikKandydata> Kandydaci);

public record WynikiOkregu(int OkregId, string NazwaOkregu, int LiczbaMandatow, string NazwaFormuly, IReadOnlyList<WynikKomitetu> Komitety);
