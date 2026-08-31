using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Wybory.Web.Dane;
using Wybory.Web.Uslugi;

namespace Wybory.Web.Pages.Wyborcy;

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
            await uslugaRejestracji.RejestrujWyborceAsync(
                Formularz.Pesel, Formularz.Imie, Formularz.Nazwisko, Formularz.OkregId,
                Formularz.CzynnePrawoWyborcze, Formularz.BierneProwoWyborcze);
            Komunikat = $"Zarejestrowano wyborcę: {Formularz.Imie} {Formularz.Nazwisko}.";
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
        [Required(ErrorMessage = "PESEL jest wymagany.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "PESEL musi mieć 11 cyfr.")]
        public string Pesel { get; set; } = "";

        [Required(ErrorMessage = "Imię jest wymagane.")]
        public string Imie { get; set; } = "";

        [Required(ErrorMessage = "Nazwisko jest wymagane.")]
        public string Nazwisko { get; set; } = "";

        [Range(1, int.MaxValue, ErrorMessage = "Wybierz okręg.")]
        public int OkregId { get; set; }

        public bool CzynnePrawoWyborcze { get; set; }
        public bool BierneProwoWyborcze { get; set; }
    }
}
