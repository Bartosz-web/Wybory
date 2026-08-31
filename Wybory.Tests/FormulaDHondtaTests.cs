using Wybory.Web.Uslugi.Formuly;

namespace Wybory.Tests;

public class FormulaDHondtaTests
{
    [Fact]
    public void DzieliMandatyProporcjonalnieWgIlorazow()
    {
        // A=537, B=421, C=205, 5 mandatów.
        // Ilorazy (dzielniki 1..5), 5 największych to: 537(A/1), 421(B/1), 268.5(A/2), 210.5(B/2), 205(C/1).
        var glosy = new Dictionary<int, int> { [1] = 537, [2] = 421, [3] = 205 };

        var mandaty = new FormulaDHondta().PodzielMandaty(glosy, 5);

        Assert.Equal(2, mandaty[1]);
        Assert.Equal(2, mandaty[2]);
        Assert.Equal(1, mandaty[3]);
    }

    [Fact]
    public void ZeroMandatowZwracaSameZera()
    {
        var glosy = new Dictionary<int, int> { [1] = 100, [2] = 50 };

        var mandaty = new FormulaDHondta().PodzielMandaty(glosy, 0);

        Assert.All(mandaty.Values, m => Assert.Equal(0, m));
    }

    [Fact]
    public void KomitetBezGlosowNieDostajeMandatu()
    {
        var glosy = new Dictionary<int, int> { [1] = 1000, [2] = 0 };

        var mandaty = new FormulaDHondta().PodzielMandaty(glosy, 3);

        Assert.Equal(3, mandaty[1]);
        Assert.Equal(0, mandaty[2]);
    }
}
