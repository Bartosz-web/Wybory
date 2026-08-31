using Wybory.Web.Uslugi.Formuly;

namespace Wybory.Tests;

public class FormulaSainteLagueTests
{
    [Fact]
    public void DzieliMandatyProporcjonalnieWgIlorazow()
    {
        // A=310, B=200, C=100, 4 mandaty.
        // Ilorazy (dzielniki 1,3,5,7), 4 największe to: 310(A/1), 200(B/1), 103.33(A/3), 100(C/1).
        var glosy = new Dictionary<int, int> { [1] = 310, [2] = 200, [3] = 100 };

        var mandaty = new FormulaSainteLague().PodzielMandaty(glosy, 4);

        Assert.Equal(2, mandaty[1]);
        Assert.Equal(1, mandaty[2]);
        Assert.Equal(1, mandaty[3]);
    }

    [Fact]
    public void FawroryzujeMniejszeKomitetyBardziejNizDHondt()
    {
        // A=10, B=4, 2 mandaty.
        // D'Hondt (dzielniki 1,2): A/1=10, A/2=5, B/1=4 -> oba mandaty dla A (10, 5 > 4).
        // Sainte-Laguë (dzielniki 1,3): A/1=10, A/3=3.33, B/1=4 -> po jednym mandacie (10, 4 > 3.33).
        var glosy = new Dictionary<int, int> { [1] = 10, [2] = 4 };

        var mandatyDHondta = new FormulaDHondta().PodzielMandaty(glosy, 2);
        var mandatySainteLague = new FormulaSainteLague().PodzielMandaty(glosy, 2);

        Assert.Equal(0, mandatyDHondta[2]);
        Assert.Equal(1, mandatySainteLague[2]);
    }
}
