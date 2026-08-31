using Microsoft.EntityFrameworkCore;
using Wybory.Web.Dane;
using Wybory.Web.Uslugi.Formuly;

namespace Wybory.Web.Uslugi;

// Zliczanie głosów i podział mandatów w okręgu wg wybranej formuły (pkt 5).
public class UslugaZliczania(BazaDanych db)
{
    public async Task<WynikiOkregu> ZliczWynikiAsync(int okregId, IFormulaPodzialuMandatow formula)
    {
        var okreg = await db.Okregi.FindAsync(okregId)
            ?? throw new BladRegulyBiznesowej("Wskazany okręg nie istnieje.");

        var kandydaci = await db.Kandydaci
            .Include(k => k.Wyborca)
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

        var mandatyNaKomitety = formula.PodzielMandaty(glosyNaKomitety, okreg.LiczbaMandatow);

        var wynikiKomitetow = kandydaci
            .GroupBy(k => new { k.KomitetId, k.NazwaKomitetu })
            .Select(grupa =>
            {
                // Lista otwarta: mandaty komitetu trafiają do kandydatów z największą
                // liczbą głosów osobistych (jak w polskich wyborach sejmowych).
                var mandaty = mandatyNaKomitety.GetValueOrDefault(grupa.Key.KomitetId);
                var posortowani = grupa.OrderByDescending(k => k.Glosy).ThenBy(k => k.NumerNaLiscie).ToList();

                var wynikiKandydatow = posortowani
                    .Select((k, indeks) => new WynikKandydata(k.Id, k.ImieNazwisko, k.NazwaKomitetu, k.NumerNaLiscie, k.Glosy, indeks < mandaty))
                    .ToList();

                return new WynikKomitetu(
                    grupa.Key.KomitetId,
                    grupa.Key.NazwaKomitetu,
                    grupa.Sum(k => k.Glosy),
                    mandaty,
                    wynikiKandydatow);
            })
            .OrderByDescending(w => w.Glosy)
            .ToList();

        return new WynikiOkregu(okreg.Id, okreg.Nazwa, okreg.LiczbaMandatow, formula.Nazwa, wynikiKomitetow);
    }
}
