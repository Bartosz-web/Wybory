namespace Wybory.Web.Uslugi.Formuly;

// Strategia podziału mandatów między komitety w okręgu na podstawie liczby głosów (pkt 5).
public interface IFormulaPodzialuMandatow
{
    string Nazwa { get; }

    // glosyNaKomitety: komitetId -> liczba głosów. Zwraca: komitetId -> liczba mandatów.
    // Komitety z zerem mandatów mogą być pominięte w wyniku.
    IReadOnlyDictionary<int, int> PodzielMandaty(IReadOnlyDictionary<int, int> glosyNaKomitety, int liczbaMandatow);
}
