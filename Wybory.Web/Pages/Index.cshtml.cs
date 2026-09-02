using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Wybory.Web.Dane;
using Wybory.Web.Uslugi;
using Wybory.Web.Uslugi.Formuly;

namespace Wybory.Web.Pages;

public class IndexModel(BazaDanych db, UslugaSymulacji uslugaSymulacji, UslugaZliczania uslugaZliczania) : PageModel
{
    public List<KafelekOkregu> Kafelki { get; private set; } = [];
    public List<WynikiOkregu>? WynikiSymulacji { get; private set; }

    public async Task OnGetAsync(bool symulacja)
    {
        await ZaladujKafelkiAsync();

        if (symulacja)
            WynikiSymulacji = (await uslugaZliczania.PodsumujWszystkieOkregiAsync(new FormulaDHondta())).Okregi.ToList();
    }

    public async Task<IActionResult> OnPostSymulujAsync()
    {
        await uslugaSymulacji.SymulujAsync();

        // POST-Redirect-Get, żeby odświeżenie strony nie ponawiało symulacji.
        // Wyniki są po przekierowaniu przeliczane odczytem.
        return RedirectToPage(new { symulacja = true });
    }

    // Projekcja zamiast Include: kafelki potrzebują wyłącznie liczb.
    private async Task ZaladujKafelkiAsync()
    {
        Kafelki = await db.Okregi
            .OrderBy(o => o.Id)
            .Select(o => new KafelekOkregu(
                o.Id,
                o.Nazwa,
                o.LiczbaMandatow,
                o.Wyborcy.Count,
                o.Wyborcy.Count(w => w.CzynnePrawoWyborcze),
                o.KomitetyOkregu.Count,
                o.Kandydaci.Count,
                o.Kandydaci.SelectMany(k => k.Glosy).Count()))
            .ToListAsync();
    }

    // Liczba głosów w okręgu to suma głosów jego kandydatów.
    public record KafelekOkregu(
        int Id,
        string Nazwa,
        int LiczbaMandatow,
        int Wyborcow,
        int Uprawnionych,
        int Komitetow,
        int Kandydatow,
        int Glosow)
    {
        public double Frekwencja => Uprawnionych > 0 ? Glosow * 100.0 / Uprawnionych : 0;
    }
}
