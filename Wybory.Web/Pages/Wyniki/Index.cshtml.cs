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

    public List<Okreg> Okregi { get; private set; } = [];
    public WynikiOkregu? Wyniki { get; private set; }

    public async Task OnGetAsync(int? okregId, string? formula)
    {
        Okregi = await db.Okregi.OrderBy(o => o.Id).ToListAsync();

        if (okregId is null || !Formuly.TryGetValue(formula ?? "", out var wybranaFormula))
            return;

        Wyniki = await uslugaZliczania.ZliczWynikiAsync(okregId.Value, wybranaFormula);
    }
}
