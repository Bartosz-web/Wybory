using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Wybory.Web.Pages;

// Podpięta w Program.cs przez UseStatusCodePagesWithReExecute.
public class BladModel : PageModel
{
    public int Kod { get; private set; }
    public string Naglowek { get; private set; } = "Coś poszło nie tak";
    public string Opis { get; private set; } = "";

    public void OnGet(int? kod)
    {
        Kod = kod ?? 404;

        (Naglowek, Opis) = Kod switch
        {
            404 => ("Nie znaleziono strony",
                    "Adres, który otworzyłeś, nie istnieje. Mógł się zmienić albo zawierać literówkę."),
            403 => ("Brak dostępu",
                    "Nie masz uprawnień do tej strony."),
            _ => ($"Błąd {Kod}",
                  "Żądanie nie mogło zostać zrealizowane.")
        };
    }
}
