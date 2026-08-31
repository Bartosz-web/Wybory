using Microsoft.EntityFrameworkCore;

namespace Wybory.Web.Dane;

// Uruchamiany raz przy starcie aplikacji:
// 1) tworzy/aktualizuje bazę (migracje),
// 2) zakłada 3 okręgi wyborcze, jeśli baza jest pusta (pkt 0).
public static class InicjalizatorBazy
{
    public static void Inicjalizuj(IServiceProvider uslugi)
    {
        using var zakres = uslugi.CreateScope();
        var db = zakres.ServiceProvider.GetRequiredService<BazaDanych>();

        db.Database.Migrate();

        if (!db.Okregi.Any())
        {
            db.Okregi.AddRange(
                new Okreg { Nazwa = "Okręg nr 1", LiczbaMandatow = 5 },
                new Okreg { Nazwa = "Okręg nr 2", LiczbaMandatow = 5 },
                new Okreg { Nazwa = "Okręg nr 3", LiczbaMandatow = 5 });
            db.SaveChanges();
        }
    }
}
