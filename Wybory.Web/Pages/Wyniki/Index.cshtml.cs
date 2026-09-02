using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Wybory.Web.Dane;
using Wybory.Web.Uslugi;
using Wybory.Web.Uslugi.Formuly;

namespace Wybory.Web.Pages.Wyniki;

public class IndexModel(BazaDanych db, UslugaZliczania uslugaZliczania) : PageModel
{
    private static readonly Dictionary<string, IFormulaPodzialuMandatow> Formuly = new()
    {
        ["dhondt"] = new FormulaDHondta(),
        ["sainte-lague"] = new FormulaSainteLague()
    };

    public const string DomyslnaFormula = "dhondt";

    public List<Okreg> Okregi { get; private set; } = [];

    // Stan formularza: kontrolki mają pokazywać to, co zostało policzone.
    public int? OkregId { get; private set; }
    public string Formula { get; private set; } = DomyslnaFormula;
    public bool Prog { get; private set; }

    public WynikiOkregu? Wyniki { get; private set; }
    public WynikiKrajowe? Podsumowanie { get; private set; }

    public string? Blad { get; private set; }

    public double ProgProcentowy => Prog ? UslugaZliczania.ProgSejmowy : 0;

    // Jedno źródło prawdy dla wysokości progu.
    public string EtykietaProgu => $"Próg wyborczy {UslugaZliczania.ProgSejmowy:0.#}%";
    public string OpisProgu => Prog ? $", próg {UslugaZliczania.ProgSejmowy:0.#}%" : "";

    public async Task OnGetAsync(int? okregId, string? formula, bool prog)
    {
        Okregi = await db.Okregi.OrderBy(o => o.Id).ToListAsync();

        OkregId = okregId;
        Prog = prog;

        // Nieznana lub brakująca formuła cofa się do domyślnej.
        Formula = formula is not null && Formuly.ContainsKey(formula) ? formula : DomyslnaFormula;
        var wybranaFormula = Formuly[Formula];

        if (okregId is null)
        {
            // Brak wyboru okręgu oznacza podsumowanie wszystkich okręgów.
            Podsumowanie = await uslugaZliczania.PodsumujWszystkieOkregiAsync(wybranaFormula, ProgProcentowy);
            return;
        }

        try
        {
            Wyniki = await uslugaZliczania.ZliczWynikiAsync(okregId.Value, wybranaFormula, ProgProcentowy);
        }
        catch (BladRegulyBiznesowej e)
        {
            // Ochrona przed ręcznie wpisanym adresem z nieistniejącym okręgiem.
            Blad = e.Message;
            OkregId = null;
        }
    }
}
