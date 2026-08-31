using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wybory.Web.Dane;
using Wybory.Web.Uslugi;

namespace Wybory.Web.Pages.Glosowanie;

public class IndexModel(UslugaGlosowania uslugaGlosowania) : PageModel
{
    [BindProperty]
    public DaneFormularza Formularz { get; set; } = new();

    public List<Kandydat> Kandydaci { get; private set; } = [];
    public string? Komunikat { get; private set; }

    public void OnGet() { }

    // Krok 1: podanie PESEL-u -> pokazanie listy kandydatów we własnym okręgu.
    public async Task<IActionResult> OnPostPokazKandydatowAsync()
    {
        ModelState.Clear();
        TryValidateModel(Formularz, nameof(Formularz));

        try
        {
            Kandydaci = (await uslugaGlosowania.PobierzKandydatowDlaWyborcyAsync(Formularz.Pesel)).ToList();
            if (Kandydaci.Count == 0)
                ModelState.AddModelError(string.Empty, "W Twoim okręgu nie zarejestrowano żadnych kandydatów.");
        }
        catch (BladRegulyBiznesowej e)
        {
            ModelState.AddModelError(string.Empty, e.Message);
        }
        return Page();
    }

    // Krok 2: oddanie głosu na wybranego kandydata.
    public async Task<IActionResult> OnPostOddajGlosAsync()
    {
        try
        {
            await uslugaGlosowania.OddajGlosAsync(Formularz.Pesel, Formularz.KandydatId);
            Komunikat = "Twój głos został zapisany. Dziękujemy za udział w głosowaniu.";
            Formularz = new DaneFormularza();
        }
        catch (BladRegulyBiznesowej e)
        {
            ModelState.AddModelError(string.Empty, e.Message);
            Kandydaci = (await uslugaGlosowania.PobierzKandydatowDlaWyborcyAsync(Formularz.Pesel)).ToList();
        }
        return Page();
    }

    public class DaneFormularza
    {
        [Required(ErrorMessage = "PESEL jest wymagany.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "PESEL musi mieć 11 cyfr.")]
        public string Pesel { get; set; } = "";

        public int KandydatId { get; set; }
    }
}
