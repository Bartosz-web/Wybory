using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Wybory.Web.Dane;
using Wybory.Web.Uslugi;

namespace Wybory.Web.Pages.Komitety;

public class RejestracjaModel(BazaDanych db, UslugaRejestracji uslugaRejestracji) : PageModel
{
    [BindProperty]
    public DaneFormularza Formularz { get; set; } = new();

    public List<Okreg> Okregi { get; private set; } = [];
    public string? Komunikat { get; private set; }

    public async Task OnGetAsync() => Okregi = await db.Okregi.OrderBy(o => o.Id).ToListAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        Okregi = await db.Okregi.OrderBy(o => o.Id).ToListAsync();
        if (!ModelState.IsValid)
            return Page();

        try
        {
            var komitet = await uslugaRejestracji.RejestrujKomitetAsync(Formularz.Nazwa, Formularz.OkregiIds);
            Komunikat = $"Zarejestrowano komitet: {komitet.Nazwa} w {Formularz.OkregiIds.Count} okręg(ach).";
            ModelState.Clear();
            Formularz = new DaneFormularza();
        }
        catch (BladRegulyBiznesowej e)
        {
            ModelState.AddModelError(string.Empty, e.Message);
        }
        return Page();
    }

    public class DaneFormularza
    {
        [Required(ErrorMessage = "Nazwa komitetu jest wymagana.")]
        public string Nazwa { get; set; } = "";

        public List<int> OkregiIds { get; set; } = [];
    }
}
