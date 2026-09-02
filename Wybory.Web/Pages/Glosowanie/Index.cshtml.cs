using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Wybory.Web.Dane;
using Wybory.Web.Uslugi;

namespace Wybory.Web.Pages.Glosowanie;

public class IndexModel(BazaDanych db, UslugaGlosowania uslugaGlosowania) : PageModel
{
    // Trzymany w adresie, a przy oddawaniu głosu przekazywany ukrytym polem.
    [BindProperty(SupportsGet = true)]
    public int? OkregId { get; set; }

    [BindProperty]
    public DaneFormularza Formularz { get; set; } = new();

    public List<Okreg> Okregi { get; private set; } = [];
    public List<Wyborca> Nieglosujacy { get; private set; } = [];
    public List<ListaKomitetu> Listy { get; private set; } = [];
    public int LiczbaUprawnionych { get; private set; }
    public int LiczbaGlosow { get; private set; }
    public string? Komunikat { get; private set; }

    public double Frekwencja => LiczbaUprawnionych > 0 ? LiczbaGlosow * 100.0 / LiczbaUprawnionych : 0;

    public async Task OnGetAsync() => await ZaladujAsync();

    public async Task<IActionResult> OnPostOddajGlosAsync()
    {
        if (!ModelState.IsValid)
        {
            await ZaladujAsync();
            return Page();
        }

        try
        {
            var kandydat = await uslugaGlosowania.OddajGlosAsync(Formularz.WyborcaId, Formularz.KandydatId);
            Komunikat = $"Głos zapisany: {kandydat.Wyborca!.Imie} {kandydat.Wyborca!.Nazwisko} "
                        + $"({kandydat.Komitet!.Nazwa}, nr {kandydat.NumerNaLiscie}). Dziękujemy za udział w głosowaniu.";
            ModelState.Clear();
            Formularz = new DaneFormularza();
        }
        catch (BladRegulyBiznesowej e)
        {
            ModelState.AddModelError(string.Empty, e.Message);
        }

        // Po zapisie wyborca znika z listy, a frekwencja się aktualizuje.
        await ZaladujAsync();
        return Page();
    }

    private async Task ZaladujAsync()
    {
        Okregi = await db.Okregi.OrderBy(o => o.Id).ToListAsync();

        if (OkregId is not int okregId)
            return;

        Nieglosujacy = (await uslugaGlosowania.PobierzNieglosujacychAsync(okregId)).ToList();
        LiczbaUprawnionych = await uslugaGlosowania.PoliczUprawnionychAsync(okregId);
        LiczbaGlosow = LiczbaUprawnionych - Nieglosujacy.Count;

        var kandydaci = await uslugaGlosowania.PobierzKandydatowWOkreguAsync(okregId);

        // Karta do głosowania grupuje kandydatów pod nazwami list komitetów.
        Listy = kandydaci
            .GroupBy(k => new { k.KomitetId, Nazwa = k.Komitet!.Nazwa })
            .OrderBy(g => g.Key.Nazwa)
            .Select(g => new ListaKomitetu(g.Key.KomitetId, g.Key.Nazwa, g.OrderBy(k => k.NumerNaLiscie).ToList()))
            .ToList();
    }

    public record ListaKomitetu(int KomitetId, string NazwaKomitetu, List<Kandydat> Kandydaci);

    public class DaneFormularza
    {
        [Range(1, int.MaxValue, ErrorMessage = "Wybierz wyborcę z listy.")]
        public int WyborcaId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Wybierz kandydata z karty do głosowania.")]
        public int KandydatId { get; set; }
    }
}
