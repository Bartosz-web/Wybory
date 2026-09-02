using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Wybory.Web.Dane;

namespace Wybory.Web.Pages.Komitety;

public class ListaModel(BazaDanych db) : PageModel
{
    public List<PozycjaListy> Komitety { get; private set; } = [];

    public async Task OnGetAsync()
    {
        // EF Core nie materializuje kolekcji przekazanej jako argument konstruktora,
        // dlatego rekord budujemy dopiero w pamięci.
        var surowe = await db.Komitety
            .OrderBy(k => k.Nazwa)
            .Select(k => new
            {
                k.Id,
                k.Nazwa,
                Okregi = k.Okregi.Select(ko => ko.Okreg!.Nazwa).ToList(),
                Kandydatow = k.Kandydaci.Count,
                Glosow = k.Kandydaci.SelectMany(kan => kan.Glosy).Count()
            })
            .ToListAsync();

        Komitety = surowe
            .Select(k => new PozycjaListy(
                k.Id, k.Nazwa, k.Okregi.OrderBy(n => n).ToList(), k.Kandydatow, k.Glosow))
            .ToList();
    }

    public record PozycjaListy(int Id, string Nazwa, List<string> Okregi, int Kandydatow, int Glosow);
}
