using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Wybory.Web.Dane;

namespace Wybory.Web.Pages.Kandydaci;

public class ListaModel(BazaDanych db) : PageModel
{
    public List<Okreg> Okregi { get; private set; } = [];
    public int? OkregId { get; private set; }
    public List<GrupaKomitetu> Komitety { get; private set; } = [];

    public async Task OnGetAsync(int? okregId)
    {
        Okregi = await db.Okregi.OrderBy(o => o.Id).ToListAsync();
        OkregId = okregId;
        if (okregId is null)
            return;

        var kandydaci = await db.Kandydaci
            .Include(k => k.Wyborca)
            .Include(k => k.Komitet)
            .Where(k => k.OkregId == okregId)
            .OrderBy(k => k.NumerNaLiscie)
            .ToListAsync();

        // Grupowanie po komitecie wykonywane w pamięci (po materializacji) —
        // GroupBy + zagnieżdżona lista encji nie zawsze da się przetłumaczyć na SQL.
        Komitety = kandydaci
            .GroupBy(k => new { k.KomitetId, Nazwa = k.Komitet!.Nazwa })
            .OrderBy(g => g.Key.Nazwa)
            .Select(g => new GrupaKomitetu(g.Key.KomitetId, g.Key.Nazwa, g.ToList()))
            .ToList();
    }

    public record GrupaKomitetu(int KomitetId, string NazwaKomitetu, List<Kandydat> Kandydaci);
}
