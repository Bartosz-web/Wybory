using Microsoft.EntityFrameworkCore;
using Wybory.Web.Dane;
using Wybory.Web.Uslugi.Formuly;

namespace Wybory.Web.Uslugi;

// Symulacja pełnych wyborów "jednym kliknięciem": czyści dane, generuje losowych
// wyborców/komitety/kandydatów (min. 10 kandydatów na okręg), oddaje losowe głosy
// i zwraca ogłoszone wyniki dla wszystkich okręgów.
public class UslugaSymulacji(BazaDanych db, UslugaZliczania uslugaZliczania)
{
    private const int LiczbaKomitetow = 3;
    private const int KandydatowNaKomitetWOkregu = 4; // 3 komitety x 4 = 12 kandydatów/okręg (>= 10)
    private const int DodatkowychWyborcowNaOkreg = 18;

    public async Task<List<WynikiOkregu>> SymulujAsync()
    {
        await WyczyscDaneAsync();

        var okregi = await db.Okregi.OrderBy(o => o.Id).ToListAsync();
        var losowy = new Random();
        var wylosowanePesele = new HashSet<string>();

        var komitety = LosujKomitety(losowy);
        foreach (var komitet in komitety)
            komitet.Okregi = okregi.Select(o => new KomitetOkreg { OkregId = o.Id }).ToList();
        db.Komitety.AddRange(komitety);
        await db.SaveChangesAsync();

        foreach (var okreg in okregi)
            await WypelnijOkregAsync(okreg, komitety, losowy, wylosowanePesele);

        await OddajLosoweGlosyAsync(losowy);

        var formula = new FormulaDHondta();
        var wyniki = new List<WynikiOkregu>();
        foreach (var okreg in okregi)
            wyniki.Add(await uslugaZliczania.ZliczWynikiAsync(okreg.Id, formula));
        return wyniki;
    }

    private async Task WyczyscDaneAsync()
    {
        db.Glosy.RemoveRange(db.Glosy);
        db.Kandydaci.RemoveRange(db.Kandydaci);
        db.KomitetyOkregow.RemoveRange(db.KomitetyOkregow);
        db.Komitety.RemoveRange(db.Komitety);
        db.Wyborcy.RemoveRange(db.Wyborcy);
        await db.SaveChangesAsync();
    }

    private static List<Komitet> LosujKomitety(Random losowy) =>
        DaneLosowania.NazwyKomitetow
            .OrderBy(_ => losowy.Next())
            .Take(LiczbaKomitetow)
            .Select(nazwa => new Komitet { Nazwa = nazwa })
            .ToList();

    private async Task WypelnijOkregAsync(Okreg okreg, List<Komitet> komitety, Random losowy, HashSet<string> wylosowanePesele)
    {
        foreach (var komitet in komitety)
        {
            var kandydaciWyborcy = Enumerable.Range(0, KandydatowNaKomitetWOkregu)
                .Select(_ => NowyWyborca(okreg.Id, czynne: true, bierne: true, losowy, wylosowanePesele))
                .ToList();

            db.Wyborcy.AddRange(kandydaciWyborcy);
            await db.SaveChangesAsync(); // potrzebne Id wyborców przed utworzeniem Kandydat

            for (var n = 0; n < kandydaciWyborcy.Count; n++)
            {
                db.Kandydaci.Add(new Kandydat
                {
                    WyborcaId = kandydaciWyborcy[n].Id,
                    KomitetId = komitet.Id,
                    OkregId = okreg.Id,
                    NumerNaLiscie = n + 1
                });
            }
        }

        var dodatkowiWyborcy = Enumerable.Range(0, DodatkowychWyborcowNaOkreg)
            .Select(_ => NowyWyborca(okreg.Id, czynne: true, bierne: false, losowy, wylosowanePesele));
        db.Wyborcy.AddRange(dodatkowiWyborcy);

        await db.SaveChangesAsync();
    }

    private static Wyborca NowyWyborca(int okregId, bool czynne, bool bierne, Random losowy, HashSet<string> wylosowanePesele)
    {
        var (imie, nazwisko) = DaneLosowania.LosujImieNazwisko(losowy);
        return new Wyborca
        {
            Pesel = GeneratorPesel.Losuj(losowy, wylosowanePesele),
            Imie = imie,
            Nazwisko = nazwisko,
            OkregId = okregId,
            CzynnePrawoWyborcze = czynne,
            BierneProwoWyborcze = bierne
        };
    }

    private async Task OddajLosoweGlosyAsync(Random losowy)
    {
        // Grupowanie w pamięci (po materializacji) — jak w Kandydaci/Lista, żeby uniknąć
        // problemów z tłumaczeniem GroupBy + zagnieżdżonej listy na SQL.
        var kandydaciPoOkregu = (await db.Kandydaci.ToListAsync())
            .GroupBy(k => k.OkregId)
            .ToDictionary(g => g.Key, g => g.Select(k => k.Id).ToList());

        var uprawnieni = await db.Wyborcy.Where(w => w.CzynnePrawoWyborcze).ToListAsync();

        foreach (var wyborca in uprawnieni)
        {
            var kandydaci = kandydaciPoOkregu[wyborca.OkregId];
            var wybranyKandydatId = kandydaci[losowy.Next(kandydaci.Count)];
            db.Glosy.Add(new Glos { WyborcaId = wyborca.Id, KandydatId = wybranyKandydatId, DataOddania = DateTime.UtcNow });
        }

        await db.SaveChangesAsync();
    }
}
