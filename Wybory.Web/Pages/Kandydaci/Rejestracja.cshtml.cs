using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Wybory.Web.Dane;
using Wybory.Web.Uslugi;

namespace Wybory.Web.Pages.Kandydaci;

public class RejestracjaModel(BazaDanych db, UslugaRejestracji uslugaRejestracji) : PageModel
{
    [BindProperty]
    public DaneFormularza Formularz { get; set; } = new();

    public List<Wyborca> UprawnieniWyborcy { get; private set; } = [];
    public List<Komitet> Komitety { get; private set; } = [];
    public List<Okreg> Okregi { get; private set; } = [];
    public string? Komunikat { get; private set; }

    public async Task OnGetAsync() => await ZaladujListyAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        await ZaladujListyAsync();
        if (!ModelState.IsValid)
            return Page();

        try
        {
            await uslugaRejestracji.RejestrujKandydataAsync(
                Formularz.WyborcaId, Formularz.KomitetId, Formularz.OkregId, Formularz.NumerNaLiscie);
            Komunikat = "Zarejestrowano kandydata.";
            ModelState.Clear();
            Formularz = new DaneFormularza();
            await ZaladujListyAsync();
        }
        catch (BladRegulyBiznesowej e)
        {
            ModelState.AddModelError(string.Empty, e.Message);
        }
        return Page();
    }

    private async Task ZaladujListyAsync()
    {
        UprawnieniWyborcy = await db.Wyborcy
            .Where(w => w.BierneProwoWyborcze && w.Kandydatura == null)
            .OrderBy(w => w.Nazwisko).ThenBy(w => w.Imie)
            .ToListAsync();
        Komitety = await db.Komitety.OrderBy(k => k.Nazwa).ToListAsync();
        Okregi = await db.Okregi.OrderBy(o => o.Id).ToListAsync();
    }

    public class DaneFormularza
    {
        [Range(1, int.MaxValue, ErrorMessage = "Wybierz wyborcę.")]
        public int WyborcaId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Wybierz komitet.")]
        public int KomitetId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Wybierz okręg.")]
        public int OkregId { get; set; }

        [Display(Name = "Numer na liście")]
        [Range(1, UslugaRejestracji.MaksymalnaLiczbaKandydatowNaLiscie, ErrorMessage = "Numer na liście musi być z zakresu 1-10.")]
        public int NumerNaLiscie { get; set; } = 1;
    }
}
