using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Wybory.Web.Dane;

namespace Wybory.Web.Pages.Wyborcy;

public class ListaModel(BazaDanych db) : PageModel
{
    public List<Okreg> Okregi { get; private set; } = [];
    public int? OkregId { get; private set; }
    public string? Szukaj { get; private set; }
    public List<PozycjaListy> Wyborcy { get; private set; } = [];
    public int Wszystkich { get; private set; }

    public async Task OnGetAsync(int? okregId, string? szukaj)
    {
        Okregi = await db.Okregi.OrderBy(o => o.Id).ToListAsync();
        OkregId = okregId;
        Szukaj = szukaj;

        var zapytanie = db.Wyborcy.AsQueryable();

        if (okregId is not null)
            zapytanie = zapytanie.Where(w => w.OkregId == okregId);

        if (!string.IsNullOrWhiteSpace(szukaj))
        {
            // Contains tłumaczy się na LIKE, które w SQLite ignoruje wielkość liter
            // tylko dla ASCII: "łuk" i "Łuk" to dla niego różne frazy.
            var fraza = szukaj.Trim();
            zapytanie = zapytanie.Where(w =>
                w.Nazwisko.Contains(fraza) || w.Imie.Contains(fraza) || w.Pesel.Contains(fraza));
        }

        Wszystkich = await db.Wyborcy.CountAsync();

        Wyborcy = await zapytanie
            .OrderBy(w => w.Nazwisko).ThenBy(w => w.Imie)
            .Select(w => new PozycjaListy(
                w.Id,
                w.Pesel,
                w.Imie,
                w.Nazwisko,
                w.Okreg!.Nazwa,
                w.CzynnePrawoWyborcze,
                w.BierneProwoWyborcze,
                db.Glosy.Any(g => g.WyborcaId == w.Id),
                db.Kandydaci.Any(k => k.WyborcaId == w.Id)))
            .ToListAsync();
    }

    public record PozycjaListy(
        int Id,
        string Pesel,
        string Imie,
        string Nazwisko,
        string NazwaOkregu,
        bool CzynnePrawo,
        bool BiernePrawo,
        bool Zaglosowal,
        bool Kandyduje);
}
