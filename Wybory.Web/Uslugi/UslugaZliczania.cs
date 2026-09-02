using Microsoft.EntityFrameworkCore;
using Wybory.Web.Dane;
using Wybory.Web.Uslugi.Formuly;

namespace Wybory.Web.Uslugi;

// Zliczanie głosów i podział mandatów w okręgu wg wybranej formuły (pkt 5).
public class UslugaZliczania(BazaDanych db)
{
    // Próg jak w wyborach do Sejmu RP dla komitetów partyjnych.
    public const double ProgSejmowy = 5.0;

    // progProcentowy = 0 oznacza brak progu.
    public async Task<WynikiOkregu> ZliczWynikiAsync(int okregId, IFormulaPodzialuMandatow formula, double progProcentowy = 0)
    {
        var okreg = await db.Okregi.FindAsync(okregId)
            ?? throw new BladRegulyBiznesowej("Wskazany okręg nie istnieje.");

        var kandydaci = await db.Kandydaci
            .Where(k => k.OkregId == okregId)
            .Select(k => new
            {
                k.Id,
                k.KomitetId,
                NazwaKomitetu = k.Komitet!.Nazwa,
                ImieNazwisko = k.Wyborca!.Imie + " " + k.Wyborca!.Nazwisko,
                k.NumerNaLiscie,
                Glosy = k.Glosy.Count
            })
            .ToListAsync();

        var glosyNaKomitety = kandydaci
            .GroupBy(k => k.KomitetId)
            .ToDictionary(g => g.Key, g => g.Sum(k => k.Glosy));

        // Głosować można tylko na kandydata z własnego okręgu, więc suma głosów
        // kandydatów jest równa liczbie głosów oddanych w okręgu.
        var glosowLacznie = glosyNaKomitety.Values.Sum();

        // Komitet poniżej progu nie bierze udziału w podziale mandatów, ale jego
        // głosy nadal liczą się do mianownika przy wyliczaniu procentów.
        bool PonizejProgu(int komitetId) =>
            progProcentowy > 0
            && glosowLacznie > 0
            && glosyNaKomitety[komitetId] * 100.0 / glosowLacznie < progProcentowy;

        var doPodzialu = glosyNaKomitety
            .Where(para => !PonizejProgu(para.Key))
            .ToDictionary(para => para.Key, para => para.Value);

        // Gdyby próg wykluczył wszystkie komitety, żaden mandat nie zostałby obsadzony.
        // Kodeks wyborczy w takiej sytuacji obniża próg; tu po prostu go pomijamy.
        var progPominiety = doPodzialu.Count == 0 && glosyNaKomitety.Count > 0;
        if (progPominiety)
            doPodzialu = new Dictionary<int, int>(glosyNaKomitety);

        var mandatyNaKomitety = formula.PodzielMandaty(doPodzialu, okreg.LiczbaMandatow);

        var liczbaUprawnionych = await db.Wyborcy
            .CountAsync(w => w.OkregId == okregId && w.CzynnePrawoWyborcze);

        var wynikiKomitetow = kandydaci
            .GroupBy(k => new { k.KomitetId, k.NazwaKomitetu })
            .Select(grupa =>
            {
                // Lista otwarta: mandaty trafiają do kandydatów z największą liczbą
                // głosów osobistych, jak w polskich wyborach sejmowych.
                var mandaty = mandatyNaKomitety.GetValueOrDefault(grupa.Key.KomitetId);
                var glosyKomitetu = grupa.Sum(k => k.Glosy);
                var posortowani = grupa.OrderByDescending(k => k.Glosy).ThenBy(k => k.NumerNaLiscie).ToList();

                var wynikiKandydatow = posortowani
                    .Select((k, indeks) => new WynikKandydata(k.Id, k.ImieNazwisko, k.NazwaKomitetu, k.NumerNaLiscie, k.Glosy, indeks < mandaty))
                    .ToList();

                return new WynikKomitetu(
                    grupa.Key.KomitetId,
                    grupa.Key.NazwaKomitetu,
                    glosyKomitetu,
                    mandaty,
                    wynikiKandydatow,
                    glosowLacznie > 0 ? glosyKomitetu * 100.0 / glosowLacznie : 0,
                    PonizejProgu(grupa.Key.KomitetId) && !progPominiety);
            })
            .OrderByDescending(w => w.Glosy)
            .ToList();

        return new WynikiOkregu(
            okreg.Id, okreg.Nazwa, okreg.LiczbaMandatow, formula.Nazwa, wynikiKomitetow,
            liczbaUprawnionych, glosowLacznie, progProcentowy);
    }

    public async Task<WynikiKrajowe> PodsumujWszystkieOkregiAsync(IFormulaPodzialuMandatow formula, double progProcentowy = 0)
    {
        var okregiIds = await db.Okregi.OrderBy(o => o.Id).Select(o => o.Id).ToListAsync();

        var wynikiOkregow = new List<WynikiOkregu>();
        foreach (var okregId in okregiIds)
            wynikiOkregow.Add(await ZliczWynikiAsync(okregId, formula, progProcentowy));

        var glosowLacznie = wynikiOkregow.Sum(w => w.LiczbaGlosow);

        // Podział mandatów jest zawsze okręgowy, więc sumujemy wyniki okręgów.
        var komitety = wynikiOkregow
            .SelectMany(w => w.Komitety)
            .GroupBy(k => new { k.KomitetId, k.NazwaKomitetu })
            .Select(grupa =>
            {
                var glosy = grupa.Sum(k => k.Glosy);
                return new WynikKomitetuKrajowy(
                    grupa.Key.KomitetId,
                    grupa.Key.NazwaKomitetu,
                    glosy,
                    glosowLacznie > 0 ? glosy * 100.0 / glosowLacznie : 0,
                    grupa.Sum(k => k.Mandaty));
            })
            .OrderByDescending(k => k.Mandaty)
            .ThenByDescending(k => k.Glosy)
            .ToList();

        return new WynikiKrajowe(
            formula.Nazwa,
            progProcentowy,
            wynikiOkregow.Sum(w => w.LiczbaMandatow),
            wynikiOkregow.Sum(w => w.LiczbaUprawnionych),
            glosowLacznie,
            komitety,
            wynikiOkregow);
    }
}
