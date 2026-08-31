using Microsoft.EntityFrameworkCore;
using Wybory.Web.Dane;

namespace Wybory.Web.Uslugi;

// Oddawanie głosów (pkt 4 zadania).
public class UslugaGlosowania(BazaDanych db)
{
    public async Task<IReadOnlyList<Kandydat>> PobierzKandydatowDlaWyborcyAsync(string pesel)
    {
        var wyborca = await db.Wyborcy.FirstOrDefaultAsync(w => w.Pesel == pesel)
            ?? throw new BladRegulyBiznesowej("Nie znaleziono wyborcy o podanym numerze PESEL.");

        return await db.Kandydaci
            .Include(k => k.Wyborca)
            .Include(k => k.Komitet)
            .Where(k => k.OkregId == wyborca.OkregId)
            .OrderBy(k => k.KomitetId).ThenBy(k => k.NumerNaLiscie)
            .ToListAsync();
    }

    public async Task OddajGlosAsync(string pesel, int kandydatId)
    {
        var wyborca = await db.Wyborcy.FirstOrDefaultAsync(w => w.Pesel == pesel)
            ?? throw new BladRegulyBiznesowej("Nie znaleziono wyborcy o podanym numerze PESEL.");

        if (!wyborca.CzynnePrawoWyborcze)
            throw new BladRegulyBiznesowej("Wyborca nie posiada czynnego prawa wyborczego (nie może głosować.)");

        if (await db.Glosy.AnyAsync(g => g.WyborcaId == wyborca.Id))
            throw new BladRegulyBiznesowej("Ten wyborca już oddał głos.");

        var kandydat = await db.Kandydaci.FindAsync(kandydatId)
            ?? throw new BladRegulyBiznesowej("Wskazany kandydat nie istnieje.");

        if (kandydat.OkregId != wyborca.OkregId)
            throw new BladRegulyBiznesowej("Można głosować wyłącznie na kandydata startującego we własnym okręgu.");

        db.Glosy.Add(new Glos
        {
            WyborcaId = wyborca.Id,
            KandydatId = kandydatId,
            DataOddania = DateTime.UtcNow
        });

        // Indeks unikalny na Glos.WyborcaId to ostatnia linia obrony przed
        // wyścigiem (dwa równoczesne żądania) — SaveChanges rzuci DbUpdateException.
        await db.SaveChangesAsync();
    }
}
