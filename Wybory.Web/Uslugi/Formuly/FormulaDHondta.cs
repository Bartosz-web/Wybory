namespace Wybory.Web.Uslugi.Formuly;

// Metoda D'Hondta — dzielniki 1, 2, 3, ... (używana w wyborach do Sejmu RP).
public class FormulaDHondta : IFormulaPodzialuMandatow
{
    public string Nazwa => "D'Hondt";

    public IReadOnlyDictionary<int, int> PodzielMandaty(IReadOnlyDictionary<int, int> glosyNaKomitety, int liczbaMandatow)
        => PodzialDzielnikowy.Podziel(glosyNaKomitety, liczbaMandatow, miejsce => miejsce);
}
