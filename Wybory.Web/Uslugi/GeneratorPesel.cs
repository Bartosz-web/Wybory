namespace Wybory.Web.Uslugi;

// Losowy, ale poprawny numer PESEL (wiarygodna data urodzenia + suma kontrolna) —
// ten sam algorytm co w wwwroot/js/losowanie.js i PeselWalidator.
internal static class GeneratorPesel
{
    private static readonly int[] Wagi = [1, 3, 7, 9, 1, 3, 7, 9, 1, 3];

    public static string Losuj(Random losowy, HashSet<string> jużWylosowane)
    {
        string pesel;
        do
        {
            var rok = 1950 + losowy.Next(60);
            var kodMiesiaca = (rok >= 2000 ? 20 : 0) + losowy.Next(1, 13);
            var dzien = losowy.Next(1, 29);
            var seria = losowy.Next(0, 10000);
            var pierwsze10 = $"{rok % 100:D2}{kodMiesiaca:D2}{dzien:D2}{seria:D4}";

            var suma = 0;
            for (var i = 0; i < 10; i++)
                suma += Wagi[i] * (pierwsze10[i] - '0');
            var cyfraKontrolna = (10 - suma % 10) % 10;

            pesel = pierwsze10 + cyfraKontrolna;
        } while (!jużWylosowane.Add(pesel));

        return pesel;
    }
}
