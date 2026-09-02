using Wybory.Web.Uslugi.Formuly;

namespace Wybory.Tests;

// Podział mandatów musi dawać ten sam wynik dla tych samych danych, niezależnie
// od kolejności iteracji po słowniku, która nie jest niczym gwarantowana.
public class PodzialDzielnikowyTests
{
    [Fact]
    public void RemisIlorazow_WynikNiezaleznyOdKolejnosciWSlowniku()
    {
        var jednaKolejnosc = new Dictionary<int, int> { [1] = 100, [2] = 100 };
        var odwrotnaKolejnosc = new Dictionary<int, int> { [2] = 100, [1] = 100 };

        var pierwszy = new FormulaDHondta().PodzielMandaty(jednaKolejnosc, 1);
        var drugi = new FormulaDHondta().PodzielMandaty(odwrotnaKolejnosc, 1);

        Assert.Equal(pierwszy[1], drugi[1]);
        Assert.Equal(pierwszy[2], drugi[2]);
        Assert.Equal(1, pierwszy[1] + pierwszy[2]);
    }

    [Fact]
    public void RemisIlorazow_PrzyRownymPoparciuWygrywaMniejszeId()
    {
        var glosy = new Dictionary<int, int> { [2] = 100, [1] = 100 };

        var mandaty = new FormulaDHondta().PodzielMandaty(glosy, 1);

        Assert.Equal(1, mandaty[1]);
        Assert.Equal(0, mandaty[2]);
    }

    [Fact]
    public void RemisIlorazow_WygrywaKomitetZWiekszymPoparciem()
    {
        // A=100, B=200, 3 mandaty. Ilorazy: 200 (B/1), 100 (B/2), 100 (A/1), 66,67 (B/3).
        // Drugi mandat rozstrzyga remis 100 na 100 na korzyść komitetu z większym poparciem.
        var glosy = new Dictionary<int, int> { [1] = 100, [2] = 200 };

        var mandaty = new FormulaDHondta().PodzielMandaty(glosy, 3);

        Assert.Equal(2, mandaty[2]);
        Assert.Equal(1, mandaty[1]);
    }

    [Fact]
    public void WiecejMandatowNizGlosow_NieGubiZadnegoMandatu()
    {
        var glosy = new Dictionary<int, int> { [1] = 3, [2] = 1 };

        var mandaty = new FormulaDHondta().PodzielMandaty(glosy, 10);

        Assert.Equal(10, mandaty.Values.Sum());
    }

    [Fact]
    public void PustyZestawKomitetow_ZwracaPustyPodzial()
    {
        var mandaty = new FormulaDHondta().PodzielMandaty(new Dictionary<int, int>(), 5);

        Assert.Empty(mandaty);
    }
}
