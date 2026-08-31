namespace Wybory.Web.Uslugi.Formuly;

// Metoda Sainte-Laguë — dzielniki 1, 3, 5, 7, ... (używana m.in. w wyborach do rad gmin).
public class FormulaSainteLague : IFormulaPodzialuMandatow
{
    public string Nazwa => "Sainte-Laguë";

    public IReadOnlyDictionary<int, int> PodzielMandaty(IReadOnlyDictionary<int, int> glosyNaKomitety, int liczbaMandatow)
        => PodzialDzielnikowy.Podziel(glosyNaKomitety, liczbaMandatow, miejsce => 2 * miejsce - 1);
}
