using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Wybory.Web.Dane;
using Wybory.Web.Uslugi;

namespace Wybory.Web.Pages;

public class IndexModel(BazaDanych db, UslugaSymulacji uslugaSymulacji) : PageModel
{
    public List<Okreg> Okregi { get; private set; } = [];
    public List<WynikiOkregu>? WynikiSymulacji { get; private set; }

    public async Task OnGetAsync() => await ZaladujOkregiAsync();

    public async Task<IActionResult> OnPostSymulujAsync()
    {
        WynikiSymulacji = await uslugaSymulacji.SymulujAsync();
        await ZaladujOkregiAsync();
        return Page();
    }

    private async Task ZaladujOkregiAsync()
    {
        Okregi = await db.Okregi
            .Include(o => o.Wyborcy)
            .Include(o => o.KomitetyOkregu)
            .Include(o => o.Kandydaci)
            .OrderBy(o => o.Id)
            .ToListAsync();
    }
}
