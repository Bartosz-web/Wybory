using Microsoft.EntityFrameworkCore;
using Wybory.Web.Dane;

namespace Wybory.Web.Uslugi;

// Rejestracja wyborców, komitetów i kandydatów (pkt 1-3 zadania).
public class UslugaRejestracji(BazaDanych db)
{
    public const int MaksymalnaLiczbaKandydatowNaLiscie = 10;

    public async Task<Wyborca> RejestrujWyborceAsync(
        string pesel, string imie, string nazwisko, int okregId, bool czynnePrawo, bool biernePrawo)
    {
        if (!PeselWalidator.CzyPoprawny(pesel))
            throw new BladRegulyBiznesowej("Nieprawidłowy numer PESEL.");

        if (await db.Wyborcy.AnyAsync(w => w.Pesel == pesel))
            throw new BladRegulyBiznesowej("Wyborca z tym numerem PESEL jest już zarejestrowany.");

        if (!await db.Okregi.AnyAsync(o => o.Id == okregId))
            throw new BladRegulyBiznesowej("Wskazany okręg nie istnieje.");

        var wyborca = new Wyborca
        {
            Pesel = pesel,
            Imie = imie,
            Nazwisko = nazwisko,
            OkregId = okregId,
            CzynnePrawoWyborcze = czynnePrawo,
            BierneProwoWyborcze = biernePrawo
        };
        db.Wyborcy.Add(wyborca);
        await db.SaveChangesAsync();
        return wyborca;
    }

    public async Task<Komitet> RejestrujKomitetAsync(string nazwa, IReadOnlyCollection<int> okregiIds)
    {
        if (string.IsNullOrWhiteSpace(nazwa))
            throw new BladRegulyBiznesowej("Nazwa komitetu jest wymagana.");

        if (okregiIds.Count == 0)
            throw new BladRegulyBiznesowej("Komitet musi być zarejestrowany w co najmniej jednym okręgu.");

        if (await db.Komitety.AnyAsync(k => k.Nazwa == nazwa))
            throw new BladRegulyBiznesowej("Komitet o tej nazwie jest już zarejestrowany.");

        var poprawneOkregi = await db.Okregi
            .Where(o => okregiIds.Contains(o.Id))
            .Select(o => o.Id)
            .ToListAsync();
        if (poprawneOkregi.Count != okregiIds.Distinct().Count())
            throw new BladRegulyBiznesowej("Wskazano nieistniejący okręg.");

        var komitet = new Komitet { Nazwa = nazwa };
        komitet.Okregi = poprawneOkregi.Select(id => new KomitetOkreg { OkregId = id }).ToList();

        db.Komitety.Add(komitet);
        await db.SaveChangesAsync();
        return komitet;
    }

    public async Task<Kandydat> RejestrujKandydataAsync(int wyborcaId, int komitetId, int okregId, int numerNaLiscie)
    {
        var wyborca = await db.Wyborcy.FindAsync(wyborcaId)
            ?? throw new BladRegulyBiznesowej("Wskazany wyborca nie istnieje.");

        if (!wyborca.BierneProwoWyborcze)
            throw new BladRegulyBiznesowej("Wyborca nie posiada biernego prawa wyborczego (nie może kandydować).");

        if (await db.Kandydaci.AnyAsync(k => k.WyborcaId == wyborcaId))
            throw new BladRegulyBiznesowej("Ten wyborca już kandyduje.");

        // Komitet musi istnieć i być zarejestrowany właśnie w tym okręgu (pkt 3).
        var zarejestrowanyWOkregu = await db.KomitetyOkregow
            .AnyAsync(ko => ko.KomitetId == komitetId && ko.OkregId == okregId);
        if (!zarejestrowanyWOkregu)
            throw new BladRegulyBiznesowej("Komitet nie jest zarejestrowany we wskazanym okręgu.");

        if (numerNaLiscie is < 1 or > MaksymalnaLiczbaKandydatowNaLiscie)
            throw new BladRegulyBiznesowej($"Numer na liście musi być z zakresu 1-{MaksymalnaLiczbaKandydatowNaLiscie}.");

        // Numer na liście musi być unikatowy w obrębie listy komitetu w danym okręgu —
        // razem z zakresem 1..MaksymalnaLiczbaKandydatowNaLiscie to też twardy limit rozmiaru listy.
        var numerZajety = await db.Kandydaci
            .AnyAsync(k => k.KomitetId == komitetId && k.OkregId == okregId && k.NumerNaLiscie == numerNaLiscie);
        if (numerZajety)
            throw new BladRegulyBiznesowej("Ten numer na liście jest już zajęty przez innego kandydata tego komitetu.");

        var kandydat = new Kandydat
        {
            WyborcaId = wyborcaId,
            KomitetId = komitetId,
            OkregId = okregId,
            NumerNaLiscie = numerNaLiscie
        };
        db.Kandydaci.Add(kandydat);
        await db.SaveChangesAsync();
        return kandydat;
    }
}
