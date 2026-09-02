namespace Wybory.Web.Uslugi.Formuly;

// Wspólny mechanizm metod dzielnikowych (D'Hondt, Sainte-Laguë): każdy komitet
// generuje ciąg ilorazów głosy/dzielnik(1), głosy/dzielnik(2), ...; mandaty
// dostają komitety z największymi ilorazami spośród wszystkich komitetów łącznie.
internal static class PodzialDzielnikowy
{
    public static IReadOnlyDictionary<int, int> Podziel(
        IReadOnlyDictionary<int, int> glosyNaKomitety, int liczbaMandatow, Func<int, double> dzielnik)
    {
        var mandaty = glosyNaKomitety.Keys.ToDictionary(id => id, _ => 0);
        if (liczbaMandatow <= 0 || glosyNaKomitety.Count == 0)
            return mandaty;

        var ilorazy = new List<(int KomitetId, double Iloraz)>();
        foreach (var (komitetId, glosy) in glosyNaKomitety)
        {
            if (glosy <= 0) continue;
            for (var miejsce = 1; miejsce <= liczbaMandatow; miejsce++)
                ilorazy.Add((komitetId, glosy / dzielnik(miejsce)));
        }

        // Remis ilorazów rozstrzygany jawnie, żeby wynik był powtarzalny: większy
        // iloraz, potem większe poparcie komitetu, na końcu mniejsze Id. Kodeks
        // wyborczy rozstrzyga taki remis losowaniem.
        var kolejnosc = ilorazy
            .OrderByDescending(i => i.Iloraz)
            .ThenByDescending(i => glosyNaKomitety[i.KomitetId])
            .ThenBy(i => i.KomitetId);

        foreach (var (komitetId, _) in kolejnosc.Take(liczbaMandatow))
            mandaty[komitetId]++;

        return mandaty;
    }
}
