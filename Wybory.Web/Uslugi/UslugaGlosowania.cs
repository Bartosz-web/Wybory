using Microsoft.EntityFrameworkCore;
using Wybory.Web.Dane;

namespace Wybory.Web.Uslugi;

// Oddawanie głosów (pkt 4 zadania).
public class UslugaGlosowania(BazaDanych db)
{
    public async Task<IReadOnlyList<Kandydat>> PobierzKandydatowDlaWyborcyAsync(string pesel)
    {
        var wyborca = await ZnajdzWyborceAsync(pesel);
        return await PobierzKandydatowWOkreguAsync(wyborca.OkregId);
    }

    public async Task OddajGlosAsync(string pesel, int kandydatId)
    {
        var wyborca = await ZnajdzWyborceAsync(pesel);
        await ZapiszGlosAsync(wyborca, kandydatId);
    }

    // Zapytanie po tabeli Glosy zamiast po nawigacji w.Glos == null: tłumaczy się
    // tak samo w SQLite i w dostawcy InMemory używanym w testach.
    public async Task<IReadOnlyList<Wyborca>> PobierzNieglosujacychAsync(int okregId)
        => await db.Wyborcy
            .Where(w => w.OkregId == okregId
                        && w.CzynnePrawoWyborcze
                        && !db.Glosy.Any(g => g.WyborcaId == w.Id))
            .OrderBy(w => w.Nazwisko).ThenBy(w => w.Imie)
            .ToListAsync();

    public async Task<int> PoliczUprawnionychAsync(int okregId)
        => await db.Wyborcy.CountAsync(w => w.OkregId == okregId && w.CzynnePrawoWyborcze);

    public async Task<IReadOnlyList<Kandydat>> PobierzKandydatowWOkreguAsync(int okregId)
        => await db.Kandydaci
            .Include(k => k.Wyborca)
            .Include(k => k.Komitet)
            .Where(k => k.OkregId == okregId)
            .OrderBy(k => k.Komitet!.Nazwa).ThenBy(k => k.NumerNaLiscie)
            .ToListAsync();

    public async Task<Kandydat> OddajGlosAsync(int wyborcaId, int kandydatId)
    {
        var wyborca = await db.Wyborcy.FindAsync(wyborcaId)
            ?? throw new BladRegulyBiznesowej("Wskazany wyborca nie istnieje.");

        return await ZapiszGlosAsync(wyborca, kandydatId);
    }

    private async Task<Wyborca> ZnajdzWyborceAsync(string pesel)
        => await db.Wyborcy.FirstOrDefaultAsync(w => w.Pesel == pesel)
           ?? throw new BladRegulyBiznesowej("Nie znaleziono wyborcy o podanym numerze PESEL.");

    private async Task<Kandydat> ZapiszGlosAsync(Wyborca wyborca, int kandydatId)
    {
        if (kandydatId <= 0)
            throw new BladRegulyBiznesowej("Nie wybrano kandydata.");

        if (!wyborca.CzynnePrawoWyborcze)
            throw new BladRegulyBiznesowej("Wyborca nie posiada czynnego prawa wyborczego (nie może głosować).");

        if (await db.Glosy.AnyAsync(g => g.WyborcaId == wyborca.Id))
            throw new BladRegulyBiznesowej("Ten wyborca już oddał głos.");

        var kandydat = await db.Kandydaci
            .Include(k => k.Wyborca)
            .Include(k => k.Komitet)
            .FirstOrDefaultAsync(k => k.Id == kandydatId)
            ?? throw new BladRegulyBiznesowej("Wskazany kandydat nie istnieje.");

        if (kandydat.OkregId != wyborca.OkregId)
            throw new BladRegulyBiznesowej("Można głosować wyłącznie na kandydata startującego we własnym okręgu.");

        db.Glosy.Add(new Glos
        {
            WyborcaId = wyborca.Id,
            KandydatId = kandydatId,
            DataOddania = DateTime.UtcNow
        });

        // Indeks unikalny na Glos.WyborcaId to ostatnia linia obrony przed wyścigiem
        // dwóch równoczesnych żądań: SaveChanges rzuci wtedy DbUpdateException.
        await db.SaveChangesAsync();

        return kandydat;
    }
}
